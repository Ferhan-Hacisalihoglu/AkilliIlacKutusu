#include <Servo.h>

// --- AYARLAR ---
const int servoSayisi = 2;
unsigned long beklemeSuresi = 60000; 

// --- HIZ AYARI (YENİ) ---
const int servoHizGecikmesi = 100; // Her adımda kaç milisaniye beklesin? (Sayı arttıkça yavaşlar)

// --- PIN TANIMLAMALARI ---
const int buttonPin = 2;      
const int kirmiziLedPin = 12; 
const int buzzerPin = 7;      
const int yesilLedPin = 8;    
const int servoPinleri[servoSayisi] = {9, 10}; // 9: SABAH, 10: AKŞAM

Servo servolar[servoSayisi];

// --- DURUM DEĞİŞKENLERİ ---
int aktifServoIndex = -1;       
unsigned long acilisZamani = 0; 
bool alarmModu = false;         

void setup() {
  Serial.begin(9600); 
  Serial.setTimeout(100); 
  
  pinMode(buttonPin, INPUT_PULLUP); 
  pinMode(yesilLedPin, OUTPUT);
  pinMode(kirmiziLedPin, OUTPUT);  
  pinMode(buzzerPin, OUTPUT);

  for (int i = 0; i < servoSayisi; i++) {
    servolar[i].attach(servoPinleri[i]);
    servolar[i].write(110); // Başlangıç pozisyonu (Kapalı)
  }
}

void loop() {
  // --- 1. BİLGİSAYARDAN KOMUT DİNLE ---
  if (Serial.available() > 0) {
    char komut = Serial.read();

    if (komut == 'T') {
      long yeniSure = Serial.parseInt(); 
      if (yeniSure > 0) {
        beklemeSuresi = yeniSure;
        tone(buzzerPin, 2000, 100);
      }
    }
    else if (aktifServoIndex == -1) {
      if (komut == '1') { kutuAc(0); } // SABAH
      else if (komut == '2') { kutuAc(1); } // AKŞAM
    }
  }

  // --- 2. EĞER BİR KUTU AÇIKSA ---
  if (aktifServoIndex != -1) {
    
    unsigned long gecenSure = millis() - acilisZamani;

    // -- SÜRE KONTROLÜ --
    if (gecenSure > beklemeSuresi && !alarmModu) {
      alarmBaslat(); 
    }

    // -- BUTON KONTROLÜ --
    if (digitalRead(buttonPin) == HIGH) { 
      kutuKapat();
      delay(500); 
    }
  }
}

void kutuAc(int index) {
  aktifServoIndex = index;
  acilisZamani = millis(); 
  alarmModu = false;       

  // DEĞİŞİKLİK BURADA: Yavaşça açılma (110'dan 190'a)
  // Eski kod: servolar[index].write(190);
  for (int pos = 110; pos <= 180; pos += 10) { 
    servolar[index].write(pos);
    delay(servoHizGecikmesi); // Hızı ayarlayan bekleme
  }
  
  digitalWrite(yesilLedPin, HIGH); 
  digitalWrite(kirmiziLedPin, LOW);
  
  tone(buzzerPin, 1000);
}

void alarmBaslat() {
  alarmModu = true;
  digitalWrite(kirmiziLedPin, HIGH); 
  tone(buzzerPin, 2000); 
}

void kutuKapat() {
  // DEĞİŞİKLİK BURADA: Yavaşça kapanma (190'dan 110'a)
  // Eski kod: servolar[aktifServoIndex].write(110);
  for (int pos = 180; pos >= 110; pos -= 10) { 
    servolar[aktifServoIndex].write(pos);
    delay(servoHizGecikmesi); // Hızı ayarlayan bekleme
  }

  digitalWrite(yesilLedPin, LOW);
  digitalWrite(kirmiziLedPin, LOW);
  noTone(buzzerPin);
  aktifServoIndex = -1; 
}