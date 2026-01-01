using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO.Ports;
using System.Linq; // LINQ sorguları için gerekli (Count kontrolü vb.)
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace AkilliIlacKutusu
{
    // Veri Sınıfı
    public class AlarmBilgisi : INotifyPropertyChanged
    {
        private string _gun;
        private string _saat;
        private string _dakika;
        private string _kutuNo;

        public string Gun
        {
            get { return _gun; }
            set { _gun = value; OnPropertyChanged(); }
        }

        public string Saat
        {
            get { return _saat; }
            set { _saat = value; OnPropertyChanged(); }
        }

        public string Dakika
        {
            get { return _dakika; }
            set { _dakika = value; OnPropertyChanged(); }
        }

        public string KutuNo
        {
            get { return _kutuNo; }
            set { _kutuNo = value; OnPropertyChanged(); }
        }

        public bool Tetiklendi { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    public partial class MainWindow : Window
    {
        SerialPort arduinoPort;
        DispatcherTimer zamanlayici;
        ObservableCollection<AlarmBilgisi> alarmListesi = new ObservableCollection<AlarmBilgisi>();

        AlarmBilgisi duzenlenecekAlarm = null;
        bool duzenlemeModu = false;
        string sabitPortAdi = "COM3"; // Portunuzu kontrol edin

        // Haftanın günleri listesi
        string[] gunler = { "Pazartesi", "Salı", "Çarşamba", "Perşembe", "Cuma", "Cumartesi", "Pazar" };

        public MainWindow()
        {
            InitializeComponent();
            lstAlarmlar.ItemsSource = alarmListesi;
            GunleriDoldur();
            BaglantiyiKur();
            ZamanlayiciyiBaslat();
        }

        private void GunleriDoldur()
        {
            cmbGunSecimi.ItemsSource = gunler;
            cmbGunSecimi.SelectedIndex = 0; // Varsayılan Pazartesi
        }

        // --- İŞLEM BUTONU ---
        private void btnIslem_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtSaatInput.Text) || string.IsNullOrWhiteSpace(txtDakikaInput.Text))
            {
                MessageBox.Show("Lütfen saat ve dakika giriniz.", "Eksik Bilgi", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string secilenGun = cmbGunSecimi.SelectedItem.ToString();
            string saat = txtSaatInput.Text.PadLeft(2, '0');
            string dakika = txtDakikaInput.Text.PadLeft(2, '0');
            string kutu = cmbKutuSecimi.Text;

            if (duzenlemeModu && duzenlenecekAlarm != null)
            {
                duzenlenecekAlarm.Gun = secilenGun;
                duzenlenecekAlarm.Saat = saat;
                duzenlenecekAlarm.Dakika = dakika;
                duzenlenecekAlarm.KutuNo = kutu;
                duzenlenecekAlarm.Tetiklendi = false;

                MessageBox.Show("Alarm güncellendi.", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
                DuzenlemeModunuKapat();
            }
            else
            {
                // KONTROL: Seçilen günde en fazla 2 alarm olsun
                int oGundekiAlarmSayisi = alarmListesi.Count(x => x.Gun == secilenGun);

                if (oGundekiAlarmSayisi >= 2)
                {
                    MessageBox.Show($"{secilenGun} günü için zaten 2 alarm kayıtlı!", "Günlük Sınır", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                alarmListesi.Add(new AlarmBilgisi
                {
                    Gun = secilenGun,
                    Saat = saat,
                    Dakika = dakika,
                    KutuNo = kutu,
                    Tetiklendi = false
                });
            }

            // Temizleme (Opsiyonel: Saati sıfırla ama günü sabit tutabiliriz)
            txtSaatInput.Text = "";
            txtDakikaInput.Text = "";
        }

        // --- DÜZENLEME MODU ---
        private void btnDuzenle_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            duzenlenecekAlarm = btn.Tag as AlarmBilgisi;

            if (duzenlenecekAlarm != null)
            {
                cmbGunSecimi.SelectedItem = duzenlenecekAlarm.Gun;
                txtSaatInput.Text = duzenlenecekAlarm.Saat;
                txtDakikaInput.Text = duzenlenecekAlarm.Dakika;
                cmbKutuSecimi.SelectedIndex = duzenlenecekAlarm.KutuNo == "1" ? 0 : 1;

                duzenlemeModu = true;
                btnIslem.Content = "Güncelle";
                btnIslem.Style = (Style)FindResource("WarningButton");
                btnIptal.Visibility = Visibility.Visible;
            }
        }

        private void btnIptal_Click(object sender, RoutedEventArgs e)
        {
            DuzenlemeModunuKapat();
        }

        private void DuzenlemeModunuKapat()
        {
            duzenlemeModu = false;
            duzenlenecekAlarm = null;
            btnIslem.Content = "Listeye Ekle";
            btnIslem.Style = (Style)FindResource("ModernButton");
            btnIptal.Visibility = Visibility.Collapsed;

            txtSaatInput.Text = "";
            txtDakikaInput.Text = "";
        }

        private void btnSil_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            AlarmBilgisi silinecek = btn.Tag as AlarmBilgisi;

            if (MessageBox.Show("Bu alarmı silmek istediğinize emin misiniz?", "Silme Onayı", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                if (silinecek == duzenlenecekAlarm) DuzenlemeModunuKapat();
                if (silinecek != null) alarmListesi.Remove(silinecek);
            }
        }

        // --- ZAMANLAYICI & ARDUINO ---
        private void ZamanlayiciyiBaslat()
        {
            zamanlayici = new DispatcherTimer();
            zamanlayici.Interval = TimeSpan.FromSeconds(1);
            zamanlayici.Tick += Zamanlayici_Tick;
            zamanlayici.Start();
        }

        private void Zamanlayici_Tick(object sender, EventArgs e)
        {
            DateTime simdi = DateTime.Now;

            // İngilizce gün ismini Türkçe'ye çevirmemiz gerek
            string bugunTurkce = GunIngilizceToTurkce(simdi.DayOfWeek);

            txtGuncelSaat.Text = $"{bugunTurkce} {simdi:HH:mm:ss}";

            if (arduinoPort == null || !arduinoPort.IsOpen) return;

            string sa = simdi.ToString("HH");
            string dk = simdi.ToString("mm");

            foreach (var alarm in alarmListesi)
            {
                // Hem SAAT hem de GÜN tutmalı
                if (alarm.Gun == bugunTurkce && alarm.Saat == sa && alarm.Dakika == dk)
                {
                    if (!alarm.Tetiklendi)
                    {
                        arduinoPort.Write(alarm.KutuNo);
                        alarm.Tetiklendi = true;
                    }
                }
                else
                {
                    // Dakika değiştiğinde tetiklendi bilgisini sıfırla ki haftaya tekrar çalışabilsin
                    alarm.Tetiklendi = false;
                }
            }
        }

        private string GunIngilizceToTurkce(DayOfWeek day)
        {
            switch (day)
            {
                case DayOfWeek.Monday: return "Pazartesi";
                case DayOfWeek.Tuesday: return "Salı";
                case DayOfWeek.Wednesday: return "Çarşamba";
                case DayOfWeek.Thursday: return "Perşembe";
                case DayOfWeek.Friday: return "Cuma";
                case DayOfWeek.Saturday: return "Cumartesi";
                case DayOfWeek.Sunday: return "Pazar";
                default: return "";
            }
        }

        private void BaglantiyiKur()
        {
            try
            {
                if (arduinoPort != null && arduinoPort.IsOpen) arduinoPort.Close();
                arduinoPort = new SerialPort(sabitPortAdi, 9600);
                arduinoPort.Open();
                txtDurum.Text = "BAĞLI";
                txtDurum.Foreground = new SolidColorBrush(Color.FromRgb(76, 175, 80));
            }
            catch
            {
                txtDurum.Text = "BAĞLI DEĞİL";
                txtDurum.Foreground = new SolidColorBrush(Color.FromRgb(229, 57, 53));
            }
        }

        private void btnBaglan_Click(object sender, RoutedEventArgs e) => BaglantiyiKur();

        private void btnSureGuncelle_Click(object sender, RoutedEventArgs e)
        {
            if (arduinoPort != null && arduinoPort.IsOpen)
            {
                try
                {
                    int ms = int.Parse(txtSureSaniye.Text) * 1000;
                    arduinoPort.Write("T" + ms.ToString());
                    MessageBox.Show("Süre güncellendi.", "Başarılı");
                }
                catch { }
            }
            else
            {
                MessageBox.Show("Bağlantı yok.", "Hata");
            }
        }
    }
}