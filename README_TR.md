# DepoBul — Turkce kullanim

## Visual Studio ile calistirma

1. ZIP'in tamamini bir klasore cikartin.
2. Kokteki **DepoBul.sln** dosyasini Visual Studio ile acin. `packages`, `Pilot_Veriler` ve `Ornek_Otomatik_Deney` klasorlerini tasimayin/silmeyin.
3. Proje **.NET Framework 4.5.2** hedefler. Bu surumun hedefleme/reference assembly dosyalari ve Visual Studio .NET masaustu gelistirme bilesenleri gerekir. Surum degistirmek deney ortamini da degistirebilir.
4. Baslangic projesi `DepoBul` olsun; **F5** ile calistirin.
5. **Otomatik Depo Ara** ekranini acin. `Pilot_Veriler/magazalar.csv` dosyasi otomatik secilir; gerekirse Sec ile yukleyin.
6. Ilk denemede varsayilanlari koruyun: kapasite 20, en cok arac 28, ceza 100, son kosum 30, son konum 3, adimlar 40,20,10.
7. **Depo konumunu arastir** butonuna basin. Ardindan **Onerilen depoyu haritada goster** ve ana ekrandaki **Karayolu Rotalari** butonlarini kullanin. Arayuzdeki butonlar Turkce karakterlerle gorunur.

Makalede kullanilan yontem **Otomatik Depo Ara** akisidir. Eski ekranin eski algoritma kodlari geriye uyumluluk icin korunmustur; makale yontemiyle karistirilmamalidir.

## Hazir deneyi acma

Otomatik arama ekraninda **Kayitli otomatik sonucu ac** ile `Ornek_Otomatik_Deney/secilen_depo_kosumlari.csv` dosyasini secin. Yeni bir konum aramasi yapilmadan kayitli sonuc yuklenir. Harita ve yeni yol geometrileri internet gerektirebilir.

Kayitli en iyi sonuc: **AUTO001, 5 arac, 1.855,3742 km**. Secilen depoda 30 kosumun ortalama mesafesi **1.936,578553 km**; en yakin komsu mesafesi **2.275,5799 km**.

**F ile mesafeyi karistirmayin:** UI'daki en yakin komsu F degeri **2.675,580** olup mesafe + 400 arac cezasidir. `en_yakin_komsu.csv` icindeki `ga_mean_improvement_percent` alani F uzerinden hesaplanmistir. Makaledeki %14,90 ve %18,47 ise toplam **mesafe** iyilesmeleridir.

## GitHub'a yukleme

- Visual Studio'da **DepoBul.sln'nin bulundugu klasoru** Git deposu olarak kullanin. ZIP dosyasinin kendisini tek dosya olarak yuklemek yerine klasor icerigini commit edin.
- GitHub'da bos bir public depo acin; bu pakette README ve .gitignore oldugu icin GitHub'da ikinci bir README olusturmaniz gerekmez.
- Commit oncesi degisiklikleri kontrol edin: `.vs`, `bin`, `obj` ve kullanici ayarlari .gitignore ile disarida tutulur. `packages` ve ornek deney kayitlari bilerek korunur.
- Ilk commit icin `Prepare DepoBul V6.1 research release` mesaji kullanilabilir. Push sonrasinda bu surume `v6.1-research` etiketi verebilirsiniz.
- Makaledeki kod erisim beyanina gercek depo baglantisini ekleyin. Paylasim bu paket tarafindan yapilmamistir.

## Veri ve haklar

Konumlar acik harita kayitlarindan gelir; talepler gercek siparis degildir. Veri kaynaklari `DATA_SOURCES.md` icindedir. Mevcut CSV kimlikleri ve sonuclari degistirilmemistir. Girdi kayitlarinda zincir/marka adlari yer alabilir; bu durum kurum veya isletme destegi/onayi anlamina gelmez.

Bu hazirlama adiminda sizin adiniza MIT gibi genis bir yeniden kullanim lisansi verilmemistir. Kodun haklari korunur; harita verisi ve ucuncu taraf bilesenlerin kosullari ayri belirtilmistir: `RIGHTS_AND_NOTICES.md`.

Bu ortamda Windows/Visual Studio derlemesi ve arayuz calistirmasi yapilamadi. Kayitli deney maliyetleri yeniden hesaplandi; ayrintilar `docs/RELEASE_CHECKS.md` icindedir. Mevcut C# kontrolleri Windows'ta `Testler/RunChecks.ps1` ile calistirilabilir.
