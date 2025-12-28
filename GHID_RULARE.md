# Ghid pentru rularea migrațiilor și pornirea proiectului

## 📋 Pași pentru rularea migrațiilor

### ⚠️ IMPORTANT: Oprește proiectul înainte de a rula migrațiile!

Dacă proiectul rulează (în Visual Studio, VS Code sau terminal), **oprește-l complet** înainte de a rula migrațiile.

### 1. Instalează dotnet-ef (doar prima dată)

**IMPORTANT:** Proiectul folosește .NET 9.0, deci trebuie să instalezi versiunea compatibilă de dotnet-ef:

```powershell
dotnet tool install --global dotnet-ef --version 9.0.0
```

Dacă ai instalat deja o versiune mai nouă (10.0.x), dezinstalează-o mai întâi:
```powershell
dotnet tool uninstall --global dotnet-ef
dotnet tool install --global dotnet-ef --version 9.0.0
```

### 2. Deschide Terminal/PowerShell în directorul proiectului

Navighează în directorul `OnlineShop`:
```powershell
cd "C:\Users\maria\OneDrive\Desktop\Facultate\Online-Shop\OnlineShop"
```

SAU dacă ești deja în folderul `Online-Shop`:
```powershell
cd OnlineShop
```

### 3. Creează migrația pentru noile culori

```powershell
dotnet ef migrations add AddMoreColors
```

Această comandă va crea un fișier nou în folderul `Migrations` cu noile culori.

### 4. Aplică migrațiile în baza de date

```powershell
dotnet ef database update
```

Această comandă va actualiza baza de date cu toate migrațiile, inclusiv noile culori.

## 🚀 Pornirea proiectului

### Opțiunea 1: Din terminal (PowerShell/CMD)

1. Asigură-te că ești în directorul `OnlineShop`:
```powershell
cd OnlineShop
```

2. Rulează proiectul:
```powershell
dotnet run
```

3. Aplicația va porni și vei vedea în terminal URL-ul:
   - HTTP: `http://localhost:5261`
   - HTTPS: `https://localhost:7244`

4. Deschide browserul la adresa afișată sau apasă `Ctrl+C` pentru a opri serverul.

### Opțiunea 2: Din Visual Studio

1. Deschide fișierul `Online-Shop.sln` în Visual Studio
2. Apasă `F5` sau click pe butonul "Run" (▶️)
3. Aplicația va porni automat și se va deschide în browser

### Opțiunea 3: Din Visual Studio Code

1. Deschide folderul proiectului în VS Code
2. Deschide terminalul integrat (`Ctrl + ~`)
3. Navighează în `OnlineShop`:
```powershell
cd OnlineShop
```
4. Rulează:
```powershell
dotnet run
```

## ⚠️ Note importante

- **OPREȘTE PROIECTUL** înainte de a rula migrațiile! Dacă vezi eroarea "file is locked by another process", înseamnă că proiectul rulează.
- **Prima dată** când rulezi proiectul, migrațiile se aplică automat dacă baza de date nu există
- Dacă ai erori la migrații, verifică că ai toate pachetele instalate:
  ```powershell
  dotnet restore
  ```
- Dacă vrei să resetezi baza de date complet, poți șterge fișierul `app.db` și să rulezi din nou `dotnet ef database update`
- Dacă ai instalat deja `dotnet-ef`, nu trebuie să-l instalezi din nou

## 🔧 Comenzi utile

- `dotnet build` - Compilează proiectul fără să-l ruleze
- `dotnet restore` - Restaurează pachetele NuGet
- `dotnet ef migrations list` - Listează toate migrațiile
- `dotnet ef migrations remove` - Șterge ultima migrație (dacă nu a fost aplicată)

