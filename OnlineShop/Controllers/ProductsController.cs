using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OnlineShop.Models;
using System.ComponentModel.DataAnnotations;

namespace OnlineShop.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: /Products
        // Lista produselor (Admin vede toate, Collaborator vede doar ale lui)
        [Authorize(Roles = "Admin,Collaborator")]
        public async Task<IActionResult> Index(ProductStatus? status)
        {
            var user = await _userManager.GetUserAsync(User);
            var isAdmin = User.IsInRole("Admin");

            var products = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Collaborator)
                .AsQueryable();

            // Collaboratorul vede doar produsele proprii
            if (!isAdmin)
            {
                products = products.Where(p => p.CollaboratorId == user!.Id);
            }

            // Filtrare după status
            if (status.HasValue)
            {
                products = products.Where(p => p.Status == status.Value);
                ViewBag.Status = status;
            }

            ViewBag.IsAdmin = isAdmin;

            return View(await products.OrderByDescending(p => p.CreatedAt).ToListAsync());
        }

        // GET: /Products/Pending
        // Produse în așteptare pentru aprobare (doar Admin)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Pending()
        {
            var products = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Collaborator)
                .Where(p => p.Status == ProductStatus.Pending)
                .OrderBy(p => p.CreatedAt)
                .ToListAsync();

            return View(products);
        }

        // GET: /Products/Details/5
        [Authorize(Roles = "Admin,Collaborator")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Collaborator)
                .Include(p => p.Reviews)
                    .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            // Verifică dacă utilizatorul are acces
            var user = await _userManager.GetUserAsync(User);
            var isAdmin = User.IsInRole("Admin");

            if (!isAdmin && product.CollaboratorId != user!.Id)
            {
                return Forbid();
            }

            return View(product);
        }

        // GET: /Products/Create
        [Authorize(Roles = "Admin,Collaborator")]
        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = new SelectList(await _context.Categories.ToListAsync(), "Id", "Name");
            return View();
        }

        // POST: /Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Collaborator")]
        public async Task<IActionResult> Create(ProductCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                var isAdmin = User.IsInRole("Admin");

                // Upload imagine
                string imagePath = await UploadImage(model.Image);

                var product = new Product
                {
                    Title = model.Title,
                    Description = model.Description,
                    ImagePath = imagePath,
                    Price = model.Price,
                    Stock = model.Stock,
                    CategoryId = model.CategoryId,
                    CollaboratorId = isAdmin ? null : user!.Id,
                    // Admin-ul adaugă direct produse aprobate, Collaboratorul trebuie să aștepte
                    Status = isAdmin ? ProductStatus.Approved : ProductStatus.Pending,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Add(product);
                await _context.SaveChangesAsync();

                if (isAdmin)
                {
                    TempData["Success"] = "Produsul a fost adăugat cu succes!";
                }
                else
                {
                    TempData["Success"] = "Produsul a fost trimis spre aprobare!";
                }

                return RedirectToAction(nameof(Index));
            }

            ViewBag.Categories = new SelectList(await _context.Categories.ToListAsync(), "Id", "Name", model.CategoryId);
            return View(model);
        }

        // GET: /Products/Edit/5
        [Authorize(Roles = "Admin,Collaborator")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            // Verifică dacă utilizatorul are acces
            var user = await _userManager.GetUserAsync(User);
            var isAdmin = User.IsInRole("Admin");

            if (!isAdmin && product.CollaboratorId != user!.Id)
            {
                return Forbid();
            }

            var model = new ProductEditViewModel
            {
                Id = product.Id,
                Title = product.Title,
                Description = product.Description,
                CurrentImagePath = product.ImagePath,
                Price = product.Price,
                Stock = product.Stock,
                CategoryId = product.CategoryId,
                Status = product.Status
            };

            ViewBag.Categories = new SelectList(await _context.Categories.ToListAsync(), "Id", "Name", product.CategoryId);
            ViewBag.IsAdmin = isAdmin;

            return View(model);
        }

        // POST: /Products/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Collaborator")]
        public async Task<IActionResult> Edit(int id, ProductEditViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            // Verifică dacă utilizatorul are acces
            var user = await _userManager.GetUserAsync(User);
            var isAdmin = User.IsInRole("Admin");

            if (!isAdmin && product.CollaboratorId != user!.Id)
            {
                return Forbid();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    product.Title = model.Title;
                    product.Description = model.Description;
                    product.Price = model.Price;
                    product.Stock = model.Stock;
                    product.CategoryId = model.CategoryId;
                    product.UpdatedAt = DateTime.UtcNow;

                    // Upload imagine nouă dacă există
                    if (model.NewImage != null)
                    {
                        // Șterge imaginea veche
                        DeleteImage(product.ImagePath);
                        product.ImagePath = await UploadImage(model.NewImage);
                    }

                    // Colaboratorul care editează - produsul revine în Pending
                    if (!isAdmin && product.Status == ProductStatus.Approved)
                    {
                        product.Status = ProductStatus.Pending;
                        product.AdminFeedback = null;
                        TempData["Warning"] = "Produsul a fost trimis pentru re-aprobare!";
                    }
                    else
                    {
                        TempData["Success"] = "Produsul a fost actualizat cu succes!";
                    }

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.Id))
                    {
                        return NotFound();
                    }
                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            ViewBag.Categories = new SelectList(await _context.Categories.ToListAsync(), "Id", "Name", model.CategoryId);
            ViewBag.IsAdmin = isAdmin;

            return View(model);
        }

        // GET: /Products/Delete/5
        [Authorize(Roles = "Admin,Collaborator")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Collaborator)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            // Verifică dacă utilizatorul are acces
            var user = await _userManager.GetUserAsync(User);
            var isAdmin = User.IsInRole("Admin");

            if (!isAdmin && product.CollaboratorId != user!.Id)
            {
                return Forbid();
            }

            return View(product);
        }

        // POST: /Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Collaborator")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product != null)
            {
                // Verifică dacă utilizatorul are acces
                var user = await _userManager.GetUserAsync(User);
                var isAdmin = User.IsInRole("Admin");

                if (!isAdmin && product.CollaboratorId != user!.Id)
                {
                    return Forbid();
                }

                // Verifică dacă produsul are comenzi active
                var hasOrders = await _context.OrderItems.AnyAsync(oi => oi.ProductId == id);
                if (hasOrders)
                {
                    TempData["Error"] = "Nu poți șterge un produs care are comenzi asociate!";
                    return RedirectToAction(nameof(Index));
                }

                // Șterge imaginea
                DeleteImage(product.ImagePath);

                _context.Products.Remove(product);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Produsul a fost șters cu succes!";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: /Products/Approve/5
        // Aprobare produs (doar Admin)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Approve(int id, string? feedback)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            product.Status = ProductStatus.Approved;
            product.AdminFeedback = feedback;
            product.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            TempData["Success"] = $"Produsul '{product.Title}' a fost aprobat!";
            return RedirectToAction(nameof(Pending));
        }

        // POST: /Products/Reject/5
        // Respingere produs (doar Admin)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Reject(int id, string feedback)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            if (string.IsNullOrWhiteSpace(feedback))
            {
                TempData["Error"] = "Trebuie să oferi un motiv pentru respingere!";
                return RedirectToAction(nameof(Pending));
            }

            product.Status = ProductStatus.Rejected;
            product.AdminFeedback = feedback;
            product.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            TempData["Success"] = $"Produsul '{product.Title}' a fost respins!";
            return RedirectToAction(nameof(Pending));
        }

        // ===== HELPER METHODS =====

        private async Task<string> UploadImage(IFormFile image)
        {
            string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "products");

            // Creează folderul dacă nu există
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            // Generează nume unic pentru fișier
            string uniqueFileName = Guid.NewGuid().ToString() + "_" + image.FileName;
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await image.CopyToAsync(fileStream);
            }

            return "/images/products/" + uniqueFileName;
        }

        private void DeleteImage(string imagePath)
        {
            if (!string.IsNullOrEmpty(imagePath))
            {
                string fullPath = Path.Combine(_webHostEnvironment.WebRootPath, imagePath.TrimStart('/'));
                if (System.IO.File.Exists(fullPath))
                {
                    System.IO.File.Delete(fullPath);
                }
            }
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }
    }

    // ===== VIEW MODELS =====

    public class ProductCreateViewModel
    {
        [Required(ErrorMessage = "Titlul este obligatoriu")]
        [StringLength(200, MinimumLength = 3, ErrorMessage = "Titlul trebuie să aibă între 3 și 200 caractere")]
        [Display(Name = "Titlu")]
        public string Title { get; set; } = null!;

        [Required(ErrorMessage = "Descrierea este obligatorie")]
        [StringLength(2000, MinimumLength = 10, ErrorMessage = "Descrierea trebuie să aibă între 10 și 2000 caractere")]
        [Display(Name = "Descriere")]
        public string Description { get; set; } = null!;

        [Required(ErrorMessage = "Imaginea este obligatorie")]
        [Display(Name = "Imagine")]
        public IFormFile Image { get; set; } = null!;

        [Required(ErrorMessage = "Prețul este obligatoriu")]
        [Range(0.01, 1000000, ErrorMessage = "Prețul trebuie să fie mai mare decât 0")]
        [Display(Name = "Preț (RON)")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Stocul este obligatoriu")]
        [Range(0, int.MaxValue, ErrorMessage = "Stocul nu poate fi negativ")]
        [Display(Name = "Stoc")]
        public int Stock { get; set; }

        [Required(ErrorMessage = "Categoria este obligatorie")]
        [Display(Name = "Categorie")]
        public int CategoryId { get; set; }
    }

    public class ProductEditViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Titlul este obligatoriu")]
        [StringLength(200, MinimumLength = 3, ErrorMessage = "Titlul trebuie să aibă între 3 și 200 caractere")]
        [Display(Name = "Titlu")]
        public string Title { get; set; } = null!;

        [Required(ErrorMessage = "Descrierea este obligatorie")]
        [StringLength(2000, MinimumLength = 10, ErrorMessage = "Descrierea trebuie să aibă între 10 și 2000 caractere")]
        [Display(Name = "Descriere")]
        public string Description { get; set; } = null!;

        public string CurrentImagePath { get; set; } = null!;

        [Display(Name = "Imagine nouă (opțional)")]
        public IFormFile? NewImage { get; set; }

        [Required(ErrorMessage = "Prețul este obligatoriu")]
        [Range(0.01, 1000000, ErrorMessage = "Prețul trebuie să fie mai mare decât 0")]
        [Display(Name = "Preț (RON)")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Stocul este obligatoriu")]
        [Range(0, int.MaxValue, ErrorMessage = "Stocul nu poate fi negativ")]
        [Display(Name = "Stoc")]
        public int Stock { get; set; }

        [Required(ErrorMessage = "Categoria este obligatorie")]
        [Display(Name = "Categorie")]
        public int CategoryId { get; set; }

        public ProductStatus Status { get; set; }
    }
}