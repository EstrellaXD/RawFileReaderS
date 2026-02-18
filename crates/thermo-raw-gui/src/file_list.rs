use std::path::PathBuf;

#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub enum FileStatus {
    Scanning,
    Pending,
    Converting,
    Done,
    Failed,
}

#[derive(Debug, Clone)]
pub struct FileEntry {
    pub path: PathBuf,
    pub name: String,
    pub size: u64,
    pub n_scans: Option<u32>,
    pub status: FileStatus,
    pub error: Option<String>,
}

impl FileEntry {
    pub fn new(path: PathBuf) -> Self {
        let name = path
            .file_name()
            .map(|n| n.to_string_lossy().into_owned())
            .unwrap_or_default();
        let size = std::fs::metadata(&path).map(|m| m.len()).unwrap_or(0);
        Self {
            path,
            name,
            size,
            n_scans: None,
            status: FileStatus::Pending,
            error: None,
        }
    }

    pub fn size_display(&self) -> String {
        const MB: u64 = 1024 * 1024;
        const GB: u64 = MB * 1024;
        if self.size >= GB {
            format!("{:.1} GB", self.size as f64 / GB as f64)
        } else {
            format!("{:.1} MB", self.size as f64 / MB as f64)
        }
    }

    pub fn status_label(&self) -> &'static str {
        match self.status {
            FileStatus::Scanning => "Scanning...",
            FileStatus::Pending => "Pending",
            FileStatus::Converting => "Converting...",
            FileStatus::Done => "Done",
            FileStatus::Failed => "Failed",
        }
    }
}
