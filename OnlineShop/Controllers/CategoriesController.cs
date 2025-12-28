using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineShop.Models;

namespace OnlineShop.Controllers
{
    [Authorize(Roles = "Admin")]
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CategoriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Categories
        public async Task<IActionResult> Index()
        {
            var categories = await _context.Categories
                .Include(c => c.Products) // Include produsele
                .ToListAsync();
            return View(categories);
        }

        // GET: /Categories/Create
        public IActionResult Create() => View();

        // POST: /Categories/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category category, IFormFile? image)
        {
            if (!ModelState.IsValid)
                return View(category);

            if (image != null && image.Length > 0)
            {
                var uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/categories");

                if (!Directory.Exists(uploadFolder))
                    Directory.CreateDirectory(uploadFolder);

                var fileName = Guid.NewGuid() + Path.GetExtension(image.FileName);
                var filePath = Path.Combine(uploadFolder, fileName);

                using var stream = new FileStream(filePath, FileMode.Create);
                await image.CopyToAsync(stream);

                category.ImagePath = "/images/categories/" + fileName;
            }

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Categoria a fost adăugată cu succes!";
            return RedirectToAction(nameof(Index));
        }


        // GET: /Categories/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var category = await _context.Categories.FindAsync(id);
            if (category == null) return NotFound();

            return View(category);
        }

        // POST: /Categories/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Category category, IFormFile? image)
        {
            if (id != category.Id)
                return NotFound();

            if (!ModelState.IsValid)
                return View(category);

            var existingCategory = await _context.Categories.FindAsync(id);
            if (existingCategory == null)
                return NotFound();

            existingCategory.Name = category.Name;

            if (image != null && image.Length > 0)
            {
                var uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/categories");

                if (!Directory.Exists(uploadFolder))
                    Directory.CreateDirectory(uploadFolder);

                var fileName = Guid.NewGuid() + Path.GetExtension(image.FileName);
                var filePath = Path.Combine(uploadFolder, fileName);

                using var stream = new FileStream(filePath, FileMode.Create);
                await image.CopyToAsync(stream);

                existingCategory.ImagePath = "/images/categories/" + fileName;
            }

            _context.Update(existingCategory);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Categoria a fost actualizată cu succes!";
            return RedirectToAction(nameof(Index));
        }


        // GET: /Categories/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var category = await _context.Categories
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null) return NotFound();

            return View(category);
        }

        // GET: /Categories/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var category = await _context.Categories
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null) return NotFound();

            return View(category);
        }

        // POST: /Categories/DeleteConfirmed/5
        [HttpPost, ActionName("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _context.Categories
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null) return NotFound();

            // șterge imaginea
            if (!string.IsNullOrEmpty(category.ImagePath))
            {
                var imagePath = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot",
                    category.ImagePath.TrimStart('/')
                );

                if (System.IO.File.Exists(imagePath))
                    System.IO.File.Delete(imagePath);
            }

            if (category.Products != null && category.Products.Any())
            {
                _context.Products.RemoveRange(category.Products);
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Categoria și produsele au fost șterse!";
            return RedirectToAction(nameof(Index));
        }


        private bool CategoryExists(int id)
        {
            return _context.Categories.Any(e => e.Id == id);
        }
    }
}
