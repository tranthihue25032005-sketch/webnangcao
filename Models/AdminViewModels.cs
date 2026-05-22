namespace FashionStoreAdmin.Models;

public class DashboardMetric
{
    public string Label { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class TopSellingProduct
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int SoldQuantity { get; set; }
}

public class StockAlert
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string VariantName { get; set; } = string.Empty;
    public int RemainingStock { get; set; }
}

public class DashboardTileGentelella
{
    public string Title { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string ChangeText { get; set; } = string.Empty;
    public string IconClass { get; set; } = string.Empty;
}

public class RevenueDayPoint
{
    public string Label { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

public class NamedPercentItem
{
    public string Name { get; set; } = string.Empty;
    public int Percent { get; set; }
    public string? Extra { get; set; }
}

public class DashboardOverviewViewModel
{
    public List<DashboardMetric> Metrics { get; set; } = [];
    public List<DashboardTileGentelella> Tiles { get; set; } = [];
    public List<RevenueDayPoint> RevenueLastDays { get; set; } = [];
    public List<NamedPercentItem> SellingCampaigns { get; set; } = [];
    public List<NamedPercentItem> OrderStatusBars { get; set; } = [];
    public List<NamedPercentItem> CategoryDonut { get; set; } = [];
    public List<TopSellingProduct> TopSellingProducts { get; set; } = [];
    public List<StockAlert> LowStockAlerts { get; set; } = [];
    public List<ClientOrder> RecentOrders { get; set; } = [];
}

public class ProductVariant
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string Size { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
}

public class ProductImage
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string Caption { get; set; } = string.Empty;
}

public class Brand
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class Collection
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Season { get; set; } = string.Empty;
}

public class ProductManagementViewModel
{
    public IReadOnlyList<Product> Products { get; set; } = [];
    public IReadOnlyList<Category> Categories { get; set; } = [];
    public IReadOnlyList<Brand> Brands { get; set; } = [];
    public IReadOnlyList<Collection> Collections { get; set; } = [];
    
    public IReadOnlyList<ProductImage> Images { get; set; } = [];

    // PHÂN TRANG
    public int CurrentPage { get; set; }

    public int TotalPages { get; set; }

    public string Keyword { get; set; } = "";
}

public class CategoryManagementViewModel
{
    public IReadOnlyList<Category> Categories { get; set; } = [];
    public IReadOnlyList<Brand> Brands { get; set; } = [];
    public IReadOnlyList<Collection> Collections { get; set; } = [];
}

public enum UserRole
{
    Admin=0,
   
    Sale=1,

     Staff=2
}

public class UserAccount
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public bool IsLocked { get; set; }
    //public int TotalOrders { get; set; }
}

public class UserPurchaseHistory
{
    public int OrderId { get; set; }
    public DateTime OrderDate { get; set; }
    public decimal TotalAmount { get; set; }
    public string StatusText { get; set; } = string.Empty;
}

public class UserWithHistory
{
    public UserAccount User { get; set; } = new();
    public List<UserPurchaseHistory> PurchaseHistory { get; set; } = [];
}

public class UserManagementViewModel
{
    public IReadOnlyList<UserWithHistory> Users { get; set; } = [];
}

public enum EmployeeRole
{
    Admin=0,
    Sale=1,
    Staff=2,
}

public enum EmployeeStatus
{
    Active,
    Blocked
}

public class EmployeeAccount
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public EmployeeRole Role { get; set; }
    public EmployeeStatus Status { get; set; }
}

public class EmployeeManagementViewModel
{
    public IReadOnlyList<UserAccount> Employees { get; set; } = [];
}

public class LoginViewModel
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
}

public enum PromotionType
{
    Percent,
    FixedAmount,
    FreeShipping
}

public class Promotion
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public PromotionType Type { get; set; }
    public decimal Value { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int MaxUse { get; set; }
    public int UsedCount { get; set; }
}

public class PromotionManagementViewModel
{
    public IReadOnlyList<Promotion> Promotions { get; set; } = [];
}

