# Heritage - Vintage Clothing Online Store

Heritage is an online store project dedicated to vintage clothing enthusiasts, built using **C#** and **ASP.NET Core MVC**. The project offers a complete browsing and shopping experience, combining elegant design with intuitive functionality.

---

## Main Features

- **Product Catalog**: Browse and filter products by category, price, rating, and new arrivals.  
- **Product Details**: Each product has a dedicated page with full information: title, price, category, stock availability, and image.  
- **Shopping Cart**: Users can add products to their cart and manage their orders.  
- **Wishlist**: Users can add favorite products to a wishlist for easy access later.  
- **Filters and Sorting**: Filter products by category and sort by price, rating, or newest arrivals.  
- **Admin Management**: Admins can add products, manage stock, and handle categories.

---

## Main Models

1. **Product**  
   - Id, Title, Description, Price, Stock, ImagePath  
   - Relationship with `Category`  

2. **Category**  
   - Id, Name, ImagePath  
   - Relationship with `Product`  

3. **User** (Identity)  
   - Id, UserName, Email  
   - Roles: `User`, `Collaborator`, `Admin`  

4. **Cart / CartItem**  
   - Handles each user's shopping cart  

5. **Wishlist / WishlistItem**  
   - Handles each user's list of favorite products  

---

## Possibilities and Extensions

- Integration with **online payments** (Stripe, PayPal)  
- Product **ratings and reviews** system  
- Advanced filtering and personalized suggestions  
- Admin dashboard with sales and stock statistics  
- Fully responsive design for mobile and tablet devices  

---

## Technologies Used

- **C# / .NET Core 7**  
- **ASP.NET Core MVC**  
- **Entity Framework Core** (for database access)  
- **Bootstrap 5** for layout and styling  
- **Font Awesome** for icons  

---

## Installation and Running

1. Clone the project:  
   ```bash
   git clone https://github.com/username/heritage.git

