using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineShop.Models;

namespace OnlineShop.Controllers
{
    [Authorize(Roles = "User,Collaborator")]
    public class CartsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CartsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /Carts
        // Vizualizare coș
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var cart = await GetOrCreateCart(user!.Id);

            var cartItems = await _context.CartItems
                .Include(ci => ci.Product)
                    .ThenInclude(p => p!.Category)
                .Where(ci => ci.CartId == cart.Id)
                .ToListAsync();

            ViewBag.TotalPrice = cartItems
                .Where(ci => ci.Product != null)
                .Sum(ci => ci.Quantity * ci.Product!.Price);

            return View(cartItems);
        }

        // POST: /Carts/Add
        // Adăugare produs în coș
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(int productId, int quantity = 1)
        {
            // Verifică dacă produsul există și e disponibil
            var product = await _context.Products.FindAsync(productId);
            if (product == null || product.Status != ProductStatus.Approved)
            {
                TempData["Error"] = "Produsul nu este disponibil!";
                return RedirectToAction("Index", "Home");
            }

            // Verifică stocul
            if (product.Stock < quantity)
            {
                TempData["Error"] = $"Stoc insuficient! Disponibil: {product.Stock} bucăți.";
                return RedirectToAction("Details", "Home", new { id = productId });
            }

            var user = await _userManager.GetUserAsync(User);
            var cart = await GetOrCreateCart(user!.Id);

            // Verifică dacă produsul e deja în coș
            var existingItem = await _context.CartItems
                .FirstOrDefaultAsync(ci => ci.CartId == cart.Id && ci.ProductId == productId);

            if (existingItem != null)
            {
                // Verifică dacă noua cantitate depășește stocul
                int newQuantity = existingItem.Quantity + quantity;
                if (newQuantity > product.Stock)
                {
                    TempData["Error"] = $"Nu poți adăuga mai mult de {product.Stock} bucăți!";
                    return RedirectToAction("Details", "Home", new { id = productId });
                }

                existingItem.Quantity = newQuantity;
            }
            else
            {
                var cartItem = new CartItem
                {
                    CartId = cart.Id,
                    ProductId = productId,
                    Quantity = quantity,
                    AddedAt = DateTime.UtcNow
                };
                _context.CartItems.Add(cartItem);
            }

            cart.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            TempData["Success"] = $"'{product.Title}' a fost adăugat în coș!";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Carts/UpdateQuantity
        // Actualizare cantitate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateQuantity(int cartItemId, int quantity)
        {
            var user = await _userManager.GetUserAsync(User);
            var cartItem = await _context.CartItems
                .Include(ci => ci.Cart)
                .Include(ci => ci.Product)
                .FirstOrDefaultAsync(ci => ci.Id == cartItemId && ci.Cart!.UserId == user!.Id);

            if (cartItem == null)
            {
                return NotFound();
            }

            if (quantity <= 0)
            {
                // Șterge item-ul dacă cantitatea e 0 sau mai mică
                _context.CartItems.Remove(cartItem);
                TempData["Success"] = "Produsul a fost eliminat din coș!";
            }
            else if (quantity > cartItem.Product!.Stock)
            {
                TempData["Error"] = $"Stoc insuficient! Disponibil: {cartItem.Product.Stock} bucăți.";
            }
            else
            {
                cartItem.Quantity = quantity;
                TempData["Success"] = "Cantitatea a fost actualizată!";
            }

            cartItem.Cart!.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // POST: /Carts/Remove
        // Eliminare produs din coș
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(int cartItemId)
        {
            var user = await _userManager.GetUserAsync(User);
            var cartItem = await _context.CartItems
                .Include(ci => ci.Cart)
                .Include(ci => ci.Product)
                .FirstOrDefaultAsync(ci => ci.Id == cartItemId && ci.Cart!.UserId == user!.Id);

            if (cartItem == null)
            {
                return NotFound();
            }

            string productTitle = cartItem.Product?.Title ?? "Produs";

            _context.CartItems.Remove(cartItem);
            cartItem.Cart!.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            TempData["Success"] = $"'{productTitle}' a fost eliminat din coș!";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Carts/Clear
        // Golire coș
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Clear()
        {
            var user = await _userManager.GetUserAsync(User);
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == user!.Id);

            if (cart != null && cart.CartItems != null)
            {
                _context.CartItems.RemoveRange(cart.CartItems);
                cart.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            TempData["Success"] = "Coșul a fost golit!";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Carts/AddFromWishlist
        // Adăugare produs din wishlist în coș
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddFromWishlist(int wishlistItemId)
        {
            var user = await _userManager.GetUserAsync(User);

            var wishlistItem = await _context.WishlistItems
                .Include(wi => wi.Wishlist)
                .Include(wi => wi.Product)
                .FirstOrDefaultAsync(wi => wi.Id == wishlistItemId && wi.Wishlist!.UserId == user!.Id);

            if (wishlistItem == null)
            {
                return NotFound();
            }

            var product = wishlistItem.Product!;

            // Verifică dacă produsul e disponibil
            if (product.Status != ProductStatus.Approved || product.Stock <= 0)
            {
                TempData["Error"] = "Produsul nu este disponibil!";
                return RedirectToAction("Index", "Wishlists");
            }

            var cart = await GetOrCreateCart(user!.Id);

            // Verifică dacă produsul e deja în coș
            var existingCartItem = await _context.CartItems
                .FirstOrDefaultAsync(ci => ci.CartId == cart.Id && ci.ProductId == product.Id);

            if (existingCartItem != null)
            {
                if (existingCartItem.Quantity < product.Stock)
                {
                    existingCartItem.Quantity++;
                }
                else
                {
                    TempData["Error"] = "Nu poți adăuga mai multe produse - stoc insuficient!";
                    return RedirectToAction("Index", "Wishlists");
                }
            }
            else
            {
                var cartItem = new CartItem
                {
                    CartId = cart.Id,
                    ProductId = product.Id,
                    Quantity = 1,
                    AddedAt = DateTime.UtcNow
                };
                _context.CartItems.Add(cartItem);
            }

            // Opțional: elimină din wishlist
            _context.WishlistItems.Remove(wishlistItem);

            cart.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            TempData["Success"] = $"'{product.Title}' a fost mutat în coș!";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Carts/Count
        // Returnează numărul de produse din coș (pentru navbar)
        [Authorize(Roles = "User,Collaborator")]
        public async Task<IActionResult> Count()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Json(new { count = 0 });
            }

            var count = await _context.CartItems
                .Include(ci => ci.Cart)
                .Where(ci => ci.Cart!.UserId == user.Id)
                .SumAsync(ci => ci.Quantity);

            return Json(new { count });
        }

        // ===== HELPER METHODS =====

        private async Task<Cart> GetOrCreateCart(string userId)
        {
            var cart = await _context.Carts
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new Cart
                {
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            return cart;
        }
    }
}