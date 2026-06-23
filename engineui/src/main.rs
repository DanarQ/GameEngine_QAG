use eframe::egui;

struct MyGameEngineApp {
    object_count: i32,
}

impl Default for MyGameEngineApp {
    fn default() -> Self {
        Self { object_count: 10 }
    }
}

// Implementasi Trait App untuk eframe v0.34.3
// `ui()` adalah satu-satunya required method — `update()` sudah deprecated.
// Parameter `ui: &mut egui::Ui` mewakili root UI, bisa langsung dipakai
// oleh panel-panel seperti `show_inside(ui, |ui| ...)`.
impl eframe::App for MyGameEngineApp {
    fn ui(&mut self, ui: &mut egui::Ui, _frame: &mut eframe::Frame) {
        // 1. SIDE BAR KIRI — Scene Hierarchy (show_inside menerima &mut Ui)
        egui::Panel::left("hierarchy_side_bar")
            .resizable(true)
            .default_size(200.0)
            .show_inside(ui, |ui| {
                ui.heading("Scene Hierarchy");
                ui.separator();

                for i in 0..self.object_count {
                    if ui
                        .selectable_label(false, format!("Cube Objek {}", i))
                        .clicked()
                    {
                        // Logika jika objek diklik
                    }
                }
            });

        // 2. SIDE BAR KANAN — Inspector Properti
        egui::Panel::right("inspector_side_bar")
            .resizable(true)
            .default_size(250.0)
            .show_inside(ui, |ui| {
                ui.heading("Inspector");
                ui.separator();

                ui.collapsing("Transform", |ui| {
                    ui.label("Position: X: 0.0, Y: 0.0, Z: 0.0");
                });
            });

        // 3. CENTRAL PANEL — Viewport utama (harus setelah side panel,
        //    agar otomatis mengisi sisa ruang yang tersisa)
        egui::CentralPanel::default().show_inside(ui, |ui| {
            ui.heading("Game Viewport (Main Area)");

            if ui.button("Tambah Objek ke Hierarchy").clicked() {
                self.object_count += 1;
            }
        });
    }
}

fn main() -> eframe::Result<()> {
    let options = eframe::NativeOptions::default();
    eframe::run_native(
        "Game Engine UI",
        options,
        Box::new(|_cc| Ok(Box::new(MyGameEngineApp::default()))),
    )
}
