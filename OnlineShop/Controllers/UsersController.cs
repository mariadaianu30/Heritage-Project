using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OnlineShop.Models;
using System.ComponentModel.DataAnnotations;

namespace OnlineShop.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UsersController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // GET: /Users
        // Lista tuturor utilizatorilor
        public async Task<IActionResult> Index(string? role, string? search)
        {
            var users = _userManager.Users.AsQueryable();

            // Filtrare după căutare
            if (!string.IsNullOrEmpty(search))
            {
                users = users.Where(u => u.Email!.Contains(search) ||
                                        (u.FirstName != null && u.FirstName.Contains(search)) ||
                                        (u.LastName != null && u.LastName.Contains(search)));
                ViewBag.Search = search;
            }

            var userList = await users.OrderBy(u => u.Email).ToListAsync();

            // Construiește lista cu roluri
            var userViewModels = new List<UserViewModel>();
            foreach (var user in userList)
            {
                var roles = await _userManager.GetRolesAsync(user);

                // Filtrare după rol
                if (!string.IsNullOrEmpty(role) && !roles.Contains(role))
                {
                    continue;
                }

                userViewModels.Add(new UserViewModel
                {
                    Id = user.Id,
                    Email = user.Email!,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Roles = roles.ToList(),
                    CreatedAt = user.CreatedAt
                });
            }

            ViewBag.Role = role;
            ViewBag.Roles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();

            return View(userViewModels);
        }

        // GET: /Users/Details/id
        public async Task<IActionResult> Details(string? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var roles = await _userManager.GetRolesAsync(user);

            // Statistici utilizator
            var ordersCount = await _context.Orders.CountAsync(o => o.UserId == id);
            var reviewsCount = await _context.Reviews.CountAsync(r => r.UserId == id);
            var productsCount = await _context.Products.CountAsync(p => p.CollaboratorId == id);
            var pendingRequests = await _context.CollaboratorRequests
                .Where(r => r.UserId == user.Id && r.Status == "Pending")
                .ToListAsync();

            ViewBag.CollabRequests = pendingRequests;

            var model = new UserDetailsViewModel
            {
                Id = user.Id,
                Email = user.Email!,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Address = user.Address,
                PhoneNumber = user.PhoneNumber,
                Roles = roles.ToList(),
                CreatedAt = user.CreatedAt,
                OrdersCount = ordersCount,
                ReviewsCount = reviewsCount,
                ProductsCount = productsCount
            };

            return View(model);
        }

        // GET: /Users/Edit/id
        // Editare utilizator (rol)
        public async Task<IActionResult> Edit(string? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var userRoles = await _userManager.GetRolesAsync(user);
            var allRoles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();

            var model = new UserEditViewModel
            {
                Id = user.Id,
                Email = user.Email!,
                FirstName = user.FirstName,
                LastName = user.LastName,
                CurrentRoles = userRoles.ToList(),
                SelectedRole = userRoles.FirstOrDefault() ?? "User"
            };

            ViewBag.AllRoles = new SelectList(allRoles, model.SelectedRole);

            return View(model);
        }

        // POST: /Users/Edit/id
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, UserEditViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // Nu permite editarea propriului rol
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser!.Id == user.Id)
            {
                TempData["Error"] = "Nu îți poți modifica propriul rol!";
                return RedirectToAction(nameof(Index));
            }

            if (ModelState.IsValid)
            {
                // Actualizează datele utilizatorului
                user.FirstName = model.FirstName;
                user.LastName = model.LastName;

                var updateResult = await _userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                {
                    foreach (var error in updateResult.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                    return View(model);
                }

                // Actualizează rolul
                var currentRoles = await _userManager.GetRolesAsync(user);

                // Elimină toate rolurile curente
                if (currentRoles.Any())
                {
                    await _userManager.RemoveFromRolesAsync(user, currentRoles);
                }

                // Adaugă noul rol
                if (!string.IsNullOrEmpty(model.SelectedRole))
                {
                    await _userManager.AddToRoleAsync(user, model.SelectedRole);
                }

                TempData["Success"] = $"Utilizatorul '{user.Email}' a fost actualizat!";
                return RedirectToAction(nameof(Index));
            }

            var allRoles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
            ViewBag.AllRoles = new SelectList(allRoles, model.SelectedRole);

            return View(model);
        }

        // GET: /Users/Delete/id
        public async Task<IActionResult> Delete(string? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // Nu permite ștergerea propriului cont
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser!.Id == user.Id)
            {
                TempData["Error"] = "Nu îți poți șterge propriul cont!";
                return RedirectToAction("Index");
            }

            string email = user.Email!;
                var result = await _userManager.DeleteAsync(user);

                if (result.Succeeded)
                {
                    TempData["Success"] = $"Utilizatorul '{email}' a fost șters!";
                }
                else
                {
                    TempData["Error"] = "Eroare la ștergerea utilizatorului!";
                }

                return RedirectToAction("Index");
        }

        

        // POST: /Users/ChangeRole
        // Schimbare rapidă rol
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeRole(string userId, string newRole)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            // Nu permite schimbarea propriului rol
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser!.Id == user.Id)
            {
                TempData["Error"] = "Nu îți poți modifica propriul rol!";
                return RedirectToAction(nameof(Index));
            }

            // Verifică dacă rolul există
            if (!await _roleManager.RoleExistsAsync(newRole))
            {
                TempData["Error"] = "Rolul nu există!";
                return RedirectToAction(nameof(Index));
            }

            // Elimină toate rolurile curente
            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);

            // Adaugă noul rol
            await _userManager.AddToRoleAsync(user, newRole);

            TempData["Success"] = $"Rolul utilizatorului '{user.Email}' a fost schimbat la '{newRole}'!";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Users/Create
        // Creare utilizator nou (de Admin)
        public async Task<IActionResult> Create()
        {
            var allRoles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
            ViewBag.AllRoles = new SelectList(allRoles, "User");

            return View();
        }

        // POST: /Users/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Address = model.Address,
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, model.Role);

                    TempData["Success"] = $"Utilizatorul '{user.Email}' a fost creat cu rolul '{model.Role}'!";
                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            var allRoles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
            ViewBag.AllRoles = new SelectList(allRoles, model.Role);

            return View(model);
        }

        // Lista cererilor pending
        public async Task<IActionResult> CollaboratorRequests()
        {
            var requests = await _context.CollaboratorRequests
                .Where(r => r.Status == "Pending")
                .OrderBy(r => r.RequestedAt)
                .ToListAsync();

            return View(requests);
        }

        // Aproba cererea
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveCollaborator(int requestId)
        {
            var request = await _context.CollaboratorRequests.FindAsync(requestId);
            if (request == null) return NotFound();

            // schimbă rolul user-ului
            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user != null)
            {
                await _userManager.AddToRoleAsync(user, "Collaborator");
            }

            // marchează cererea ca Approved
            request.Status = "Approved";
            await _context.SaveChangesAsync();

            TempData["Success"] = "Cererea a fost aprobată!";
            return RedirectToAction("Index"); // sau Details(userId)
        }


        // Respinge cererea
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DenyCollaborator(int id, string? feedback)
        {
            var request = await _context.CollaboratorRequests.FindAsync(id);
            if (request == null) return NotFound();

            request.Status = "Denied";
            _context.CollaboratorRequests.Update(request);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Cererea lui {request.UserId} a fost respinsă!";
            return RedirectToAction(nameof(CollaboratorRequests));
        }

    }

    // ===== VIEW MODELS =====

    public class UserViewModel
    {
        public string Id { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public List<string> Roles { get; set; } = new();
        public DateTime CreatedAt { get; set; }

        public string FullName => $"{FirstName} {LastName}".Trim();
    }

    public class UserDetailsViewModel : UserViewModel
    {
        public string? Address { get; set; }
        public string? PhoneNumber { get; set; }
        public int OrdersCount { get; set; }
        public int ReviewsCount { get; set; }
        public int ProductsCount { get; set; }
    }

    public class UserEditViewModel
    {
        public string Id { get; set; } = null!;

        [Display(Name = "Email")]
        public string Email { get; set; } = null!;

        [StringLength(50)]
        [Display(Name = "Prenume")]
        public string? FirstName { get; set; }

        [StringLength(50)]
        [Display(Name = "Nume")]
        public string? LastName { get; set; }

        public List<string> CurrentRoles { get; set; } = new();

        [Required(ErrorMessage = "Selectează un rol")]
        [Display(Name = "Rol")]
        public string SelectedRole { get; set; } = null!;
    }

    public class UserCreateViewModel
    {
        [Required(ErrorMessage = "Email-ul este obligatoriu")]
        [EmailAddress(ErrorMessage = "Format email invalid")]
        [Display(Name = "Email")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Parola este obligatorie")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Parola trebuie să aibă cel puțin 6 caractere")]
        [DataType(DataType.Password)]
        [Display(Name = "Parolă")]
        public string Password { get; set; } = null!;

        [DataType(DataType.Password)]
        [Display(Name = "Confirmă parola")]
        [Compare("Password", ErrorMessage = "Parolele nu coincid")]
        public string ConfirmPassword { get; set; } = null!;

        [StringLength(50)]
        [Display(Name = "Prenume")]
        public string? FirstName { get; set; }

        [StringLength(50)]
        [Display(Name = "Nume")]
        public string? LastName { get; set; }

        [StringLength(200)]
        [Display(Name = "Adresă")]
        public string? Address { get; set; }

        [Required(ErrorMessage = "Selectează un rol")]
        [Display(Name = "Rol")]
        public string Role { get; set; } = "User";
    }
}