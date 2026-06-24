# Catatan Belajar: Integrasi Rust Core & WPF C# (FFI Bridge)

Dokumen ini mencatat langkah demi langkah bagaimana kita menghubungkan **Rust Core (sebagai Dynamic Library / `.dll`)** dengan **UI Editor C# WPF**, lengkap dengan penjelasan teori dan kode.

---

## 🏗️ Struktur Arsitektur Engine
Dalam pembuatan game engine hibrida, alur kerjanya adalah:
* **Host Utama**: C# WPF adalah aplikasi `.exe` yang berjalan pertama kali.
* **Core Engine**: Kode Rust dikompilasi menjadi `.dll` (Dynamic Link Library).
* **Jembatan**: C# memuat `.dll` Rust tersebut dan memanggil fungsinya menggunakan teknologi bernama **P/Invoke (Platform Invoke)** atau **FFI (Foreign Function Interface)**.

---

## 🛠️ Langkah Demi Langkah yang Kita Lakukan

### Langkah 1: Mengonfigurasi Crate Rust agar Menghasilkan DLL
Secara bawaan, pustaka (*library*) Rust hanya menghasilkan file `.rlib` (hanya bisa dibaca oleh compiler Rust). Agar bisa dibaca oleh C# (dan bahasa non-Rust lainnya), kita harus mengubah konfigurasinya menjadi `cdylib` (C-Dynamic Library).

* **File yang diubah**: [enginelibs/Cargo.toml](file:///D:/Pemrograman/_Game_Engine_QAG/enginelibs/Cargo.toml)
* **Kodenya**:
  ```toml
  [lib]
  crate-type = ["cdylib", "rlib"]
  ```
  * `cdylib`: Menghasilkan file `enginelibs.dll` saat di-build. Ini yang dibaca oleh C#.
  * `rlib`: Menghasilkan pustaka Rust standar. Ini penting agar biner Rust [enginecore](file:///D:/Pemrograman/_Game_Engine_QAG/enginecore) tetap bisa mengimpor `enginelibs` secara native tanpa lewat FFI.

---

### Langkah 2: Menulis Fungsi FFI di Rust
Kita menulis fungsi di Rust yang menggunakan standar bahasa C agar bisa dipahami oleh sistem C#.

* **File yang diubah**: [enginelibs/src/lib.rs](file:///D:/Pemrograman/_Game_Engine_QAG/enginelibs/src/lib.rs)
* **Kodenya**:
  ```rust
  use std::os::raw::c_char;

  #[unsafe(no_mangle)]
  pub extern "C" fn add_numbers(a: i32, b: i32) -> i32 {
      a + b
  }

  #[unsafe(no_mangle)]
  pub extern "C" fn get_engine_version() -> *const c_char {
      let version_str = b"0.1.0-RustCore-WPF\0";
      version_str.as_ptr() as *const c_char
  }
  ```
* **Penjelasan Teori**:
  * `extern "C"`: Ini memberitahu compiler Rust untuk menyusun fungsi ini dengan **C Calling Convention** (standar komunikasi fungsi universal di industri OS).
  * `#[unsafe(no_mangle)]` (Fitur Rust 2024): 
    * *Mangling* adalah proses kompilator mengubah nama fungsi menjadi kode acak unik (misal `_ZN10enginelibs11add_numbersE`).
    * Kita mematikan pengacakan ini agar namanya tetap `"add_numbers"` di dalam DLL, sehingga C# bisa mencarinya dengan mudah.
    * Pada Rust edisi terbaru (Edition 2024), mengubah nama link tingkat rendah dianggap rawan kesalahan, sehingga penulisan atribut ini sekarang harus dibungkus oleh kata kunci `unsafe()`.
  * `*const c_char` & `b"...\0"`:
    * Rust menyimpan string UTF-8 yang dinamis. Bahasa C dan C# tidak memahaminya secara langsung.
    * Kita mengirimnya sebagai *pointer* (alamat memori) ke deretan byte karakter bergaya C yang wajib diakhiri dengan null terminator (`\0`).

---

### Langkah 3: Memperbaiki Hubungan Dependency Rust
Awalnya, kode biner Rust (`enginecore`) mencoba mengimpor folder C# (`engineui`) sebagai pustaka Rust. Ini menyebabkan compiler error karena C# tidak memiliki file konfigurasi Rust (`Cargo.toml`).

* **File yang diubah**: [enginecore/Cargo.toml](file:///D:/Pemrograman/_Game_Engine_QAG/enginecore/Cargo.toml)
* **Tindakan**: Menghapus baris dependency `engineui = { ... }`. Sekarang biner Rust hanya bergantung pada `enginelibs`.

---

### Langkah 4: Otomatisasi Build Rust dari C# WPF
Saat kita memprogram, sangat melelahkan jika setiap kali mengubah kode Rust, kita harus mengetik `cargo build` secara manual di terminal, lalu menyalin `.dll` ke dalam folder proyek C#. Kita mengotomatiskan hal ini di level compiler MSBuild.

* **File yang diubah**: [engineui/engineui/engineui.csproj](file:///D:/Pemrograman/_Game_Engine_QAG/engineui/engineui/engineui.csproj)
* **Kodenya**:
  ```xml
  <Target Name="BuildRustEngine" BeforeTargets="BeforeBuild">
    <!-- 1. Jalankan perintah kompilasi Rust secara otomatis -->
    <Exec Command="cargo build --manifest-path &quot;$(ProjectDir)..\..\Cargo.toml&quot;" />
    
    <!-- 2. Salin DLL hasil kompilasi ke folder tempat WPF dijalankan -->
    <Copy SourceFiles="$(ProjectDir)..\..\target\debug\enginelibs.dll" 
          DestinationFolder="$(TargetDir)" 
          SkipUnchangedFiles="true" />
  </Target>
  ```
  * `BeforeTargets="BeforeBuild"`: Target ini berjalan tepat sebelum WPF mulai dikompilasi.
  * `$(ProjectDir)..\..\Cargo.toml`: Mengarah ke file konfigurasi utama di root workspace Anda.
  * `$(TargetDir)`: Folder output kompilasi WPF (biasanya `bin\Debug\net8.0-windows\`).

---

### Langkah 5: Memanggil Fungsi DLL Rust dari C#
Terakhir, kita mengimpor fungsi-fungsi dari DLL ke dalam kode WPF.

* **File yang diubah**: [MainWindow.xaml.cs](file:///D:/Pemrograman/_Game_Engine_QAG/engineui/engineui/MainWindow.xaml.cs)
* **Kodenya**:
  ```csharp
  using System.Runtime.InteropServices; // Diperlukan untuk P/Invoke

  // Deklarasi FFI
  [DllImport("enginelibs.dll", EntryPoint = "add_numbers", CallingConvention = CallingConvention.Cdecl)]
  public static extern int AddNumbers(int a, int b);

  [DllImport("enginelibs.dll", EntryPoint = "get_engine_version", CallingConvention = CallingConvention.Cdecl)]
  public static extern IntPtr GetEngineVersionPtr();
  ```
* **Penjelasan Teori**:
  * `[DllImport("enginelibs.dll")]`: Menginstruksikan C# untuk mencari file `enginelibs.dll` di folder eksekusinya dan memuatnya ke memori.
  * `IntPtr`: Mewakili pointer memori mentah. Kita **tidak boleh** langsung memakai tipe `string` di deklarasi `DllImport` untuk string yang dikembalikan oleh Rust. Mengapa? Karena .NET CLR akan mengira string tersebut dialokasikan oleh C# dan akan mencoba menghapusnya secara paksa setelah dibaca, menyebabkan *Memory Access Violation* (Crash).
  * `Marshal.PtrToStringAnsi(versiPointer)`: Kita mengambil alamat memori mentah (`IntPtr`), lalu menyalin isinya menjadi string C# yang aman dikelola oleh .NET Garbage Collector.

---

## 📈 Alur Kerja Pengembangan Selanjutnya
Jika nanti Anda melakukan perubahan kode di sisi Rust (`lib.rs`):
1. Cukup tekan **F5 / Build** di proyek WPF Anda (melalui Visual Studio / Rider).
2. Proyek WPF secara otomatis memanggil Rust compiler, memperbarui `.dll`, menyalinnya, dan menjalankan UI versi terbaru dengan kode Rust terbaru Anda!
