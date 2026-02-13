//! Shared atomic progress counter for parallel operations.
//!
//! Workers increment the counter atomically; consumers (CLI/Python) poll it
//! on a timer to drive progress bars without coupling the core library to any UI.

use std::sync::atomic::{AtomicU64, Ordering};
use std::sync::Arc;

/// Thread-safe progress counter shared between rayon workers and a UI poller.
pub type ProgressCounter = Arc<AtomicU64>;

/// Create a new zero-initialized progress counter.
pub fn new_counter() -> ProgressCounter {
    Arc::new(AtomicU64::new(0))
}

/// Increment the counter by one (called by each worker after completing a unit).
#[inline]
pub fn tick(counter: &ProgressCounter) {
    counter.fetch_add(1, Ordering::Relaxed);
}
