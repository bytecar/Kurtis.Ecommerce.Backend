using Kurtis.Common.Models;
using Microsoft.EntityFrameworkCore;
namespace Kurtis.DAL
{
    public class KurtisDbContext : DbContext
    {
        public KurtisDbContext(DbContextOptions<KurtisDbContext> options) : base(options) { }
        public DbSet<Product> Products => Set<Product>();
        public DbSet<Brand> Brands => Set<Brand>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Collection> Collections => Set<Collection>();
        public DbSet<ProductCollection> ProductCollections => Set<ProductCollection>();
        public DbSet<Inventory> Inventories => Set<Inventory>();
        public DbSet<User> Users => Set<User>();
        protected override void OnModelCreating(ModelBuilder model)
        {
            base.OnModelCreating(model);
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
            model.Entity<Inventory>(b =>
            {
                b.HasKey(x => x.Id);
                b.HasIndex(x => new { x.ProductId, x.Size }).IsUnique();
            });
            model.Entity<User>(b =>
            {
                b.HasIndex(u => u.Email).IsUnique();
            });
            model.Entity<ProductCollection>(b =>
            {
                b.HasIndex(pc => new { pc.ProductId, pc.CollectionId });
            });
        }
    }
}