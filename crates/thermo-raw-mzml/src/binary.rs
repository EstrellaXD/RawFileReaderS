//! Binary data encoding pipeline for mzML binary arrays.
//!
//! Encodes numeric arrays through the standard mzML pipeline:
//! `f64 array -> precision cast (f32/f64) -> LE bytes -> compress -> base64`

use crate::{Compression, Precision};
use base64::Engine;
use flate2::write::ZlibEncoder;
use std::io::Write;

/// Encode a floating-point array to a base64 string for mzML binary data.
///
/// The pipeline is:
/// 1. Cast to target precision (f32 or f64)
/// 2. Convert to little-endian bytes
/// 3. Optionally compress with zlib
/// 4. Encode as base64
pub fn encode_array(data: &[f64], precision: Precision, compression: Compression) -> String {
    let raw_bytes = match precision {
        Precision::F64 => {
            let mut buf = Vec::with_capacity(data.len() * 8);
            for &val in data {
                buf.extend_from_slice(&val.to_le_bytes());
            }
            buf
        }
        Precision::F32 => {
            let mut buf = Vec::with_capacity(data.len() * 4);
            for &val in data {
                buf.extend_from_slice(&(val as f32).to_le_bytes());
            }
            buf
        }
    };

    let compressed = match compression {
        Compression::Zlib => {
            let mut encoder = ZlibEncoder::new(Vec::new(), flate2::Compression::default());
            encoder.write_all(&raw_bytes).expect("zlib write failed");
            encoder.finish().expect("zlib finish failed")
        }
        Compression::None => raw_bytes,
    };

    base64::engine::general_purpose::STANDARD.encode(&compressed)
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn test_encode_f64_no_compression() {
        let data = [100.0, 200.0, 300.0];
        let encoded = encode_array(&data, Precision::F64, Compression::None);
        let decoded = base64::engine::general_purpose::STANDARD
            .decode(&encoded)
            .unwrap();
        assert_eq!(decoded.len(), 24); // 3 * 8 bytes
                                       // Verify round-trip
        let val = f64::from_le_bytes(decoded[0..8].try_into().unwrap());
        assert!((val - 100.0).abs() < f64::EPSILON);
    }

    #[test]
    fn test_encode_f32_no_compression() {
        let data = [100.0, 200.0, 300.0];
        let encoded = encode_array(&data, Precision::F32, Compression::None);
        let decoded = base64::engine::general_purpose::STANDARD
            .decode(&encoded)
            .unwrap();
        assert_eq!(decoded.len(), 12); // 3 * 4 bytes
        let val = f32::from_le_bytes(decoded[0..4].try_into().unwrap());
        assert!((val - 100.0).abs() < 0.01);
    }

    #[test]
    fn test_encode_zlib_decompresses_correctly() {
        use flate2::read::ZlibDecoder;
        use std::io::Read;

        let data = [1.5, 2.5, 3.5, 4.5, 5.5];
        let encoded = encode_array(&data, Precision::F64, Compression::Zlib);
        let compressed = base64::engine::general_purpose::STANDARD
            .decode(&encoded)
            .unwrap();

        let mut decoder = ZlibDecoder::new(&compressed[..]);
        let mut decompressed = Vec::new();
        decoder.read_to_end(&mut decompressed).unwrap();

        assert_eq!(decompressed.len(), 40); // 5 * 8 bytes
        let val = f64::from_le_bytes(decompressed[0..8].try_into().unwrap());
        assert!((val - 1.5).abs() < f64::EPSILON);
    }

    #[test]
    fn test_encode_empty_array() {
        let encoded = encode_array(&[], Precision::F64, Compression::None);
        let decoded = base64::engine::general_purpose::STANDARD
            .decode(&encoded)
            .unwrap();
        assert!(decoded.is_empty());
    }
}
