using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace OnlineShop.Models
{
    public static class SeedData
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            using var context = new ApplicationDbContext(
                serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>());

            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            // Creare roluri
            await CreateRolesAsync(roleManager);

            // Creare utilizatori de test
            await CreateUsersAsync(userManager);

            // Creare categorii și produse
            await CreateCategoriesAndProductsAsync(context, userManager);
        }

        private static async Task CreateRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            string[] roles = { "Admin", "Collaborator", "User" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }

        private static async Task CreateUsersAsync(UserManager<ApplicationUser> userManager)
        {
            // Admin
            if (await userManager.FindByEmailAsync("admin@onlineshop.com") == null)
            {
                var admin = new ApplicationUser
                {
                    UserName = "admin@onlineshop.com",
                    Email = "admin@onlineshop.com",
                    FirstName = "Admin",
                    LastName = "Principal",
                    Address = "Strada Victoriei 10, București",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(admin, "Admin123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, "Admin");
                }
            }

            // Collaborator
            if (await userManager.FindByEmailAsync("collaborator@onlineshop.com") == null)
            {
                var collaborator = new ApplicationUser
                {
                    UserName = "collaborator@onlineshop.com",
                    Email = "collaborator@onlineshop.com",
                    FirstName = "Ion",
                    LastName = "Popescu",
                    Address = "Bulevardul Unirii 5, Cluj-Napoca",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(collaborator, "Collab123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(collaborator, "Collaborator");
                }
            }

            // User normal
            if (await userManager.FindByEmailAsync("user@onlineshop.com") == null)
            {
                var user = new ApplicationUser
                {
                    UserName = "user@onlineshop.com",
                    Email = "user@onlineshop.com",
                    FirstName = "Maria",
                    LastName = "Ionescu",
                    Address = "Strada Libertății 20, Timișoara",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(user, "User123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "User");
                }
            }
        }

        private static async Task CreateCategoriesAndProductsAsync(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            // Verifică dacă există deja categorii
            if (context.Categories.Any()) return;

            // Creare categorii
            var categories = new List<Category>
            {
                new Category
                {
                    Name = "Electronice",
                    Description = "Telefoane, laptopuri, tablete și alte dispozitive electronice"
                },
                new Category
                {
                    Name = "Îmbrăcăminte",
                    Description = "Haine și accesorii pentru bărbați, femei și copii"
                },
                new Category
                {
                    Name = "Casă & Grădină",
                    Description = "Mobilier, decorațiuni și articole pentru grădină"
                }
            };

            context.Categories.AddRange(categories);
            await context.SaveChangesAsync();

            // Obține utilizatorii
            var admin = await userManager.FindByEmailAsync("admin@onlineshop.com");
            var collaborator = await userManager.FindByEmailAsync("collaborator@onlineshop.com");
            var user = await userManager.FindByEmailAsync("user@onlineshop.com");

            // Creare produse
            var products = new List<Product>
            {
                // Electronice
                new Product
                {
                    Title = "iPhone 15 Pro Max 256GB",
                    Description = "Cel mai avansat iPhone cu chip A17 Pro, cameră de 48MP și ecran Super Retina XDR de 6.7 inch. Include funcții AI avansate și baterie pentru o zi întreagă.",
                    ImagePath = "/images/products/iphone15.jpg",
                    Price = 6499.99m,
                    Stock = 15,
                    CategoryId = categories[0].Id,
                    Status = ProductStatus.Approved
                },
                new Product
                {
                    Title = "Laptop ASUS ROG Strix G16",
                    Description = "Laptop gaming cu procesor Intel Core i9, 32GB RAM, SSD 1TB și placă video RTX 4070. Ecran 16 inch 165Hz pentru gaming fluid.",
                    ImagePath = "/images/products/asus-rog.jpg",
                    Price = 8999.99m,
                    Stock = 8,
                    CategoryId = categories[0].Id,
                    Status = ProductStatus.Approved
                },
                // Îmbrăcăminte
                new Product
                {
                    Title = "Geacă de iarnă impermeabilă",
                    Description = "Geacă călduroasă cu puf natural, impermeabilă și rezistentă la vânt. Glugă detașabilă și multiple buzunare. Măsuri disponibile: S-XXL.",
                    ImagePath = "/images/products/geaca-iarna.jpg",
                    Price = 549.99m,
                    Stock = 25,
                    CategoryId = categories[1].Id,
                    CollaboratorId = collaborator?.Id,
                    Status = ProductStatus.Approved
                },
                new Product
                {
                    Title = "Sneakers Nike Air Max 270",
                    Description = "Adidași sport cu tehnologie Air Max pentru confort maxim. Design modern în culori neutre, potriviți pentru sport și casual.",
                    ImagePath = "/images/products/nike-airmax.jpg",
                    Price = 699.99m,
                    Stock = 30,
                    CategoryId = categories[1].Id,
                    Status = ProductStatus.Approved
                },
                // Casă & Grădină
                new Product
                {
                    Title = "Canapea extensibilă 3 locuri",
                    Description = "Canapea modernă din catifea cu funcție de extensie pentru pat. Include spațiu de depozitare și perne decorative. Culoare: gri antracit.",
                    ImagePath = "/images/products/canapea.jpg",
                    Price = 2499.99m,
                    Stock = 5,
                    CategoryId = categories[2].Id,
                    CollaboratorId = collaborator?.Id,
                    Status = ProductStatus.Pending // În așteptare
                }
            };

            context.Products.AddRange(products);
            await context.SaveChangesAsync();

            // Creare review-uri de test
            var reviews = new List<Review>
            {
                new Review
                {
                    Content = "Telefonul este excelent! Camerele sunt spectaculoase și bateria durează toată ziua.",
                    Rating = 5,
                    ProductId = products[0].Id,
                    UserId = user!.Id
                },
                new Review
                {
                    Content = "Foarte mulțumit de laptop. Merge perfect pentru gaming și productivitate.",
                    Rating = 4,
                    ProductId = products[1].Id,
                    UserId = user.Id
                },
                new Review
                {
                    Content = "Geaca este caldă și arată foarte bine. Recomand!",
                    Rating = 5,
                    ProductId = products[2].Id,
                    UserId = user.Id
                }
            };

            context.Reviews.AddRange(reviews);
            await context.SaveChangesAsync();

            // Actualizează rating-urile produselor
            foreach (var product in products.Where(p => p.Status == ProductStatus.Approved))
            {
                var productWithReviews = await context.Products
                    .Include(p => p.Reviews)
                    .FirstOrDefaultAsync(p => p.Id == product.Id);

                if (productWithReviews != null)
                {
                    productWithReviews.RecalculateRating();
                }
            }

            await context.SaveChangesAsync();

            // Creare FAQ-uri de test pentru AI Assistant
            var faqs = new List<ProductFAQ>
            {
                new ProductFAQ
                {
                    ProductId = products[0].Id,
                    Question = "Are garanție?",
                    Answer = "Da, produsul beneficiază de garanție 24 de luni conform legislației în vigoare.",
                    TimesAsked = 15
                },
                new ProductFAQ
                {
                    ProductId = products[0].Id,
                    Question = "Este rezistent la apă?",
                    Answer = "Da, iPhone 15 Pro Max are certificare IP68, fiind rezistent la apă și praf.",
                    TimesAsked = 8
                },
                new ProductFAQ
                {
                    ProductId = products[2].Id,
                    Question = "Se poate spăla la mașină?",
                    Answer = "Recomandăm spălarea la curățătorie sau spălare manuală pentru a păstra calitatea pufului.",
                    TimesAsked = 5
                }
            };

            context.ProductFAQs.AddRange(faqs);
            await context.SaveChangesAsync();
        }
    }
}