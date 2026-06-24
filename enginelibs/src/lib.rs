use std::os::raw::c_char;

// 1. Fungsi pertambahan angka (Sederhana)
//
// #[unsafe(no_mangle)]: Memberitahu kompiler Rust agar tidak mengacak nama fungsi ini.
//                      Mulai Rust Edition 2024, kita harus membungkusnya dengan `unsafe(...)` 
//                      karena ini mempengaruhi linking tingkat rendah.
//
// extern "C": Memberitahu Rust untuk menggunakan standar pemanggilan C (C Calling Convention).
//             Ini adalah standar universal yang dipahami oleh hampir semua bahasa pemrograman, termasuk C# (WPF).
//
// pub: Membuat fungsi ini bisa diakses dari luar (public).
#[unsafe(no_mangle)]
pub extern "C" fn add_numbers(a: i32, b: i32) -> i32 {
    a + b
}

// 2. Fungsi untuk mengambil versi engine (Mengembalikan Teks/String)
//
// *const c_char: Adalah representasi pointer teks bergaya bahasa C (null-terminated string).
//                Karena Rust menyimpan string secara berbeda dari C#, kita harus mengirimkannya 
//                sebagai pointer ke memory byte berakhiran '\0' (null byte).
#[unsafe(no_mangle)]
pub extern "C" fn get_engine_version() -> *const c_char {
    // Kita membuat byte string statis dengan akhiran \0 (null terminator).
    // Menggunakan b"..." menghasilkan array byte statis.
    // Ini aman dikembalikan karena data statis akan selalu ada di memori selama program berjalan.
    let version_str = b"0.1.0-RustCore-WPF\0";
    version_str.as_ptr() as *const c_char
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn test_add_numbers() {
        assert_eq!(add_numbers(5, 10), 15);
    }
}

