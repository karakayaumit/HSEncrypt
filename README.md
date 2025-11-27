# HSEncrypt

## BankServiceViewer Windows uygulaması

Bu depo, `vNew_BankServiceSettings` tablosundaki verileri gösteren örnek bir Windows Forms uygulaması içerir. Uygulama, seçim yapılabilen bir `DataGridView` ve seçili satırlarla çalışmak için temel butonlar sağlar.

### Proje yapısı
- `BankServiceViewer/BankServiceViewer.csproj`: .NET 8.0 Windows Forms proje dosyası.
- `BankServiceViewer/Program.cs`: Uygulamanın giriş noktası.
- `BankServiceViewer/MainForm.cs` ve `MainForm.Designer.cs`: Grid ve butonlar içeren ana form ve olaylar.
- `BankServiceViewer/SettingDetailsDialog.cs`: Seçilen satırın detaylarını gösteren basit pencere.
- `BankServiceViewer/Data/*`: `vNew_BankServiceSettings` verisini yüklemek için SQL erişim kodu.
- `BankServiceViewer/App.config`: `BankServiceDb` bağlantı dizesi tanımı.

### Çalıştırma
1. `BankServiceViewer/App.config` içindeki `BankServiceDb` bağlantı dizesini kendi veritabanınıza göre güncelleyin.
2. Windows ortamında `.NET 8.0` SDK kurulu iken aşağıdaki komutları çalıştırın:
   ```bash
   dotnet restore BankServiceViewer/BankServiceViewer.csproj
   dotnet run --project BankServiceViewer/BankServiceViewer.csproj
   ```
3. Grid otomatik olarak `vNew_BankServiceSettings` verilerini yükler; satır seçerek butonlar üzerinden işlemleri tetikleyebilirsiniz.

### Hızlı derleme (Windows)
`NETSDK1004: project.assets.json not found` hatasını önlemek için önce NuGet paketlerini geri yüklemek gerekir. Aşağıdaki PowerShell betiği hem geri yüklemeyi hem de derlemeyi tek adımda yapar:

```powershell
./build.ps1 -Configuration Debug
```

`-Configuration Release` parametresiyle Release derlemesi de alınabilir.

> Not: Bu ortamda .NET SDK kurulu olmadığı için CI veya test komutları çalıştırılamadı.
