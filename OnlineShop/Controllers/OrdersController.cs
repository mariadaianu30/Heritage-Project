using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineShop.Models;
using System.ComponentModel.DataAnnotations;

namespace OnlineShop.Controllers
{
    [Authorize(Roles = "User,Admin")]
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public OrdersController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /Orders
        // Lista comenzilor utilizatorului (sau toate pentru Admin)
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var isAdmin = User.IsInRole("Admin");

            var orders = _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .AsQueryable();

            // Admin vede toate comenzile, User doar ale lui
            if (!isAdmin)
            {
                orders = orders.Where(o => o.UserId == user!.Id);
            }

            var orderList = await orders
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            ViewBag.IsAdmin = isAdmin;

            return View(orderList);
        }

        // GET: /Orders/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            var isAdmin = User.IsInRole("Admin");

            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                        .ThenInclude(p => p!.Category)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            // Verifică dacă utilizatorul are acces
            if (!isAdmin && order.UserId != user!.Id)
            {
                return Forbid();
            }

            ViewBag.IsAdmin = isAdmin;

            return View(order);
        }

        // GET: /Orders/Checkout
        // Pagina de plasare comandă
        public async Task<IActionResult> Checkout()
        {
            var user = await _userManager.GetUserAsync(User);

            var cart = await _context.Carts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.UserId == user!.Id);

            if (cart == null || !cart.CartItems.Any())
            {
                TempData["Error"] = "Coșul este gol!";
                return RedirectToAction("Index", "Carts");
            }

            // Verifică disponibilitatea produselor
            var unavailableProducts = cart.CartItems
                .Where(ci => ci.Product == null ||
                            ci.Product.Status != ProductStatus.Approved ||
                            ci.Product.Stock < ci.Quantity)
                .ToList();

            if (unavailableProducts.Any())
            {
                TempData["Error"] = "Unele produse din coș nu mai sunt disponibile sau au stoc insuficient!";
                return RedirectToAction("Index", "Carts");
            }

            var model = new CheckoutViewModel
            {
                ShippingAddress = user!.Address ?? "",
                CartItems = cart.CartItems.ToList(),
                TotalAmount = cart.CartItems.Sum(ci => ci.Quantity * ci.Product!.Price)
            };

            return View(model);
        }

        // POST: /Orders/PlaceOrder
        // Plasare comandă
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder(CheckoutViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);

            var cart = await _context.Carts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.UserId == user!.Id);

            if (cart == null || !cart.CartItems.Any())
            {
                TempData["Error"] = "Coșul este gol!";
                return RedirectToAction("Index", "Carts");
            }

            if (!ModelState.IsValid)
            {
                model.CartItems = cart.CartItems.ToList();
                model.TotalAmount = cart.CartItems.Sum(ci => ci.Quantity * ci.Product!.Price);
                return View("Checkout", model);
            }

            // Verifică din nou disponibilitatea (pentru race conditions)
            foreach (var item in cart.CartItems)
            {
                if (item.Product == null ||
                    item.Product.Status != ProductStatus.Approved ||
                    item.Product.Stock < item.Quantity)
                {
                    TempData["Error"] = $"Produsul '{item.Product?.Title ?? "Unknown"}' nu mai este disponibil!";
                    return RedirectToAction("Index", "Carts");
                }
            }

            // Creează comanda
            var order = new Order
            {
                UserId = user!.Id,
                ShippingAddress = model.ShippingAddress,
                Status = OrderStatus.Pending,
                OrderDate = DateTime.UtcNow,
                TotalAmount = 0
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Adaugă produsele în comandă și actualizează stocul
            foreach (var cartItem in cart.CartItems)
            {
                var orderItem = new OrderItem
                {
                    OrderId = order.Id,
                    ProductId = cartItem.ProductId,
                    Quantity = cartItem.Quantity,
                    UnitPrice = cartItem.Product!.Price
                };

                _context.OrderItems.Add(orderItem);

                // Scade stocul
                cartItem.Product.Stock -= cartItem.Quantity;
            }

            // Calculează totalul
            order.TotalAmount = cart.CartItems.Sum(ci => ci.Quantity * ci.Product!.Price);

            // Golește coșul
            _context.CartItems.RemoveRange(cart.CartItems);

            await _context.SaveChangesAsync();

            TempData["Success"] = $"Comanda #{order.Id} a fost plasată cu succes!";
            return RedirectToAction(nameof(Confirmation), new { id = order.Id });
        }

        // GET: /Orders/Confirmation/5
        // Pagina de confirmare comandă
        public async Task<IActionResult> Confirmation(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);

            var order = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == id && o.UserId == user!.Id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // POST: /Orders/Cancel/5
        // Anulare comandă (doar dacă e în Pending)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var isAdmin = User.IsInRole("Admin");

            var order = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            // Verifică dacă utilizatorul are acces
            if (!isAdmin && order.UserId != user!.Id)
            {
                return Forbid();
            }

            // Doar comenzile în Pending pot fi anulate
            if (order.Status != OrderStatus.Pending)
            {
                TempData["Error"] = "Doar comenzile în așteptare pot fi anulate!";
                return RedirectToAction(nameof(Details), new { id });
            }

            // Restaurează stocul
            foreach (var item in order.OrderItems)
            {
                if (item.Product != null)
                {
                    item.Product.Stock += item.Quantity;
                }
            }

            order.Status = OrderStatus.Cancelled;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Comanda a fost anulată!";
            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: /Orders/UpdateStatus/5 (Admin only)
        // Actualizare status comandă
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateStatus(int id, OrderStatus status)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            // Dacă se anulează, restaurează stocul
            if (status == OrderStatus.Cancelled && order.Status != OrderStatus.Cancelled)
            {
                foreach (var item in order.OrderItems)
                {
                    if (item.Product != null)
                    {
                        item.Product.Stock += item.Quantity;
                    }
                }
            }

            order.Status = status;
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Statusul comenzii #{order.Id} a fost actualizat la {status}!";
            return RedirectToAction(nameof(Details), new { id });
        }
    }

    // ===== VIEW MODELS =====

    public class CheckoutViewModel
    {
        [Required(ErrorMessage = "Adresa de livrare este obligatorie")]
        [StringLength(500, MinimumLength = 10, ErrorMessage = "Adresa trebuie să aibă între 10 și 500 caractere")]
        [Display(Name = "Adresa de livrare")]
        public string ShippingAddress { get; set; } = null!;

        public List<CartItem> CartItems { get; set; } = new();

        public decimal TotalAmount { get; set; }
    }
}