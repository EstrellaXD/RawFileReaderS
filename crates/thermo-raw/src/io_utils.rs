//! Binary reading utilities for parsing Thermo RAW structures.

use crate::RawError;
use byteorder::{LittleEndian, ReadBytesExt};
use std::io::Cursor;

/// A cursor wrapper for reading binary data from a byte slice.
pub struct BinaryReader<'a> {
    cursor: Cursor<&'a [u8]>,
}

impl<'a> BinaryReader<'a> {
    pub fn new(data: &'a [u8]) -> Self {
        Self {
            cursor: Cursor::new(data),
        }
    }

    /// Create a reader starting at a specific offset.
    pub fn at_offset(data: &'a [u8], offset: u64) -> Self {
        let mut cursor = Cursor::new(data);
        cursor.set_position(offset);
        Self { cursor }
    }

    pub fn position(&self) -> u64 {
        self.cursor.position()
    }

    pub fn set_position(&mut self, pos: u64) {
        self.cursor.set_position(pos);
    }

    pub fn remaining(&self) -> usize {
        let pos = self.cursor.position() as usize;
        let len = self.cursor.get_ref().len();
        len.saturating_sub(pos)
    }

    pub fn read_u8(&mut self) -> Result<u8, RawError> {
        self.check_remaining(1, "read_u8")?;
        self.cursor.read_u8().map_err(RawError::Io)
    }

    pub fn read_u16(&mut self) -> Result<u16, RawError> {
        self.check_remaining(2, "read_u16")?;
        self.cursor.read_u16::<LittleEndian>().map_err(RawError::Io)
    }

    pub fn read_u32(&mut self) -> Result<u32, RawError> {
        self.check_remaining(4, "read_u32")?;
        self.cursor.read_u32::<LittleEndian>().map_err(RawError::Io)
    }

    pub fn read_i32(&mut self) -> Result<i32, RawError> {
        self.check_remaining(4, "read_i32")?;
        self.cursor.read_i32::<LittleEndian>().map_err(RawError::Io)
    }

    pub fn read_u64(&mut self) -> Result<u64, RawError> {
        self.check_remaining(8, "read_u64")?;
        self.cursor.read_u64::<LittleEndian>().map_err(RawError::Io)
    }

    pub fn read_f32(&mut self) -> Result<f32, RawError> {
        self.check_remaining(4, "read_f32")?;
        self.cursor.read_f32::<LittleEndian>().map_err(RawError::Io)
    }

    pub fn read_f64(&mut self) -> Result<f64, RawError> {
        self.check_remaining(8, "read_f64")?;
        self.cursor.read_f64::<LittleEndian>().map_err(RawError::Io)
    }

    fn check_remaining(&self, needed: usize, op: &str) -> Result<(), RawError> {
        let remaining = self.remaining();
        if remaining < needed {
            return Err(RawError::CorruptedData(format!(
                "{}: need {} bytes at offset {}, but only {} remaining (file size: {})",
                op,
                needed,
                self.cursor.position(),
                remaining,
                self.cursor.get_ref().len()
            )));
        }
        Ok(())
    }

    /// Read N bytes into a new Vec.
    pub fn read_bytes(&mut self, n: usize) -> Result<Vec<u8>, RawError> {
        self.check_remaining(n, "read_bytes")?;
        let pos = self.cursor.position() as usize;
        let data = self.cursor.get_ref();
        let result = data[pos..pos + n].to_vec();
        self.cursor.set_position((pos + n) as u64);
        Ok(result)
    }

    /// Skip N bytes.
    pub fn skip(&mut self, n: usize) -> Result<(), RawError> {
        self.check_remaining(n, "skip")?;
        self.cursor.set_position(self.cursor.position() + n as u64);
        Ok(())
    }

    /// Read a fixed-size UTF-16LE string (size in bytes, not chars).
    pub fn read_utf16_fixed(&mut self, byte_len: usize) -> Result<String, RawError> {
        let bytes = self.read_bytes(byte_len)?;
        let u16s: Vec<u16> = bytes
            .chunks_exact(2)
            .map(|c| u16::from_le_bytes([c[0], c[1]]))
            .collect();
        Ok(String::from_utf16_lossy(&u16s)
            .trim_end_matches('\0')
            .to_string())
    }

    /// Skip a PascalStringWin32 without allocating: read i32 length prefix, skip length * 2 bytes.
    pub fn skip_pascal_string(&mut self) -> Result<(), RawError> {
        let len = self.read_i32()?;
        if len < 0 {
            return Err(RawError::CorruptedData(format!(
                "PascalString with negative length: {}",
                len
            )));
        }
        if len > 0 {
            self.skip((len as usize) * 2)?;
        }
        Ok(())
    }

    /// Read a PascalStringWin32: i32 length prefix, then length * 2 bytes of UTF-16LE.
    pub fn read_pascal_string(&mut self) -> Result<String, RawError> {
        let len = self.read_i32()?;
        if len < 0 {
            return Err(RawError::CorruptedData(format!(
                "PascalString with negative length: {}",
                len
            )));
        }
        if len == 0 {
            return Ok(String::new());
        }
        let byte_len = (len as usize) * 2;
        self.read_utf16_fixed(byte_len)
    }

    /// Read an array of f32 values.
    pub fn read_f32_array(&mut self, count: usize) -> Result<Vec<f32>, RawError> {
        let mut result = Vec::with_capacity(count);
        for _ in 0..count {
            result.push(self.read_f32()?);
        }
        Ok(result)
    }

    /// Read an array of f64 values.
    pub fn read_f64_array(&mut self, count: usize) -> Result<Vec<f64>, RawError> {
        let mut result = Vec::with_capacity(count);
        for _ in 0..count {
            result.push(self.read_f64()?);
        }
        Ok(result)
    }

    /// Get a slice of the underlying data at the current position.
    pub fn slice(&self, len: usize) -> Result<&'a [u8], RawError> {
        self.check_remaining(len, "slice")?;
        let pos = self.cursor.position() as usize;
        Ok(&self.cursor.get_ref()[pos..pos + len])
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn test_read_primitives() {
        let data: Vec<u8> = vec![
            0x01, 0xA1, // u16: 0xA101
            0x39, 0x00, 0x00, 0x00, // u32: 57
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x59, 0x40, // f64: 100.0
        ];
        let mut reader = BinaryReader::new(&data);
        assert_eq!(reader.read_u16().unwrap(), 0xA101);
        assert_eq!(reader.read_u32().unwrap(), 57);
        assert_eq!(reader.read_f64().unwrap(), 100.0);
    }

    #[test]
    fn test_read_pascal_string() {
        // PascalStringWin32: i32 length=3, then "abc" in UTF-16LE
        let data: Vec<u8> = vec![
            0x03, 0x00, 0x00, 0x00, // length: 3
            0x61, 0x00, // 'a'
            0x62, 0x00, // 'b'
            0x63, 0x00, // 'c'
        ];
        let mut reader = BinaryReader::new(&data);
        assert_eq!(reader.read_pascal_string().unwrap(), "abc");
    }

    #[test]
    fn test_read_utf16_fixed_with_nulls() {
        // "Hi" followed by null padding (8 bytes total = 4 UTF-16 chars)
        let data: Vec<u8> = vec![
            0x48, 0x00, // 'H'
            0x69, 0x00, // 'i'
            0x00, 0x00, // null
            0x00, 0x00, // null
        ];
        let mut reader = BinaryReader::new(&data);
        assert_eq!(reader.read_utf16_fixed(8).unwrap(), "Hi");
    }

    #[test]
    fn test_at_offset() {
        let data: Vec<u8> = vec![0x00, 0x00, 0x00, 0x00, 0x42, 0x00, 0x00, 0x00];
        let mut reader = BinaryReader::at_offset(&data, 4);
        assert_eq!(reader.read_u32().unwrap(), 0x42);
    }

    #[test]
    fn test_skip_and_remaining() {
        let data: Vec<u8> = vec![0; 100];
        let mut reader = BinaryReader::new(&data);
        assert_eq!(reader.remaining(), 100);
        reader.skip(50).unwrap();
        assert_eq!(reader.remaining(), 50);
        assert_eq!(reader.position(), 50);
    }

    // Bounds-checking tests for check_remaining()

    #[test]
    fn test_read_u32_insufficient_bytes() {
        // Only 3 bytes available, but u32 needs 4
        let data: Vec<u8> = vec![0x01, 0x02, 0x03];
        let mut reader = BinaryReader::new(&data);
        let err = reader.read_u32().unwrap_err();
        match err {
            RawError::CorruptedData(msg) => {
                assert!(msg.contains("read_u32"));
                assert!(msg.contains("need 4 bytes"));
                assert!(msg.contains("only 3 remaining"));
                assert!(msg.contains("file size: 3"));
            }
            _ => panic!("Expected CorruptedData error, got {:?}", err),
        }
    }

    #[test]
    fn test_read_u64_insufficient_bytes() {
        // Only 5 bytes available, but u64 needs 8
        let data: Vec<u8> = vec![0x01, 0x02, 0x03, 0x04, 0x05];
        let mut reader = BinaryReader::new(&data);
        let err = reader.read_u64().unwrap_err();
        match err {
            RawError::CorruptedData(msg) => {
                assert!(msg.contains("read_u64"));
                assert!(msg.contains("need 8 bytes"));
                assert!(msg.contains("only 5 remaining"));
            }
            _ => panic!("Expected CorruptedData error, got {:?}", err),
        }
    }

    #[test]
    fn test_read_u16_insufficient_bytes() {
        // Only 1 byte available, but u16 needs 2
        let data: Vec<u8> = vec![0x01];
        let mut reader = BinaryReader::new(&data);
        let err = reader.read_u16().unwrap_err();
        match err {
            RawError::CorruptedData(msg) => {
                assert!(msg.contains("read_u16"));
                assert!(msg.contains("need 2 bytes"));
                assert!(msg.contains("only 1 remaining"));
            }
            _ => panic!("Expected CorruptedData error, got {:?}", err),
        }
    }

    #[test]
    fn test_read_u8_insufficient_bytes() {
        // Empty buffer
        let data: Vec<u8> = vec![];
        let mut reader = BinaryReader::new(&data);
        let err = reader.read_u8().unwrap_err();
        match err {
            RawError::CorruptedData(msg) => {
                assert!(msg.contains("read_u8"));
                assert!(msg.contains("need 1 bytes"));
                assert!(msg.contains("only 0 remaining"));
            }
            _ => panic!("Expected CorruptedData error, got {:?}", err),
        }
    }

    #[test]
    fn test_read_i32_insufficient_bytes() {
        // Only 2 bytes available, but i32 needs 4
        let data: Vec<u8> = vec![0x01, 0x02];
        let mut reader = BinaryReader::new(&data);
        let err = reader.read_i32().unwrap_err();
        match err {
            RawError::CorruptedData(msg) => {
                assert!(msg.contains("read_i32"));
                assert!(msg.contains("need 4 bytes"));
                assert!(msg.contains("only 2 remaining"));
            }
            _ => panic!("Expected CorruptedData error, got {:?}", err),
        }
    }

    #[test]
    fn test_read_f32_insufficient_bytes() {
        // Only 3 bytes available, but f32 needs 4
        let data: Vec<u8> = vec![0x01, 0x02, 0x03];
        let mut reader = BinaryReader::new(&data);
        let err = reader.read_f32().unwrap_err();
        match err {
            RawError::CorruptedData(msg) => {
                assert!(msg.contains("read_f32"));
                assert!(msg.contains("need 4 bytes"));
            }
            _ => panic!("Expected CorruptedData error, got {:?}", err),
        }
    }

    #[test]
    fn test_read_f64_insufficient_bytes() {
        // Only 6 bytes available, but f64 needs 8
        let data: Vec<u8> = vec![0x01, 0x02, 0x03, 0x04, 0x05, 0x06];
        let mut reader = BinaryReader::new(&data);
        let err = reader.read_f64().unwrap_err();
        match err {
            RawError::CorruptedData(msg) => {
                assert!(msg.contains("read_f64"));
                assert!(msg.contains("need 8 bytes"));
                assert!(msg.contains("only 6 remaining"));
            }
            _ => panic!("Expected CorruptedData error, got {:?}", err),
        }
    }

    #[test]
    fn test_read_with_offset_insufficient_bytes() {
        // 10 bytes total, start at offset 8, try to read u32 (needs 4 bytes)
        let data: Vec<u8> = vec![0; 10];
        let mut reader = BinaryReader::at_offset(&data, 8);
        let err = reader.read_u32().unwrap_err();
        match err {
            RawError::CorruptedData(msg) => {
                assert!(msg.contains("read_u32"));
                assert!(msg.contains("need 4 bytes"));
                assert!(msg.contains("offset 8"));
                assert!(msg.contains("only 2 remaining"));
            }
            _ => panic!("Expected CorruptedData error, got {:?}", err),
        }
    }

    #[test]
    fn test_reads_succeed_with_sufficient_bytes() {
        // Verify all read operations succeed when sufficient bytes are present
        let data: Vec<u8> = vec![
            0x42, // u8
            0x01, 0x02, // u16
            0x03, 0x04, 0x05, 0x06, // u32
            0x07, 0x08, 0x09, 0x0A, // i32
            0x00, 0x00, 0x80, 0x3F, // f32: 1.0
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xF0, 0x3F, // f64: 1.0
        ];
        let mut reader = BinaryReader::new(&data);

        assert_eq!(reader.read_u8().unwrap(), 0x42);
        assert_eq!(reader.read_u16().unwrap(), 0x0201);
        assert_eq!(reader.read_u32().unwrap(), 0x06050403);
        assert_eq!(reader.read_i32().unwrap(), 0x0A090807);
        assert_eq!(reader.read_f32().unwrap(), 1.0);
        assert_eq!(reader.read_f64().unwrap(), 1.0);
    }

    #[test]
    fn test_sequential_reads_track_position_correctly() {
        let data: Vec<u8> = vec![0; 20];
        let mut reader = BinaryReader::new(&data);

        reader.read_u32().unwrap(); // consume 4 bytes
        assert_eq!(reader.remaining(), 16);

        reader.read_u64().unwrap(); // consume 8 bytes
        assert_eq!(reader.remaining(), 8);

        // Try to read u64 (needs 8 bytes, exactly 8 remaining - should succeed)
        assert!(reader.read_u64().is_ok());
        assert_eq!(reader.remaining(), 0);

        // Try to read u8 (needs 1 byte, but 0 remaining - should fail)
        let err = reader.read_u8().unwrap_err();
        match err {
            RawError::CorruptedData(msg) => {
                assert!(msg.contains("only 0 remaining"));
            }
            _ => panic!("Expected CorruptedData error, got {:?}", err),
        }
    }

    // Tests for skip_pascal_string()

    #[test]
    fn test_skip_pascal_string_with_valid_string() {
        // PascalStringWin32: length=5, then "Hello" in UTF-16LE
        let data: Vec<u8> = vec![
            0x05, 0x00, 0x00, 0x00, // length: 5
            0x48, 0x00, // 'H'
            0x65, 0x00, // 'e'
            0x6C, 0x00, // 'l'
            0x6C, 0x00, // 'l'
            0x6F, 0x00, // 'o'
            0x42, 0x00, 0x00, 0x00, // trailing u32: 66
        ];
        let mut reader = BinaryReader::new(&data);

        let start_pos = reader.position();
        assert_eq!(start_pos, 0);

        reader.skip_pascal_string().unwrap();

        // Should have skipped 4 bytes (length prefix) + 10 bytes (5 * 2 for UTF-16LE)
        assert_eq!(reader.position(), 14);

        // Verify we can read the trailing data correctly
        assert_eq!(reader.read_u32().unwrap(), 66);
    }

    #[test]
    fn test_skip_pascal_string_with_empty_string() {
        // PascalStringWin32: length=0 (no string data)
        let data: Vec<u8> = vec![
            0x00, 0x00, 0x00, 0x00, // length: 0
            0x99, 0x00, 0x00, 0x00, // trailing u32: 153
        ];
        let mut reader = BinaryReader::new(&data);

        reader.skip_pascal_string().unwrap();

        // Should have skipped only the 4-byte length prefix
        assert_eq!(reader.position(), 4);
        assert_eq!(reader.read_u32().unwrap(), 153);
    }

    #[test]
    fn test_skip_pascal_string_with_long_string() {
        // PascalStringWin32: length=100 (200 bytes of string data)
        let mut data: Vec<u8> = vec![0x64, 0x00, 0x00, 0x00]; // length: 100
        data.extend([0x41, 0x00].repeat(100)); // 'A' repeated 100 times in UTF-16LE
        data.extend([0xFF, 0x00, 0x00, 0x00]); // trailing u32: 255

        let mut reader = BinaryReader::new(&data);
        reader.skip_pascal_string().unwrap();

        // Should have skipped 4 + (100 * 2) = 204 bytes
        assert_eq!(reader.position(), 204);
        assert_eq!(reader.read_u32().unwrap(), 255);
    }

    #[test]
    fn test_skip_pascal_string_with_negative_length() {
        // PascalStringWin32: length=-1 (invalid)
        let data: Vec<u8> = vec![0xFF, 0xFF, 0xFF, 0xFF]; // length: -1
        let mut reader = BinaryReader::new(&data);

        let err = reader.skip_pascal_string().unwrap_err();
        match err {
            RawError::CorruptedData(msg) => {
                assert!(msg.contains("negative length"));
                assert!(msg.contains("-1"));
            }
            _ => panic!("Expected CorruptedData error, got {:?}", err),
        }
    }

    #[test]
    fn test_skip_pascal_string_insufficient_data_for_length() {
        // Only 3 bytes available, but need 4 for length prefix
        let data: Vec<u8> = vec![0x01, 0x02, 0x03];
        let mut reader = BinaryReader::new(&data);

        let err = reader.skip_pascal_string().unwrap_err();
        match err {
            RawError::CorruptedData(msg) => {
                assert!(msg.contains("read_i32"));
                assert!(msg.contains("need 4 bytes"));
            }
            _ => panic!("Expected CorruptedData error, got {:?}", err),
        }
    }

    #[test]
    fn test_skip_pascal_string_insufficient_data_for_string() {
        // Length=10, but only 5 UTF-16LE chars (10 bytes) available after prefix
        let data: Vec<u8> = vec![
            0x0A, 0x00, 0x00, 0x00, // length: 10
            0x41, 0x00, 0x42, 0x00, 0x43, 0x00, // only 3 chars (6 bytes)
        ];
        let mut reader = BinaryReader::new(&data);

        let err = reader.skip_pascal_string().unwrap_err();
        match err {
            RawError::CorruptedData(msg) => {
                assert!(msg.contains("skip"));
                assert!(msg.contains("need 20 bytes")); // 10 * 2
            }
            _ => panic!("Expected CorruptedData error, got {:?}", err),
        }
    }

    #[test]
    fn test_skip_pascal_string_multiple_sequential() {
        // Multiple PascalStrings in sequence
        let data: Vec<u8> = vec![
            0x02, 0x00, 0x00, 0x00, // length: 2
            0x41, 0x00, 0x42, 0x00, // "AB"
            0x01, 0x00, 0x00, 0x00, // length: 1
            0x58, 0x00, // "X"
            0x00, 0x00, 0x00, 0x00, // length: 0 (empty)
            0x99, 0x00, 0x00, 0x00, // trailing u32: 153
        ];
        let mut reader = BinaryReader::new(&data);

        // Skip first string: 4 + 4 = 8 bytes
        reader.skip_pascal_string().unwrap();
        assert_eq!(reader.position(), 8);

        // Skip second string: 4 + 2 = 6 bytes
        reader.skip_pascal_string().unwrap();
        assert_eq!(reader.position(), 14);

        // Skip third string (empty): 4 bytes
        reader.skip_pascal_string().unwrap();
        assert_eq!(reader.position(), 18);

        // Verify trailing data
        assert_eq!(reader.read_u32().unwrap(), 153);
    }

    #[test]
    fn test_skip_pascal_string_at_offset() {
        // Test skip_pascal_string with a reader starting at an offset
        let data: Vec<u8> = vec![
            0x00, 0x00, 0x00, 0x00, // padding
            0x03, 0x00, 0x00, 0x00, // length: 3
            0x41, 0x00, 0x42, 0x00, 0x43, 0x00, // "ABC"
            0x77, 0x00, 0x00, 0x00, // trailing u32: 119
        ];
        let mut reader = BinaryReader::at_offset(&data, 4);

        reader.skip_pascal_string().unwrap();
        assert_eq!(reader.position(), 14); // 4 (offset) + 4 (length) + 6 (string)
        assert_eq!(reader.read_u32().unwrap(), 119);
    }
}
