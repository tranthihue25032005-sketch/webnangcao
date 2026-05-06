namespace FashionStoreAdmin.Models;

public class ClientHomeViewModel
{
    public IReadOnlyList<Product> FeaturedProducts { get; set; } = [];
    public IReadOnlyList<Category> Categories { get; set; } = [];
}

public class ClientProductsViewModel
{
    public IReadOnlyList<Product> Products { get; set; } = [];
    public IReadOnlyList<Category> Categories { get; set; } = [];
    public List<ClientOrder> Orders { get; set; }= new List<ClientOrder>();
    public string Search { get; set; } = string.Empty;
    public int? CategoryId { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
}

public class ClientProductDetailViewModel
{
    public Product Product { get; set; } = new();
    public IReadOnlyList<ProductVariant> Variants { get; set; } = [];
    public IReadOnlyList<ProductImage> Images { get; set; } = [];
    public Category? Category { get; set; }
}

public class CartItem
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string Size { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

public class CartViewModel
{
    public List<CartItem> Items { get; set; } = [];
    public decimal TotalAmount => Items.Sum(x => x.UnitPrice * x.Quantity);
}

public class CheckoutViewModel
{
    public string CustomerName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public CartViewModel Cart { get; set; } = new();
}

public class ClientRegisterViewModel
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
}

public class ClientLoginViewModel
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
}

public class ClientOrdersViewModel
{
    public List<ClientOrder> Orders { get; set; } = [];
}

public class ApiRegisterRequest
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class ApiLoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class ApiCreateOrderRequest
{
    public string CustomerName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string ShippingAddress { get; set; } = string.Empty;
    public List<OrderItem> Items { get; set; } = [];
}
