use std::path::PathBuf;
use std::sync::atomic::{AtomicBool, Ordering};
use std::sync::Arc;
use std::time::Instant;

use gpui::prelude::FluentBuilder as _;
use gpui::*;
use gpui_component::button::{Button, ButtonVariants as _};
use gpui_component::checkbox::Checkbox;
use gpui_component::input::{Input, InputState};
use gpui_component::progress::Progress;
use gpui_component::select::{Select, SelectState};
use gpui_component::{
    ActiveTheme, Disableable as _, IndexPath, Root, Sizable, TitleBar, h_flex, v_flex,
};

use crate::conversion;
use crate::file_list::{FileEntry, FileStatus};

#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub enum AppPhase {
    Idle,
    Converting,
    Done,
}

pub struct AppState {
    files: Vec<FileEntry>,
    output_dir: Option<PathBuf>,

    // Conversion options
    mz_select: Entity<SelectState<Vec<&'static str>>>,
    intensity_select: Entity<SelectState<Vec<&'static str>>>,
    compression_select: Entity<SelectState<Vec<&'static str>>>,
    write_index: bool,
    include_ms2: bool,
    threshold_input: Entity<InputState>,

    // Conversion state
    phase: AppPhase,
    progress_counter: Option<thermo_raw::ProgressCounter>,
    cancel_flag: Option<Arc<AtomicBool>>,
    total_scans: u64,
    convert_handle: Option<std::thread::JoinHandle<Vec<conversion::ConversionResult>>>,
    start_time: Option<Instant>,
    messages: Vec<(String, bool)>, // (text, is_error)
}

impl AppState {
    pub fn new(window: &mut Window, cx: &mut Context<Self>) -> Self {
        let mz_items = vec!["64-bit", "32-bit"];
        let mz_select = cx.new(|cx| {
            SelectState::new(mz_items, Some(IndexPath::default().row(0)), window, cx)
        });

        let intensity_items = vec!["32-bit", "64-bit"];
        let intensity_select = cx.new(|cx| {
            SelectState::new(intensity_items, Some(IndexPath::default().row(0)), window, cx)
        });

        let compression_items = vec!["Zlib", "None"];
        let compression_select = cx.new(|cx| {
            SelectState::new(
                compression_items,
                Some(IndexPath::default().row(0)),
                window,
                cx,
            )
        });

        let threshold_input = cx.new(|cx| {
            InputState::new(window, cx).placeholder("0")
        });

        Self {
            files: Vec::new(),
            output_dir: None,
            mz_select,
            intensity_select,
            compression_select,
            write_index: true,
            include_ms2: true,
            threshold_input,
            phase: AppPhase::Idle,
            progress_counter: None,
            cancel_flag: None,
            total_scans: 0,
            convert_handle: None,
            start_time: None,
            messages: Vec::new(),
        }
    }

    fn build_config(&self, cx: &App) -> thermo_raw_mzml::MzmlConfig {
        let mz_precision = match self.mz_select.read(cx).selected_value() {
            Some(&"32-bit") => thermo_raw_mzml::Precision::F32,
            _ => thermo_raw_mzml::Precision::F64,
        };
        let intensity_precision = match self.intensity_select.read(cx).selected_value() {
            Some(&"64-bit") => thermo_raw_mzml::Precision::F64,
            _ => thermo_raw_mzml::Precision::F32,
        };
        let compression = match self.compression_select.read(cx).selected_value() {
            Some(&"None") => thermo_raw_mzml::Compression::None,
            _ => thermo_raw_mzml::Compression::Zlib,
        };
        let threshold_text = self.threshold_input.read(cx).value().to_string();
        let intensity_threshold = threshold_text
            .trim()
            .parse::<f64>()
            .unwrap_or(0.0)
            .max(0.0);
        thermo_raw_mzml::MzmlConfig {
            mz_precision,
            intensity_precision,
            compression,
            write_index: self.write_index,
            include_ms2: self.include_ms2,
            intensity_threshold,
        }
    }

    fn add_files_action(&mut self, _: &ClickEvent, _window: &mut Window, cx: &mut Context<Self>) {
        cx.spawn(async move |this, cx| {
            let dialog = rfd::AsyncFileDialog::new()
                .add_filter("RAW files", &["raw", "RAW"])
                .set_title("Select RAW files");

            if let Some(handles) = dialog.pick_files().await {
                let paths: Vec<PathBuf> = handles.into_iter().map(|h| h.path().to_path_buf()).collect();
                this.update(cx, |this, cx| {
                    this.add_paths(&paths, cx);
                    this.scan_files_background(paths, cx);
                }).ok();
            }
        })
        .detach();
    }

    fn add_folder_action(
        &mut self,
        _: &ClickEvent,
        _window: &mut Window,
        cx: &mut Context<Self>,
    ) {
        cx.spawn(async move |this, cx| {
            let dialog = rfd::AsyncFileDialog::new().set_title("Select folder with RAW files");

            if let Some(handle) = dialog.pick_folder().await {
                let folder = handle.path().to_path_buf();

                // read_dir on background thread (can be slow on network drives)
                let folder_clone = folder.clone();
                let paths: Vec<PathBuf> = cx
                    .background_executor()
                    .spawn(async move {
                        std::fs::read_dir(&folder_clone)
                            .into_iter()
                            .flatten()
                            .filter_map(|e| e.ok())
                            .map(|e| e.path())
                            .filter(|p| {
                                p.extension()
                                    .is_some_and(|ext| ext.eq_ignore_ascii_case("raw"))
                            })
                            .collect()
                    })
                    .await;

                this.update(cx, |this, cx| {
                    if this.output_dir.is_none() {
                        this.output_dir = Some(folder);
                    }
                    this.add_paths(&paths, cx);
                    this.scan_files_background(paths, cx);
                }).ok();
            }
        })
        .detach();
    }

    /// Push placeholder entries for each path (no I/O, instant).
    fn add_paths(&mut self, paths: &[PathBuf], cx: &mut Context<Self>) {
        for path in paths {
            if self.files.iter().any(|f| f.path == *path) {
                continue;
            }
            let mut entry = FileEntry::new(path.clone());
            entry.status = FileStatus::Scanning;
            self.files.push(entry);
        }
        cx.notify();
    }

    /// Open each file on the background executor to read scan counts,
    /// then update the matching entries on the main thread.
    fn scan_files_background(&mut self, paths: Vec<PathBuf>, cx: &mut Context<Self>) {
        cx.spawn(async move |this, cx| {
            // Heavy I/O on background thread
            let results: Vec<(PathBuf, Result<u32, String>)> = cx
                .background_executor()
                .spawn(async move {
                    paths
                        .into_iter()
                        .map(|p| {
                            let result = thermo_raw::RawFile::open_mmap(&p)
                                .map(|raw| raw.n_scans())
                                .map_err(|e| format!("Cannot read: {e}"));
                            (p, result)
                        })
                        .collect()
                })
                .await;

            // Apply results back on the main thread
            this.update(cx, |this, cx| {
                for (path, result) in results {
                    if let Some(entry) = this.files.iter_mut().find(|f| f.path == path) {
                        match result {
                            Ok(n) => {
                                entry.n_scans = Some(n);
                                entry.status = FileStatus::Pending;
                                if this.output_dir.is_none() {
                                    if let Some(parent) = path.parent() {
                                        this.output_dir = Some(parent.to_path_buf());
                                    }
                                }
                            }
                            Err(e) => {
                                entry.status = FileStatus::Failed;
                                entry.error = Some(e);
                            }
                        }
                    }
                }
                cx.notify();
            })
            .ok();
        })
        .detach();
    }

    fn clear_files(&mut self, _: &ClickEvent, _window: &mut Window, cx: &mut Context<Self>) {
        self.files.clear();
        self.phase = AppPhase::Idle;
        self.messages.clear();
        cx.notify();
    }

    fn change_output_dir(
        &mut self,
        _: &ClickEvent,
        _window: &mut Window,
        cx: &mut Context<Self>,
    ) {
        cx.spawn(async move |this, cx| {
            let dialog = rfd::AsyncFileDialog::new().set_title("Select output directory");
            if let Some(handle) = dialog.pick_folder().await {
                this.update(cx, |this, cx| {
                    this.output_dir = Some(handle.path().to_path_buf());
                    cx.notify();
                }).ok();
            }
        })
        .detach();
    }

    fn start_conversion(
        &mut self,
        _: &ClickEvent,
        _window: &mut Window,
        cx: &mut Context<Self>,
    ) {
        let Some(output_dir) = self.output_dir.clone() else {
            self.messages
                .push(("No output directory selected.".into(), true));
            cx.notify();
            return;
        };

        // Collect files to convert
        let convertible: Vec<(usize, PathBuf)> = self
            .files
            .iter()
            .enumerate()
            .filter(|(_, f)| f.status == FileStatus::Pending && f.n_scans.is_some())
            .map(|(i, f)| (i, f.path.clone()))
            .collect();

        if convertible.is_empty() {
            self.messages.push(("No files to convert.".into(), true));
            cx.notify();
            return;
        }

        // Calculate total scans
        self.total_scans = self
            .files
            .iter()
            .filter(|f| f.status == FileStatus::Pending && f.n_scans.is_some())
            .map(|f| f.n_scans.unwrap_or(0) as u64)
            .sum();

        // Mark files as converting
        for (i, _) in &convertible {
            self.files[*i].status = FileStatus::Converting;
        }

        let config = self.build_config(cx);
        let counter = thermo_raw::progress::new_counter();
        let cancel = Arc::new(AtomicBool::new(false));

        self.progress_counter = Some(Arc::clone(&counter));
        self.cancel_flag = Some(Arc::clone(&cancel));
        self.phase = AppPhase::Converting;
        self.start_time = Some(Instant::now());
        self.messages.clear();

        let handle =
            conversion::spawn_conversion(convertible, output_dir, config, counter, cancel);
        self.convert_handle = Some(handle);

        // Start polling loop
        cx.spawn(async move |this, cx| {
            loop {
                cx.background_executor()
                    .timer(std::time::Duration::from_millis(50))
                    .await;

                let should_stop = this
                    .update(cx, |this, cx| {
                        let handle = this.convert_handle.as_ref();
                        let finished = handle.is_some_and(|h| h.is_finished());

                        if finished {
                            let handle = this.convert_handle.take().unwrap();
                            let results = handle.join().unwrap_or_default();
                            let elapsed = this
                                .start_time
                                .map(|s| s.elapsed().as_secs_f64())
                                .unwrap_or(0.0);

                            let mut success = 0usize;
                            let mut failed = 0usize;
                            for r in &results {
                                match &r.result {
                                    Ok(()) => {
                                        if let Some(f) = this.files.get_mut(r.index) {
                                            f.status = FileStatus::Done;
                                        }
                                        success += 1;
                                    }
                                    Err(e) => {
                                        if let Some(f) = this.files.get_mut(r.index) {
                                            f.status = FileStatus::Failed;
                                            f.error = Some(e.clone());
                                        }
                                        failed += 1;
                                    }
                                }
                            }

                            // Mark any remaining Converting files as cancelled
                            for f in &mut this.files {
                                if f.status == FileStatus::Converting {
                                    f.status = FileStatus::Pending;
                                }
                            }

                            let mut msg =
                                format!("Converted {success} file(s) in {elapsed:.1}s.");
                            if failed > 0 {
                                msg.push_str(&format!(" {failed} failed."));
                            }
                            this.messages.push((msg, false));
                            this.phase = AppPhase::Done;
                            this.progress_counter = None;
                            this.cancel_flag = None;
                            cx.notify();
                            return true;
                        }

                        cx.notify();
                        false
                    })
                    .unwrap_or(true);

                if should_stop {
                    break;
                }
            }
        })
        .detach();

        cx.notify();
    }

    fn cancel_conversion(
        &mut self,
        _: &ClickEvent,
        _window: &mut Window,
        cx: &mut Context<Self>,
    ) {
        if let Some(flag) = &self.cancel_flag {
            flag.store(true, Ordering::Relaxed);
        }
        self.messages
            .push(("Cancelling after current file...".into(), false));
        cx.notify();
    }

    fn progress_fraction(&self) -> f32 {
        if self.total_scans == 0 {
            return 0.0;
        }
        let done = self
            .progress_counter
            .as_ref()
            .map(|c| c.load(Ordering::Relaxed))
            .unwrap_or(0);
        (done as f64 / self.total_scans as f64 * 100.0).min(100.0) as f32
    }

    fn can_convert(&self) -> bool {
        self.phase != AppPhase::Converting
            && self.output_dir.is_some()
            && self
                .files
                .iter()
                .any(|f| f.status == FileStatus::Pending && f.n_scans.is_some())
    }

    fn render_header(&self, cx: &Context<Self>) -> impl IntoElement {
        h_flex()
            .px_4()
            .py_3()
            .gap_3()
            .items_center()
            .border_b_1()
            .border_color(cx.theme().border)
            .child(
                div()
                    .text_base()
                    .font_weight(FontWeight::SEMIBOLD)
                    .text_color(cx.theme().foreground)
                    .child("RAW to mzML Converter"),
            )
            .child(
                div()
                    .text_xs()
                    .text_color(cx.theme().muted_foreground)
                    .child(format!("v{}", env!("CARGO_PKG_VERSION"))),
            )
    }

    fn render_file_list(&self, cx: &Context<Self>) -> impl IntoElement {
        div()
            .id("file-list")
            .flex_1()
            .overflow_y_scroll()
            .child(v_flex().children(
                self.files
                    .iter()
                    .enumerate()
                    .map(|(i, entry)| self.render_file_row(i, entry, cx)),
            ))
    }

    fn render_file_row(
        &self,
        index: usize,
        entry: &FileEntry,
        cx: &Context<Self>,
    ) -> impl IntoElement {
        let status_color = match entry.status {
            FileStatus::Scanning => cx.theme().muted_foreground,
            FileStatus::Pending => cx.theme().muted_foreground,
            FileStatus::Converting => cx.theme().blue,
            FileStatus::Done => cx.theme().green,
            FileStatus::Failed => cx.theme().red,
        };

        let error_text = entry.error.clone();

        h_flex()
            .id(("file-row", index))
            .px_4()
            .py_1()
            .gap_3()
            .items_center()
            .border_b_1()
            .border_color(cx.theme().border.opacity(0.5))
            .hover(|s| s.bg(cx.theme().muted.opacity(0.3)))
            .child(div().w_4().h_4().rounded(px(2.)).bg(status_color))
            .child(
                v_flex()
                    .flex_1()
                    .child(
                        div()
                            .text_sm()
                            .text_color(cx.theme().foreground)
                            .truncate()
                            .child(entry.name.clone()),
                    )
                    .when_some(error_text, |this, err| {
                        this.child(
                            div()
                                .text_xs()
                                .text_color(cx.theme().red)
                                .truncate()
                                .child(err),
                        )
                    }),
            )
            .child(
                div()
                    .text_xs()
                    .text_color(cx.theme().muted_foreground)
                    .child(entry.size_display()),
            )
            .child(
                div()
                    .text_xs()
                    .text_color(cx.theme().muted_foreground)
                    .w(px(60.))
                    .child(if entry.status == FileStatus::Scanning {
                        "...".into()
                    } else {
                        entry
                            .n_scans
                            .map(|n| format!("{n} scans"))
                            .unwrap_or_else(|| "N/A".into())
                    }),
            )
            .child(
                div()
                    .text_xs()
                    .w(px(80.))
                    .text_color(status_color)
                    .child(entry.status_label()),
            )
    }

    fn render_action_buttons(&self, cx: &Context<Self>) -> impl IntoElement {
        let is_converting = self.phase == AppPhase::Converting;
        h_flex()
            .px_4()
            .py_2()
            .gap_2()
            .border_b_1()
            .border_color(cx.theme().border)
            .child(
                Button::new("add-files")
                    .small()
                    .outline()
                    .label("Add Files...")
                    .disabled(is_converting)
                    .on_click(cx.listener(Self::add_files_action)),
            )
            .child(
                Button::new("add-folder")
                    .small()
                    .outline()
                    .label("Add Folder...")
                    .disabled(is_converting)
                    .on_click(cx.listener(Self::add_folder_action)),
            )
            .child(
                Button::new("clear")
                    .small()
                    .outline()
                    .label("Clear")
                    .disabled(is_converting || self.files.is_empty())
                    .on_click(cx.listener(Self::clear_files)),
            )
            .child(
                div()
                    .flex_1()
                    .text_xs()
                    .text_color(cx.theme().muted_foreground)
                    .text_right()
                    .child(format!("{} file(s)", self.files.len())),
            )
    }

    fn render_output_row(&self, cx: &Context<Self>) -> impl IntoElement {
        let dir_display = self
            .output_dir
            .as_ref()
            .map(|p| p.to_string_lossy().into_owned())
            .unwrap_or_else(|| "Not set (same as input)".into());

        h_flex()
            .px_4()
            .py_2()
            .gap_3()
            .items_center()
            .border_b_1()
            .border_color(cx.theme().border)
            .child(
                div()
                    .text_sm()
                    .text_color(cx.theme().foreground)
                    .font_weight(FontWeight::MEDIUM)
                    .child("Output:"),
            )
            .child(
                div()
                    .flex_1()
                    .text_sm()
                    .text_color(cx.theme().muted_foreground)
                    .truncate()
                    .child(dir_display),
            )
            .child(
                Button::new("change-output")
                    .xsmall()
                    .outline()
                    .label("Change...")
                    .disabled(self.phase == AppPhase::Converting)
                    .on_click(cx.listener(Self::change_output_dir)),
            )
    }

    fn render_options_row(&self, cx: &Context<Self>) -> impl IntoElement {
        let is_converting = self.phase == AppPhase::Converting;
        v_flex()
            .border_b_1()
            .border_color(cx.theme().border)
            .child(
                h_flex()
                    .px_4()
                    .py_2()
                    .gap_4()
                    .items_center()
                    .child(
                        h_flex()
                            .gap_1()
                            .items_center()
                            .child(
                                div()
                                    .text_xs()
                                    .text_color(cx.theme().muted_foreground)
                                    .child("m/z:"),
                            )
                            .child(Select::new(&self.mz_select).xsmall().w(px(80.)).disabled(is_converting)),
                    )
                    .child(
                        h_flex()
                            .gap_1()
                            .items_center()
                            .child(
                                div()
                                    .text_xs()
                                    .text_color(cx.theme().muted_foreground)
                                    .child("Intensity:"),
                            )
                            .child(
                                Select::new(&self.intensity_select)
                                    .xsmall()
                                    .w(px(80.))
                                    .disabled(is_converting),
                            ),
                    )
                    .child(
                        h_flex()
                            .gap_1()
                            .items_center()
                            .child(
                                div()
                                    .text_xs()
                                    .text_color(cx.theme().muted_foreground)
                                    .child("Compression:"),
                            )
                            .child(
                                Select::new(&self.compression_select)
                                    .xsmall()
                                    .w(px(80.))
                                    .disabled(is_converting),
                            ),
                    )
                    .child({
                        let write_index = self.write_index;
                        Checkbox::new("indexed")
                            .small()
                            .label("Indexed")
                            .checked(write_index)
                            .disabled(is_converting)
                            .on_click(cx.listener(move |this, checked: &bool, _window, cx| {
                                this.write_index = *checked;
                                cx.notify();
                            }))
                    }),
            )
            .child(
                h_flex()
                    .px_4()
                    .py_2()
                    .gap_4()
                    .items_center()
                    .child({
                        let include_ms2 = self.include_ms2;
                        Checkbox::new("include-ms2")
                            .small()
                            .label("Include MS2")
                            .checked(include_ms2)
                            .disabled(is_converting)
                            .on_click(cx.listener(move |this, checked: &bool, _window, cx| {
                                this.include_ms2 = *checked;
                                cx.notify();
                            }))
                    })
                    .child(
                        h_flex()
                            .gap_1()
                            .items_center()
                            .child(
                                div()
                                    .text_xs()
                                    .text_color(cx.theme().muted_foreground)
                                    .child("Min intensity:"),
                            )
                            .child(
                                Input::new(&self.threshold_input)
                                    .xsmall()
                                    .w(px(80.))
                                    .disabled(is_converting),
                            ),
                    ),
            )
    }

    fn render_bottom_bar(&self, cx: &Context<Self>) -> impl IntoElement {
        let is_converting = self.phase == AppPhase::Converting;
        let progress = self.progress_fraction();

        v_flex()
            .px_4()
            .py_2()
            .gap_2()
            .border_t_1()
            .border_color(cx.theme().border)
            .bg(cx.theme().tab_bar)
            .children(self.messages.iter().map(|(msg, is_err)| {
                div()
                    .text_xs()
                    .text_color(if *is_err {
                        cx.theme().red
                    } else {
                        cx.theme().green
                    })
                    .child(msg.clone())
            }))
            .child(
                h_flex()
                    .gap_3()
                    .items_center()
                    .child(
                        Button::new("convert")
                            .small()
                            .primary()
                            .label("Convert")
                            .disabled(!self.can_convert())
                            .on_click(cx.listener(Self::start_conversion)),
                    )
                    .when(is_converting, |this: Div| {
                        this.child(Progress::new().flex_1().h(px(8.)).value(progress))
                            .child(
                                div()
                                    .text_xs()
                                    .text_color(cx.theme().muted_foreground)
                                    .child(format!("{progress:.0}%")),
                            )
                            .child(
                                Button::new("cancel")
                                    .xsmall()
                                    .outline()
                                    .label("Cancel")
                                    .on_click(cx.listener(Self::cancel_conversion)),
                            )
                    }),
            )
    }
}

impl Render for AppState {
    fn render(&mut self, window: &mut Window, cx: &mut Context<Self>) -> impl IntoElement {
        let dialog_layer = Root::render_dialog_layer(window, cx);

        v_flex()
            .size_full()
            .font_family(cx.theme().font_family.clone())
            .bg(cx.theme().background)
            .text_color(cx.theme().foreground)
            .child(TitleBar::new().child("RAW to mzML Converter"))
            .child(self.render_header(cx))
            .child(self.render_action_buttons(cx))
            .child(self.render_file_list(cx))
            .child(self.render_output_row(cx))
            .child(self.render_options_row(cx))
            .child(self.render_bottom_bar(cx))
            .children(dialog_layer)
    }
}
