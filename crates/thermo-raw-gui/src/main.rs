mod app;
mod conversion;
mod file_list;

use gpui::*;
use gpui_component::{Root, TitleBar};
use gpui_component_assets::Assets;

actions!(converter, [Quit]);

fn main() {
    let app = Application::new().with_assets(Assets);

    app.run(move |cx| {
        gpui_component::init(cx);

        cx.bind_keys([
            #[cfg(target_os = "macos")]
            KeyBinding::new("cmd-q", Quit, None),
            #[cfg(not(target_os = "macos"))]
            KeyBinding::new("alt-f4", Quit, None),
        ]);

        cx.on_action(|_: &Quit, cx: &mut App| {
            cx.quit();
        });

        let window_options = WindowOptions {
            titlebar: Some(TitleBar::title_bar_options()),
            window_bounds: Some(WindowBounds::centered(size(px(700.), px(520.)), cx)),
            window_min_size: Some(size(px(500.), px(400.))),
            ..Default::default()
        };

        cx.spawn(async move |cx| {
            cx.open_window(window_options, |window, cx| {
                window.activate_window();
                window.set_window_title("RAW to mzML Converter");

                let view = cx.new(|cx| app::AppState::new(window, cx));
                cx.new(|cx| Root::new(view, window, cx))
            })?;

            Ok::<_, anyhow::Error>(())
        })
        .detach();
    });
}
