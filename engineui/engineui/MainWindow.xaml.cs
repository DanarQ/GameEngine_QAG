using System.Runtime.InteropServices; // Diperlukan untuk melakukan P/Invoke (interaksi dengan C/Rust DLL)
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace engineui
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // 1. Mengimpor fungsi "add_numbers" dari enginelibs.dll
        // DllImport: Atribut C# untuk meload library eksternal (dalam hal ini enginelibs.dll).
        // CallingConvention.Cdecl: Standar pemanggilan fungsi yang cocok dengan `extern "C"` di Rust.
        [DllImport("enginelibs.dll", EntryPoint = "add_numbers", CallingConvention = CallingConvention.Cdecl)]
        public static extern int AddNumbers(int a, int b);

        // 2. Mengimpor fungsi "get_engine_version" dari enginelibs.dll
        // Mengapa mengembalikan IntPtr? 
        // Karena Rust mengembalikan pointer memori teks (char*). Jika kita langsung memakai tipe data C# `string`,
        // C# .NET akan mencoba otomatis menghapus memori teks tersebut setelah selesai (deallokasi). 
        // Ini akan menyebabkan crash karena memori dialokasikan oleh Rust, bukan C#.
        // Dengan IntPtr, kita menerima alamat memori mentahnya terlebih dahulu secara aman.
        [DllImport("enginelibs.dll", EntryPoint = "get_engine_version", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr GetEngineVersionPtr();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnKlik_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Memanggil fungsi penambahan angka dari Rust
                int hasilTambah = AddNumbers(15, 27);

                // Memanggil fungsi pengambil teks versi dari Rust
                IntPtr versiPointer = GetEngineVersionPtr();

                // Marshal.PtrToStringAnsi: Menyalin data teks dari alamat memori C (Rust)
                // ke objek string C# yang aman dikelola oleh .NET Garbage Collector.
                string? versiEngine = Marshal.PtrToStringAnsi(versiPointer);

                // Menampilkan hasil pada TextBlock di WPF
                txtHalo.Text = $"Koneksi ke Rust Berhasil!\n\n" +
                              $"Hasil Penjumlahan (Rust): 15 + 27 = {hasilTambah}\n" +
                              $"Versi Engine (Rust): {versiEngine}";
            }
            catch (DllNotFoundException)
            {
                txtHalo.Text = "Kesalahan: enginelibs.dll tidak ditemukan!\n" +
                              "Pastikan proyek Rust sudah dikompilasi.";
            }
            catch (Exception ex)
            {
                txtHalo.Text = $"Terjadi kesalahan: {ex.Message}";
            }
        }
    }
}