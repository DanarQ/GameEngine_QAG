
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
#[unsafe(no_mangle)]
pub extern  "C" fn subtracting_numbers(a: i32, b: i32) -> i32 {
    a - b
}


#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn test_add_numbers() {
        assert_eq!(add_numbers(5, 10), 15);
    }
    #[test]
    fn test_subtracting_numbers(){
        assert_eq!(subtracting_numbers(5, 5),0);
    }
}

