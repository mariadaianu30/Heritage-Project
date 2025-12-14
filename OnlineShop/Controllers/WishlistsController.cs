using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineShop.Models;

namespace OnlineShop.Controllers
{
    [Authorize(Roles = "User,Collaborator")]
    public class WishlistsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public WishlistsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // =======================
        // GET: /Wishlists
        // =======================
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);

            var wishlist = await _context.Wishlists
                .Include(w => w.WishlistItems)
                    .ThenInclude(wi => wi.Product)
                        .ThenInclude(p => p!.Category)
                .FirstOrDefaultAsync(w => w.UserId == user!.Id);

            if (wishlist == null)
            {
                wishlist = new Wishlist
                {
                    UserId = user!.Id
                };

                _context.Wishlists.Add(wishlist);
                await _context.SaveChangesAsync();
            }

            return View(wishlist);
        }

        // =======================
        // POST: /Wishlists/Add
        // =======================
        [HttpPost]
        [Authorize(Roles = "User,Collaborator")]
        public async Task<IActionResult> Add(int productId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var wishlist = await _context.Wishlists
                .Include(w => w.WishlistItems)
                .FirstOrDefaultAsync(w => w.UserId == user.Id);

            if (wishlist == null)
            {
                wishlist = new Wishlist
                {
                    UserId = user.Id
                };
                _context.Wishlists.Add(wishlist);
                await _context.SaveChangesAsync();
            }

            bool exists = wishlist.WishlistItems
                .Any(wi => wi.ProductId == productId);

            if (!exists)
            {
                wishlist.WishlistItems.Add(new WishlistItem
                {
                    ProductId = productId
                });

                await _context.SaveChangesAsync();
            }

            return Redirect(Request.Headers["Referer"].ToString());

        }


        // =======================
        // POST: /Wishlists/Remove/5
        // =======================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            var item = await _context.WishlistItems
                .Include(wi => wi.Wishlist)
                .FirstOrDefaultAsync(wi =>
                    wi.Id == id &&
                    wi.Wishlist!.UserId == user!.Id);

            if (item == null)
            {
                return NotFound();
            }

            _context.WishlistItems.Remove(item);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Produsul a fost eliminat din wishlist!";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Count()
        {
            var user = await _userManager.GetUserAsync(User);

            var count = await _context.WishlistItems
                .CountAsync(wi => wi.Wishlist!.UserId == user!.Id);

            return Json(new { count });
        }

        // =======================
        // POST: /Wishlists/Clear
        // =======================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Clear()
        {
            var user = await _userManager.GetUserAsync(User);

            var wishlist = await _context.Wishlists
                .Include(w => w.WishlistItems)
                .FirstOrDefaultAsync(w => w.UserId == user!.Id);

            if (wishlist == null || wishlist.WishlistItems == null)
            {
                return RedirectToAction(nameof(Index));
            }

            _context.WishlistItems.RemoveRange(wishlist.WishlistItems);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Wishlist-ul a fost golit!";
            return RedirectToAction(nameof(Index));
        }
    }
}
