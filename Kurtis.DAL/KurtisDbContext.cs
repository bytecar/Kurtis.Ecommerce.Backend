using Kurtis.Common.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Kurtis.DAL
{
    public class KurtisDbContext(DbContextOptions<KurtisDbContext> options) : IdentityDbContext<User, Role, int>(options)
    {
        public DbSet<Product> Products => Set<Product>();
        public DbSet<Brand> Brands => Set<Brand>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Collection> Collections => Set<Collection>();
        public DbSet<ProductCollection> ProductCollections => Set<ProductCollection>();
        public DbSet<Inventory> Inventories => Set<Inventory>();
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderItem> OrderItems => Set<OrderItem>();
        public DbSet<Wishlist> Wishlist => Set<Wishlist>();
        public DbSet<Review> Reviews => Set<Review>();
        public DbSet<RecentlyViewed> RecentlyViewed => Set<RecentlyViewed>();
        public DbSet<UserPreferences> UserPreferences => Set<UserPreferences>();
        public DbSet<Return> Returns => Set<Return>();
        public DbSet<Client> Clients => Set<Client>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

        protected override void OnModelCreating(ModelBuilder model)
        {
            base.OnModelCreating(model);
            
            // Configure Product entity
            model.Entity<Product>(b =>
            {
                b.HasKey(x => x.Id);
                b.Property(x => x.Name).IsRequired().HasMaxLength(250);
                b.Property(x => x.Price).HasColumnType("decimal(18,2)");
                b.Property(x => x.DiscountedPrice).HasColumnType("decimal(18,2)");
                b.Property(x => x.SizesJson).HasColumnType("nvarchar(max)");
                b.Property(x => x.ImageUrlsJson).HasColumnType("nvarchar(max)");
                b.HasIndex(x => new { x.BrandId, x.CategoryId });
                b.HasOne(p => p.Brand).WithMany().HasForeignKey(p => p.BrandId).OnDelete(DeleteBehavior.Restrict);
                b.HasOne(p => p.Category).WithMany().HasForeignKey(p => p.CategoryId).OnDelete(DeleteBehavior.Restrict);
            });
            
            // Configure Inventory entity
            model.Entity<Inventory>(b =>
            {
                b.HasKey(x => x.Id);
                b.HasIndex(x => new { x.ProductId, x.Size }).IsUnique();
            });
            
            // Configure User entity
            model.Entity<User>(b =>
            {
                b.HasIndex(u => u.Email).IsUnique();
                b.Property(u => u.RoleId).IsRequired().HasDefaultValue(4); // Default to Customer role
            });
            
            // Configure ProductCollection entity
            model.Entity<ProductCollection>(b =>
            {
                b.HasKey(x => x.Id);
                b.HasIndex(pc => new { pc.ProductId, pc.CollectionId });
            });
            
            // Configure Role entity
            model.Entity<Role>(b =>
            {
                b.HasIndex(pc => new { pc.Id });
            });
            
            // Configure UserRole entity
            model.Entity<UserRole>()
              .HasKey(ur => new { ur.UserId, ur.RoleId });
            model.Entity<UserRole>()
                .HasOne(ur => ur.User);
            model.Entity<UserRole>()
                .HasOne(ur => ur.Role);
            
            // Configure OrderItem entity
            model.Entity<OrderItem>(b =>
            {
                b.HasKey(x => x.Id);
                b.HasIndex(x => new { x.OrderId, x.ProductId });
                b.HasOne(x => x.Order).WithMany(o => o.Items).HasForeignKey(x => x.OrderId).OnDelete(DeleteBehavior.Cascade);
                b.HasOne(x => x.Product).WithMany(p => p.OrderItems).HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
                b.Property(x => x.Size).IsRequired().HasMaxLength(50);
                b.Property(x => x.Price).HasColumnType("decimal(18,2)");
            });
            
            // Configure RecentlyViewed entity
            model.Entity<RecentlyViewed>(b =>
            {
                b.HasKey(x => x.Id);
                b.HasIndex(x => new { x.UserId, x.ViewedAt });
                b.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
                b.HasOne(x => x.Product).WithMany(p => p.RecentlyViewedRecords).HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Cascade);
            });
            
            // Configure UserPreferences entity
            model.Entity<UserPreferences>(b =>
            {
                b.HasKey(x => x.Id);
                b.HasIndex(x => x.UserId).IsUnique();
                b.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
                b.Property(x => x.FavoriteCategoriesJson).HasColumnType("nvarchar(max)");
                b.Property(x => x.FavoriteColorsJson).HasColumnType("nvarchar(max)");
                b.Property(x => x.FavoriteOccasionsJson).HasColumnType("nvarchar(max)");
            });
            
            // Configure Return entity
            model.Entity<Return>(b =>
            {
                b.HasKey(x => x.Id);
                b.HasIndex(x => new { x.OrderId, x.Status });
                b.HasIndex(x => new { x.Status, x.CreatedAt });
                b.HasOne(x => x.Order).WithMany(o => o.Returns).HasForeignKey(x => x.OrderId).OnDelete(DeleteBehavior.Restrict);
                b.HasOne(x => x.OrderItem).WithMany().HasForeignKey(x => x.OrderItemId).OnDelete(DeleteBehavior.Restrict);
                b.Property(x => x.Reason).IsRequired().HasMaxLength(1000);
                b.Property(x => x.Status).IsRequired().HasMaxLength(50).HasDefaultValue("pending");
            });
            
            // Configure Wishlist entity
            model.Entity<Wishlist>(b =>
            {
                b.HasKey(x => x.Id);
                b.HasIndex(x => new { x.UserId, x.ProductId }).IsUnique();
                b.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
                b.HasOne(x => x.Product).WithMany(p => p.Wishlists).HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Cascade);
            });
            
            // Configure Review entity
            model.Entity<Review>(b =>
            {
                b.HasKey(x => x.Id);
                b.HasIndex(x => new { x.ProductId, x.CreatedAt });
                b.HasOne(x => x.Product).WithMany(p => p.Reviews).HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Cascade);
                b.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
            });
            
            // Seed initial Role data in the database
            model.Entity<Role>().HasData(
                new Role
                {
                    Id = 1,
                    Name = "User",
                    NormalizedName = "USER",
                    ConcurrencyStamp = "a1"
                },
                new Role
                {
                    Id = 2,
                    Name = "Admin",
                    NormalizedName = "ADMIN",
                    ConcurrencyStamp = "a2"
                },
                new Role
                {
                    Id = 3,
                    Name = "ContentCreator",
                    NormalizedName = "CONTENTCREATOR",
                    ConcurrencyStamp = "a3"
                },
                new Role
                {
                    Id = 4,
                    Name = "Customer",
                    NormalizedName = "CUSTOMER",
                    ConcurrencyStamp = "a4"
                }
            );
            
            // Seed initial Client data for authentication (demo purposes)
            model.Entity<Client>().HasData(
                new Client
                {
                    Id = 1,
                    ClientId = "client-app-one",
                    Name = "Demo Client Application One",
                    ClientSecret = "fPXxcJw8TW5sA+S4rl4tIPcKk+oXAqoRBo+1s2yjUS4=",
                    ClientURL = "https://clientappone.example.com",
                    IsActive = true,
                    ConcurrencyStamp = "c1",
                    CreatedAt = new DateTime(2025,11,11)
                },
                new Client
                {
                    Id = 2,
                    ClientId = "client-app-two",
                    Name = "Demo Client Application Two",
                    ClientSecret = "UkY2JEdtWqKFY5cEUuWqKZut2o6BI5cf3oexOlCMZvQ=",
                    ClientURL = "https://clientapptwo.example.com",
                    IsActive = true,
                    ConcurrencyStamp = "c1",
                    CreatedAt = new DateTime(2025, 11, 11)
                }
            );
        }
    }
}