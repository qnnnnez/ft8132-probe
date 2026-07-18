//! Compile-time hex string to byte array utilities.
//!
//! The main entrypoint is the [`hex_bytes!`] macro, which turns a hex string
//! (with no `0x` prefix) into a `[u8; N]` array at compile time.
//!
//! Examples:
//!
//! ```rust
//! use ft8132_probe::hex_bytes;
//! const MAGIC: [u8; 4] = hex_bytes!(cafebabe);
//! assert_eq!(MAGIC, [0xCA, 0xFE, 0xBA, 0xBE]);
//! ```
//!
//! You may also pass a string literal:
//!
//! ```rust
//! use ft8132_probe::hex_bytes;
//! const MAGIC: [u8; 4] = hex_bytes!("cafebabe");
//! assert_eq!(MAGIC, [0xCA, 0xFE, 0xBA, 0xBE]);
//! ```

/// Returns the number of bytes encoded by a hex string.
///
/// Panics at compile-time if the string has odd length.
pub const fn hex_len_bytes(s: &str) -> usize {
    let len = s.as_bytes().len();
    if (len & 1) != 0 {
        panic!("hex string must have even length");
    }
    len / 2
}

const fn hex_nibble(b: u8) -> u8 {
    match b {
        b'0'..=b'9' => b - b'0',
        b'a'..=b'f' => 10 + (b - b'a'),
        b'A'..=b'F' => 10 + (b - b'A'),
        _ => panic!("invalid hex digit"),
    }
}

/// Converts a hex string into a fixed-sized byte array.
///
/// Panics at compile-time if:
/// - The string length is not exactly `N * 2`
/// - The string contains non-hex characters
pub const fn hex_to_bytes<const N: usize>(s: &str) -> [u8; N] {
    let bytes = s.as_bytes();
    if bytes.len() != N * 2 {
        panic!("hex string length does not match output array length");
    }

    let mut out = [0u8; N];
    let mut i = 0usize;
    while i < N {
        let hi = hex_nibble(bytes[i * 2]);
        let lo = hex_nibble(bytes[i * 2 + 1]);
        out[i] = (hi << 4) | lo;
        i += 1;
    }
    out
}

/// Converts a hex string literal into a byte array literal at compile time.
///
/// - `hex_bytes!(cafebabe)` expands to a `[u8; 4]` array.
/// - `hex_bytes!("cafebabe")` is also supported.
#[macro_export]
macro_rules! hex_bytes {
    ($hex:literal) => {{
        const S: &str = $hex;
        const N: usize = $crate::hexstring::hex_len_bytes(S);
        const OUT: [u8; N] = $crate::hexstring::hex_to_bytes::<N>(S);
        OUT
    }};
    ($hex:tt) => {{
        const S: &str = stringify!($hex);
        const N: usize = $crate::hexstring::hex_len_bytes(S);
        const OUT: [u8; N] = $crate::hexstring::hex_to_bytes::<N>(S);
        OUT
    }};
}
