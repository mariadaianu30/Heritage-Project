# Heritage - Magazin Online de Haine Vintage

Heritage este un proiect de magazin online dedicat pasionaților de haine vintage, construit folosind **C#** și **ASP.NET Core MVC**. Proiectul oferă o experiență completă de navigare și cumpărare, combinând design elegant și funcționalitate intuitivă.

---

## Funcționalități principale

- **Catalog de produse**: Vizualizarea și filtrarea produselor după categorie, preț, rating și noutăți.  
- **Detalii produs**: Fiecare produs are o pagină dedicată cu informații complete: titlu, preț, categorie, stoc disponibil și imagine.  
- **Coș de cumpărături**: Utilizatorii pot adăuga produse în coș și gestiona comenzile.  
- **Wishlist**: Posibilitatea de a adăuga produse preferate în wishlist pentru a le accesa mai ușor ulterior.  
- **Filtre și sortări**: Filtrarea produselor după categorie, sortare după preț, rating sau cele mai noi produse.  
- **Gestionare admin**: Adminii pot adăuga produse, gestiona stocuri și categorii.

---

## Modele principale

1. **Product**  
   - Id, Title, Description, Price, Stock, ImagePath  
   - Relație cu `Category`  

2. **Category**  
   - Id, Name, ImagePath  
   - Relație cu `Product`  

3. **User** (Identity)  
   - Id, UserName, Email  
   - Roluri: `User`, `Collaborator`, `Admin`  

4. **Cart / CartItem**  
   - Gestionarea coșului de cumpărături pentru fiecare utilizator  

5. **Wishlist / WishlistItem**  
   - Gestionarea listei de favorite pentru fiecare utilizator  

---

## Posibilități și extinderi

- Integrare **plăți online** (Stripe, PayPal)  
- Sistem de **rating și recenzii** pentru produse  
- Filtrare avansată și sugestii personalizate  
- Dashboard admin cu statistici despre vânzări și stocuri  
- Responsivitate completă pentru mobile și tablete  

---

## Tehnologii utilizate

- **C# / .NET Core 7**  
- **ASP.NET Core MVC**  
- **Entity Framework Core** (pentru acces la baza de date)  
- **Bootstrap 5** pentru design și layout  
- **Font Awesome** pentru icon-uri  

---

## Instalare și rulare

1. Clonează proiectul:  
   ```bash
   git clone https://github.com/username/heritage.git
