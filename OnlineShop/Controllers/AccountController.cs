using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineShop.Models;
using System.ComponentModel.DataAnnotations;

namespace OnlineShop.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _context = context;
        }

        // ================== Register/Login/Logout/AccessDenied/ChangePassword/Profile ==================

        [HttpGet]
        public IActionResult Register(string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model, string? returnUrl = null)
        {
            returnUrl ??= Url.Action("Index", "Home");

            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Address = model.Address
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    user.EmailConfirmed = true;
                    await _userManager.UpdateAsync(user);
                    
                    await _userManager.AddToRoleAsync(user, "User");
                    await _signInManager.SignInAsync(user, isPersistent: false);

                    TempData["Success"] = "Cont creat cu succes! Bine ai venit!";
                    if (Url.IsLocalUrl(returnUrl)) return LocalRedirect(returnUrl!);
                    return RedirectToAction("Index", "Home");
                }

                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            returnUrl ??= Url.Action("Index", "Home");

            if (!ModelState.IsValid) return View(model);

            var result = await _signInManager.PasswordSignInAsync(
                model.Email, model.Password, model.RememberMe, lockoutOnFailure: true);

            if (result.Succeeded)
            {
                TempData["Success"] = "Te-ai autentificat cu succes!";
                return LocalRedirect(returnUrl ?? "/");
            }
            if (result.IsLockedOut)
            {
                ModelState.AddModelError("", "Contul a fost blocat temporar.");
                return View(model);
            }

            ModelState.AddModelError("", "Email sau parolă incorectă.");
            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            TempData["Success"] = "Te-ai deconectat cu succes!";
            return RedirectToAction("Index", "Home");
        }

        public IActionResult AccessDenied() => View();

        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var roles = await _userManager.GetRolesAsync(user);

            var model = new ProfileViewModel
            {
                Email = user.Email!,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Address = user.Address,
                PhoneNumber = user.PhoneNumber,
                Roles = roles.ToList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Profile(ProfileViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            if (ModelState.IsValid)
            {
                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.Address = model.Address;
                user.PhoneNumber = model.PhoneNumber;

                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    TempData["Success"] = "Profilul a fost actualizat cu succes!";
                    return RedirectToAction(nameof(Profile));
                }

                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
            }

            var roles = await _userManager.GetRolesAsync(user);
            model.Roles = roles.ToList();
            return View(model);
        }

        [Authorize]
        public IActionResult ChangePassword() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if (result.Succeeded)
            {
                await _signInManager.RefreshSignInAsync(user);
                TempData["Success"] = "Parola a fost schimbată cu succes!";
                return RedirectToAction(nameof(Profile));
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return View(model);
        }

        // ================== Request Collaborator ==================
        [HttpPost]
        [Authorize(Roles = "User")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RequestCollaborator()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            // Verifică dacă există deja o cerere pending
            var existingRequest = await _context.CollaboratorRequests
                .FirstOrDefaultAsync(r => r.UserId == user.Id && r.Status == "Pending");

            if (existingRequest != null)
            {
                TempData["Error"] = "Ai deja o cerere în așteptare!";
                return RedirectToAction("Profile");
            }

            var request = new CollaboratorRequest
            {
                UserId = user.Id,
                RequestedAt = DateTime.UtcNow,
                Status = "Pending"
            };

            _context.CollaboratorRequests.Add(request);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Cererea ta a fost trimisă! Vei fi notificat când este aprobată.";
            return RedirectToAction("Profile");
        }

    }

    // ================== View Models ==================

    public class RegisterViewModel
    {
        [Required, EmailAddress]
        public string Email { get; set; } = null!;

        [Required, StringLength(100, MinimumLength = 6), DataType(DataType.Password)]
        public string Password { get; set; } = null!;

        [DataType(DataType.Password), Compare("Password")]
        public string ConfirmPassword { get; set; } = null!;

        [StringLength(50)] public string? FirstName { get; set; }
        [StringLength(50)] public string? LastName { get; set; }
        [StringLength(200)] public string? Address { get; set; }
    }

    public class LoginViewModel
    {
        [Required, EmailAddress] public string Email { get; set; } = null!;
        [Required, DataType(DataType.Password)] public string Password { get; set; } = null!;
        [Display(Name = "Ține-mă minte")] public bool RememberMe { get; set; }
    }

    public class ProfileViewModel
    {
        [EmailAddress] public string Email { get; set; } = null!;
        [StringLength(50)] public string? FirstName { get; set; }
        [StringLength(50)] public string? LastName { get; set; }
        [StringLength(200)] public string? Address { get; set; }
        [Phone] public string? PhoneNumber { get; set; }
        public List<string> Roles { get; set; } = new();
    }

    public class ChangePasswordViewModel
    {
        [Required, DataType(DataType.Password)] public string CurrentPassword { get; set; } = null!;
        [Required, StringLength(100, MinimumLength = 6), DataType(DataType.Password)] public string NewPassword { get; set; } = null!;
        [DataType(DataType.Password), Compare("NewPassword")] public string ConfirmPassword { get; set; } = null!;
    }
}
