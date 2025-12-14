using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace OnlineShop.Models
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // ===== DbSets =====
        public DbSet<Category> Categories { get; set; } = null!;
        public DbSet<Product> Products { get; set; } = null!;
        public DbSet<Review> Reviews { get; set; } = null!;
        public DbSet<Cart> Carts { get; set; } = null!;
        public DbSet<CartItem> CartItems { get; set; } = null!;
        public DbSet<Wishlist> Wishlists { get; set; } = null!;
        public DbSet<WishlistItem> WishlistItems { get; set; } = null!;
        public DbSet<Order> Orders { get; set; } = null!;
        public DbSet<OrderItem> OrderItems { get; set; } = null!;
        public DbSet<ProductFAQ> ProductFAQs { get; set; } = null!;
        public DbSet<CollaboratorRequest> CollaboratorRequests { get; set; } = null!;


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ===== CATEGORY =====
            modelBuilder.Entity<Category>(entity =>
            {
                // Numele categoriei trebuie să fie unic
                entity.HasIndex(c => c.Name).IsUnique();

                // La ștergerea categoriei se șterg toate produsele (Cascade)
                entity.HasMany(c => c.Products)
                      .WithOne(p => p.Category)
                      .HasForeignKey(p => p.CategoryId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ===== PRODUCT =====
            modelBuilder.Entity<Product>(entity =>
            {
                // Index pentru căutare rapidă după titlu
                entity.HasIndex(p => p.Title);

                // Index pentru filtrare după status
                entity.HasIndex(p => p.Status);

                // Relație cu colaboratorul
                entity.HasOne(p => p.Collaborator)
                      .WithMany(u => u.ProposedProducts)
                      .HasForeignKey(p => p.CollaboratorId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // ===== REVIEW =====
            modelBuilder.Entity<Review>(entity =>
            {
                // Un utilizator poate avea un singur review per produs
                entity.HasIndex(r => new { r.ProductId, r.UserId }).IsUnique();

                // Relație cu produsul
                entity.HasOne(r => r.Product)
                      .WithMany(p => p.Reviews)
                      .HasForeignKey(r => r.ProductId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Relație cu utilizatorul
                entity.HasOne(r => r.User)
                      .WithMany(u => u.Reviews)
                      .HasForeignKey(r => r.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ===== CART =====
            modelBuilder.Entity<Cart>(entity =>
            {
                // Un utilizator are un singur coș
                entity.HasIndex(c => c.UserId).IsUnique();

                entity.HasOne(c => c.User)
                      .WithOne(u => u.Cart)
                      .HasForeignKey<Cart>(c => c.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ===== CART ITEM =====
            modelBuilder.Entity<CartItem>(entity =>
            {
                // Un produs apare o singură dată în același coș
                entity.HasIndex(ci => new { ci.CartId, ci.ProductId }).IsUnique();

                entity.HasOne(ci => ci.Cart)
                      .WithMany(c => c.CartItems)
                      .HasForeignKey(ci => ci.CartId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(ci => ci.Product)
                      .WithMany(p => p.CartItems)
                      .HasForeignKey(ci => ci.ProductId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ===== WISHLIST =====
            modelBuilder.Entity<Wishlist>(entity =>
            {
                // Un utilizator are un singur wishlist
                entity.HasIndex(w => w.UserId).IsUnique();

                entity.HasOne(w => w.User)
                      .WithOne(u => u.Wishlist)
                      .HasForeignKey<Wishlist>(w => w.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ===== WISHLIST ITEM =====
            modelBuilder.Entity<WishlistItem>(entity =>
            {
                // Un produs apare o singură dată în același wishlist (fără duplicare)
                entity.HasIndex(wi => new { wi.WishlistId, wi.ProductId }).IsUnique();

                entity.HasOne(wi => wi.Wishlist)
                      .WithMany(w => w.WishlistItems)
                      .HasForeignKey(wi => wi.WishlistId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(wi => wi.Product)
                      .WithMany(p => p.WishlistItems)
                      .HasForeignKey(wi => wi.ProductId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ===== ORDER =====
            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasOne(o => o.User)
                      .WithMany(u => u.Orders)
                      .HasForeignKey(o => o.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ===== ORDER ITEM =====
            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.HasOne(oi => oi.Order)
                      .WithMany(o => o.OrderItems)
                      .HasForeignKey(oi => oi.OrderId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Restrict pentru a păstra istoricul comenzilor când se șterge produsul
                entity.HasOne(oi => oi.Product)
                      .WithMany(p => p.OrderItems)
                      .HasForeignKey(oi => oi.ProductId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ===== PRODUCT FAQ =====
            modelBuilder.Entity<ProductFAQ>(entity =>
            {
                entity.HasOne(f => f.Product)
                      .WithMany(p => p.FAQs)
                      .HasForeignKey(f => f.ProductId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}