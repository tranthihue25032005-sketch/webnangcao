using FashionStoreAdmin.Models;
using Microsoft.EntityFrameworkCore;

namespace FashionStoreAdmin.Data;

public class ClientOrdersDbContext : DbContext
{
    public ClientOrdersDbContext(DbContextOptions<ClientOrdersDbContext> options)
        : base(options)
    {
    }
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<UserAccount> Users => Set<UserAccount>();
    public DbSet<ClientOrder> Orders => Set<ClientOrder>();
    public DbSet<ClientOrderDetail> OrderDetails => Set<ClientOrderDetail>();
    public DbSet<Promotion> Promotions { get; set; }
   
    //public DbSet<ClientUser> ClientUsers { get; set; }
    
public DbSet<Collection> Collections { get; set; }
public DbSet<ProductVariant> ProductVariants { get; set; }
public DbSet<ProductImage> ProductImages { get; set; }

    public DbSet<Brand> Brands => Set<Brand>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>().ToTable("Products");
        modelBuilder.Entity<Category>().ToTable("Categories");
        modelBuilder.Entity<UserAccount>().ToTable("Users");

        modelBuilder.Entity<ClientOrder>(entity =>
        {
            entity.ToTable("Orders"); // Requirement table name
            entity.HasKey(x => x.Id);

            entity.Property(x => x.CustomerName).HasMaxLength(200);
            entity.Property(x => x.PhoneNumber).HasMaxLength(30);
            entity.Property(e => e.UserEmail);
            entity.Property(e => e.ShippingAddress);

            entity.Property(x => x.TotalAmount).HasColumnType("decimal(18,2)");
            
            entity.Property(x => x.Status).HasConversion<int>(); // lưu enum 

            

            entity.HasMany(x => x.OrderDetails)
                .WithOne(x => x.Order)
                .HasForeignKey(x => x.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ClientOrderDetail>(entity =>
        {
            entity.ToTable("OrderDetails"); // Requirement table name
            entity.HasKey(x => x.Id);

            entity.Property(x => x.UnitPrice).HasColumnType("decimal(18,2)");
        });
    }
}

