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

        [DllImport("enginelibs.dll", EntryPoint = "subtracting_numbers", CallingConvention = CallingConvention.Cdecl)]
        public static extern int SubsNumbers(int a, int b);
        public MainWindow()
        {
            InitializeComponent();
        }

        private void add_btn_click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Memanggil fungsi penambahan angka dari Rust
                int hasilTambah = AddNumbers(15, 27);


                // Menampilkan hasil pada TextBlock di WPF
                txtAdd.Text = $"Hasil Penjumlahan dari rust: 15 + 27 = {hasilTambah}\n";
                       
            }
            catch (DllNotFoundException)
            {
                txtAdd.Text = "Kesalahan: enginelibs.dll tidak ditemukan!\n" +
                              "Pastikan proyek Rust sudah dikompilasi.";
            }
            catch (Exception ex)
            {
                txtAdd.Text = $"Terjadi kesalahan: {ex.Message}";
            }
        }

        private void subs_btn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //Memanggil fungsi pengurangan angka dari rust
                int hasilPengurangan = SubsNumbers(10, 14);
                txtSubs.Text = $"Hasil Pengurangan dari rust: 10 - 14 = {hasilPengurangan}\n";
            }
            catch (DllNotFoundException)
            {
                txtAdd.Text = "Kesalahan: enginelibs.dll tidak ditemukan!\n" +
                              "Pastikan proyek Rust sudah dikompilasi.";
            }
            catch (Exception ex)
            {
                txtAdd.Text = $"Terjadi kesalahan: {ex.Message}";
            }
        }
    }
}