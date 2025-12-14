namespace OnlineShop.Models
{
    // Status pentru produsele propuse de colaboratori
    public enum ProductStatus
    {
        Pending,    // În așteptare - așteaptă aprobare
        Approved,   // Aprobat - vizibil în magazin
        Rejected    // Respins - nu va fi afișat
    }

    // Status pentru comenzi
    public enum OrderStatus
    {
        Pending,     // În așteptare
        Processing,  // În procesare
        Shipped,     // Expediată
        Delivered,   // Livrată
        Cancelled    // Anulată
    }
}