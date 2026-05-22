using System.Text.Json;
using FashionStoreAdmin.Models;
using FashionStoreAdmin.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace FashionStoreAdmin.Areas.FashionStoreClient.Controllers;

[Area("FashionStoreClient")]
public class ClientController : Controller
{
    private const string CartSessionKey = "ClientCart";
   private readonly ClientOrdersDbContext _context;

    public ClientController(ClientOrdersDbContext context)
{
    _context = context;
}

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        base.OnActionExecuting(context);
        var cart = GetCart();
        ViewBag.CartCount = cart.Sum(x => x.Quantity);
        ViewBag.ClientUserName = HttpContext.Session.GetString("ClientUserName");
        ViewBag.ClientUserEmail = HttpContext.Session.GetString("ClientUserEmail");
    }

    public IActionResult Index()
    {
        var vm = new ClientHomeViewModel
        {
           Categories = _context.Categories.ToList(),
FeaturedProducts = _context.Products
    .Include(p => p.ProductImages)
    .Where(p => p.IsActive)
    .Take(8)
    .ToList()
        };
        return View(vm);
    }

    [HttpGet]
public IActionResult Search(string keyword)
{
    if (string.IsNullOrEmpty(keyword))
        return Json(new List<object>());

    var results = _context.Products
        .Where(p => p.Name.Contains(keyword))
        .Select(p => new {
            p.Id,
            p.Name,
            p.Price,
            Image = p.ProductImages.FirstOrDefault() != null 
    ? p.ProductImages.FirstOrDefault().ImageUrl 
    : "/images/no-image.png"
        })
        
        .ToList();

    return Json(results);
}

    public IActionResult Products(string? search, int? categoryId, decimal? minPrice, decimal? maxPrice)
{
    var products = _context.Products.Include(p => p.ProductImages).AsQueryable();

    if (!string.IsNullOrEmpty(search))
        products = products.Where(p => p.Name.Contains(search));

    if (categoryId.HasValue)
        products = products.Where(p => p.CategoryId == categoryId);

    if (minPrice.HasValue)
        products = products.Where(p => p.Price >= minPrice);

    if (maxPrice.HasValue)
        products = products.Where(p => p.Price <= maxPrice);

    var vm = new ClientProductsViewModel
    {
        Search = search ?? string.Empty,
        CategoryId = categoryId,
        MinPrice = minPrice,
        MaxPrice = maxPrice,
        Categories = _context.Categories.ToList(),
        Products = products.ToList()
    };

    return View(vm);
}

    public IActionResult Product(int id)
    {
        var product = _context.Products.FirstOrDefault(p => p.Id == id);
        if (product is null)
        {
            return RedirectToAction(nameof(Products));
        }

        var vm = new ClientProductDetailViewModel
        {
            Product = product,
            Variants = _context.ProductVariants.Where(v => v.ProductId == id).ToList(),
            Images = _context.ProductImages.Where(i => i.ProductId == id).ToList(),
            Category = _context.Categories.FirstOrDefault(c => c.Id == product.CategoryId)
        };
        return View(vm);
    }

    public IActionResult Cart()
    {
        var vm = new CartViewModel { Items = GetCart() };
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult AddToCart(int productId, int quantity = 1, string? size = null, string? color = null)
    {
        var email = HttpContext.Session.GetString("ClientUserEmail");
        if (string.IsNullOrWhiteSpace(email))
        {
            TempData["LoginRequiredMessage"] = "Vui lòng đăng nhập để mua hàng.";
            return RedirectToAction(nameof(Login));
        }

        var product = _context.Products.FirstOrDefault(p => p.Id == productId);
        if (product is null)
        {
            return RedirectToAction(nameof(Products));
        }

        var variant = _context.ProductVariants
        .FirstOrDefault(v =>
            v.ProductId == productId &&
            (string.IsNullOrWhiteSpace(size) || v.Size == size) &&
            (string.IsNullOrWhiteSpace(color) || v.Color == color));

        var image = _context.ProductImages
        .Where(i => i.ProductId == productId)
        .Select(i => i.ImageUrl)
        .FirstOrDefault() ?? "https://picsum.photos/seed/no-image/300/200";
        var cart = GetCart();
        var matched = cart.FirstOrDefault(x => x.ProductId == productId &&
                                               x.Size == (variant?.Size ?? size ?? product.Size) &&
                                               x.Color == (variant?.Color ?? color ?? product.Color));
        if (matched is null)
        {
            cart.Add(new CartItem
            {
                ProductId = productId,
                ProductName = product.Name,
                ImageUrl = image,
                Size = variant?.Size ?? size ?? product.Size,
                Color = variant?.Color ?? color ?? product.Color,
                Quantity = Math.Max(1, quantity),
                UnitPrice = variant?.Price ?? product.Price
            });
        }
        else
        {
            matched.Quantity += Math.Max(1, quantity);
        }

        SaveCart(cart);
        return RedirectToAction(nameof(Cart));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult UpdateCart(int productId, string size, string color, int quantity)
    {
        var cart = GetCart();
        var item = cart.FirstOrDefault(x => x.ProductId == productId && x.Size == size && x.Color == color);
        if (item is not null)
        {
            if (quantity <= 0)
            {
                cart.Remove(item);
            }
            else
            {
                item.Quantity = quantity;
            }
            SaveCart(cart);
        }
        return RedirectToAction(nameof(Cart));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult RemoveFromCart(int productId, string size, string color)
    {
        var cart = GetCart();
        cart.RemoveAll(x => x.ProductId == productId && x.Size == size && x.Color == color);
        SaveCart(cart);
        return RedirectToAction(nameof(Cart));
    }

    public IActionResult Checkout()
    {
        var email = HttpContext.Session.GetString("ClientUserEmail");
        if (string.IsNullOrWhiteSpace(email))
        {
            TempData["LoginRequiredMessage"] = "Vui lòng đăng nhập để thanh toán.";
            return RedirectToAction(nameof(Login));
        }

        var cart = GetCart();
        if (cart.Count == 0)
        {
            return RedirectToAction(nameof(Cart));
        }

        var vm = new CheckoutViewModel
        {
            CustomerName = HttpContext.Session.GetString("ClientUserName") ?? string.Empty,
            Cart = new CartViewModel { Items = cart }
        };
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Checkout(CheckoutViewModel model)
    {
        var email = HttpContext.Session.GetString("ClientUserEmail");
        if (string.IsNullOrWhiteSpace(email))
        {
            TempData["LoginRequiredMessage"] = "Vui lòng đăng nhập để thanh toán.";
            return RedirectToAction(nameof(Login));
        }

        var cart = GetCart();
        if (cart.Count == 0)
        {
            return RedirectToAction(nameof(Cart));
        }

        if (string.IsNullOrWhiteSpace(model.CustomerName) ||
            string.IsNullOrWhiteSpace(model.PhoneNumber) ||
            string.IsNullOrWhiteSpace(model.Address))
        {
            model.Cart = new CartViewModel { Items = cart };
            return View(model);
        }

        var items = cart.Select(x => new OrderItem
        {
            ProductId = x.ProductId,
            ProductName = x.ProductName,
            Size = x.Size,
            Color = x.Color,
            Quantity = x.Quantity,
            UnitPrice = x.UnitPrice
        }).ToList();

        // Tạo đơn hàng
var order = new ClientOrder
{
    CustomerName = model.CustomerName,
    UserEmail = email!, // ✅
    PhoneNumber = model.PhoneNumber,
    ShippingAddress = model.Address, // ✅
    TotalAmount = items.Sum(x => x.Quantity * x.UnitPrice),
    OrderDate = DateTime.Now,
    Status = OrderStatus.Pending
};

_context.Orders.Add(order);
_context.SaveChanges();

// Thêm chi tiết đơn hàng
foreach (var item in items)
{
    var orderDetail = new ClientOrderDetail
    {
        OrderId = order.Id,
        ProductId = item.ProductId,
        ProductName = item.ProductName,
        Size = item.Size,
        Color = item.Color,
        Quantity = item.Quantity,
        UnitPrice = item.UnitPrice
    };

    _context.OrderDetails.Add(orderDetail);
}

_context.SaveChanges();

        SaveCart([]);
        TempData["SuccessMessage"] = "Đặt hàng thành công.";

        return RedirectToAction(nameof(Orders));
    }

    public IActionResult Register()
    {
        return View(new ClientRegisterViewModel());
    }

    [HttpPost]
[ValidateAntiForgeryToken]
public IActionResult Register(ClientRegisterViewModel model)
{
    if (string.IsNullOrWhiteSpace(model.FullName) ||
        string.IsNullOrWhiteSpace(model.Email) ||
        string.IsNullOrWhiteSpace(model.Password))
    {
        model.ErrorMessage = "Vui lòng nhập đầy đủ thông tin.";
        return View(model);
    }

    // check email đã tồn tại chưa
    var existed = _context.Users
        .FirstOrDefault(x => x.Email == model.Email);

    if (existed != null)
    {
        model.ErrorMessage = "Email đã tồn tại.";
        return View(model);
    }

    // tạo user mới
    var user = new UserAccount
    {
        FullName = model.FullName,
        Email = model.Email,
        PhoneNumber = model.PhoneNumber,
        Password = model.Password // (sau này nên hash)
    };

    _context.Users.Add(user);
    _context.SaveChanges();

    // lưu session
    HttpContext.Session.SetString("ClientUserName", user.FullName);
    HttpContext.Session.SetString("ClientUserEmail", user.Email);

    return RedirectToAction(nameof(Index));
}

    public IActionResult Login()
    {
        return View(new ClientLoginViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Login(ClientLoginViewModel model)
    {
        var user = _context.Users
    .FirstOrDefault(u => u.Email == model.Email && u.Password == model.Password);
        if (user is null)
        {
            model.ErrorMessage = "Email hoặc mật khẩu không đúng.";
            return View(model);
        }

        HttpContext.Session.SetString("ClientUserName", user.FullName);
        HttpContext.Session.SetString("ClientUserEmail", user.Email);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Logout()
    {
        HttpContext.Session.Remove("ClientUserName");
        HttpContext.Session.Remove("ClientUserEmail");
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Orders()
    {
        var email = HttpContext.Session.GetString("ClientUserEmail");
        if (string.IsNullOrWhiteSpace(email))
        {
            TempData["LoginRequiredMessage"] = "Vui lòng đăng nhập để xem đơn hàng.";
            return RedirectToAction(nameof(Login));
        }

        var vm = new ClientOrdersViewModel
        {
            Orders =  _context.Orders.Include(o => o.OrderDetails).Where(o => o.UserEmail == email)
            .OrderByDescending(o => o.Id).ToList()
        };
        return View(vm);
    }

    private List<CartItem> GetCart()
    {
        var json = HttpContext.Session.GetString(CartSessionKey);
        if (string.IsNullOrWhiteSpace(json))
        {
            return [];
        }
        return JsonSerializer.Deserialize<List<CartItem>>(json) ?? [];
    }

    private void SaveCart(List<CartItem> cart)
    {
        HttpContext.Session.SetString(CartSessionKey, JsonSerializer.Serialize(cart));
    }

    // Guest checkout removed: only logged-in users can purchase.
}

