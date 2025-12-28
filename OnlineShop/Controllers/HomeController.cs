using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineShop.Models;
using OnlineShop.Models.Enums;


namespace OnlineShop.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ======================================================
        // GET: / sau /Home/Index
        // Pagina principală – listă produse aprobate
        // ======================================================
        public async Task<IActionResult> Index(
            string? search,
            int? categoryId,
            string? sortOrder,
            int page = 1)
        {
            int pageSize = 9;

            var products = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Reviews)
                .Where(p => p.Status == ProductStatus.Approved)
                .AsQueryable();

            // Căutare
            if (!string.IsNullOrWhiteSpace(search))
            {
                products = products.Where(p =>
                    p.Title.Contains(search) ||
                    p.Description.Contains(search));

                ViewBag.Search = search;
            }

            // Filtrare categorie
            if (categoryId.HasValue)
            {
                products = products.Where(p => p.CategoryId == categoryId.Value);
                ViewBag.CategoryId = categoryId;
            }

            // Sortare
            products = sortOrder switch
            {
                "price_asc" => products.OrderBy(p => p.Price),
                "price_desc" => products.OrderByDescending(p => p.Price),
                "rating_asc" => products.OrderBy(p => p.AverageRating ?? 0),
                "rating_desc" => products.OrderByDescending(p => p.AverageRating ?? 0),
                "newest" => products.OrderByDescending(p => p.CreatedAt),
                _ => products.OrderByDescending(p => p.CreatedAt)
            };

            ViewBag.SortOrder = sortOrder;

            // Paginare
            int totalProducts = await products.CountAsync();
            int totalPages = (int)Math.Ceiling(totalProducts / (double)pageSize);

            var paginatedProducts = await products
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Categories = await _context.Categories.ToListAsync();
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View(paginatedProducts);
        }

        // ======================================================
        // GET: /Home/Details/5
        // Detalii produs + stare Wishlist
        // ======================================================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Color)
                .Include(p => p.Materials)
                .Include(p => p.Reviews)
                    .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(p =>
                    p.Id == id &&
                    p.Status == ProductStatus.Approved);

            if (product == null)
                return NotFound();

            // ===== WISHLIST =====
            bool inWishlist = false;
            var user = await _userManager.GetUserAsync(User);

            if (user != null && (User.IsInRole("User") || User.IsInRole("Collaborator")))
            {
                inWishlist = await _context.WishlistItems
                    .Include(wi => wi.Wishlist)
                    .AnyAsync(wi =>
                        wi.ProductId == product.Id &&
                        wi.Wishlist!.UserId == user.Id);
            }

            ViewBag.InWishlist = inWishlist;

            // ===== DATE PENTRU SELECT =====

            // Culori (din DB)
            ViewBag.Colors = await _context.Colors.ToListAsync();

            // Mărimi (din enum)
            ViewBag.Sizes = Enum.GetValues(typeof(ProductSize))
                .Cast<ProductSize>()
                .ToList();

            // Materiale (deja incluse, doar pentru claritate)
            ViewBag.Materials = product.Materials;

            return View(product);
        }


        // ======================================================
        // Pagini standard
        // ======================================================
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }
    }
}
