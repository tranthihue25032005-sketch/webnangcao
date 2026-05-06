using FashionStoreAdmin.Models;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using FashionStoreAdmin.Data;
namespace FashionStoreAdmin.Areas.FashionStoreAdmin.Controllers;

[Area("FashionStoreAdmin")]
public class AdminController : Controller
{
private readonly ClientOrdersDbContext _context;
private readonly IWebHostEnvironment _environment;
public AdminController(ClientOrdersDbContext context, IWebHostEnvironment environment)
{
            _context = context;
            _environment = environment;
}
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var employeeId = HttpContext.Session.GetInt32("EmployeeId");
        var role = HttpContext.Session.GetString("EmployeeRole");
        var action = context.RouteData.Values["action"]?.ToString() ?? string.Empty;

        if (employeeId is null)
        {
            context.Result = RedirectToAction("Login", "Account");
            return;
        }

        var staffAllowedActions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            nameof(Orders),
            nameof(OrderDetail),
            nameof(UpdateOrderStatus),
            nameof(ConfirmOrder),
            nameof(AddOrder)
        };

        if (string.Equals(role, EmployeeRole.Staff.ToString(), StringComparison.OrdinalIgnoreCase) &&
            !staffAllowedActions.Contains(action))
        {
            context.Result = RedirectToAction(nameof(Orders));
            return;
        }

        base.OnActionExecuting(context);
    }

    public IActionResult Index()
{
    var vm = new DashboardOverviewViewModel
    {
        Tiles = new List<DashboardTileGentelella>
        {
            new DashboardTileGentelella
            {
                Title = "Products",
                Value = _context.Products.Count().ToString(),
                ChangeText = "",
                IconClass = "fa fa-cube"
            },
            new DashboardTileGentelella
            {
                Title = "Orders",
                Value = _context.Orders.Count().ToString(),
                IconClass = "fa fa-shopping-cart"
            },
            new DashboardTileGentelella
            {
                Title = "Users",
                Value = _context.Users.Count().ToString(),
                IconClass = "fa fa-users"
            },
            new DashboardTileGentelella
            {
                Title = "Revenue",
                Value = _context.Orders.Sum(x => x.TotalAmount).ToString("N0"),
                IconClass = "fa fa-money"
            }
        },

        Metrics = new List<DashboardMetric>
        {
            new DashboardMetric { Label = "Today", Value = "100" },
            new DashboardMetric { Label = "This week", Value = "500" }
        },

        RevenueLastDays = _context.Orders
            .GroupBy(o => o.OrderDate.Date)
            .ToList()
            .Select(g => new RevenueDayPoint
            {
                Label = g.Key.ToString("dd/MM"),
                Amount = g.Sum(x => x.TotalAmount)
            })
            .OrderBy(x => x.Label)
            .Take(7)
            .ToList(),

        CategoryDonut = new List<NamedPercentItem>
        {
            new NamedPercentItem { Name = "Shirt", Percent = 40 },
            new NamedPercentItem { Name = "Pants", Percent = 30 },
            new NamedPercentItem { Name = "Shoes", Percent = 30 }
        },

        RecentOrders = _context.Orders
            .OrderByDescending(x => x.Id)
            .Take(5)
            .ToList()
    };

    return View(vm); // 🔥 QUAN TRỌNG
}

    public IActionResult Categories()
{
    var model = new CategoryManagementViewModel
    {
        Categories = _context.Categories.ToList(),
        Brands = _context.Brands.ToList(),
        Collections = _context.Collections.ToList()
    };

    ViewBag.ParentCategories = _context.Categories.ToList();

    return View(model);
}
    [HttpPost]
[ValidateAntiForgeryToken]
public IActionResult AddCategory(string name, string description, int? parentCategoryId)
{
    if (!string.IsNullOrWhiteSpace(name))
    {
        var category = new Category
        {
            Name = name.Trim(),
            Description = description?.Trim() ?? string.Empty,
            ParentCategoryId = parentCategoryId
        };

        _context.Categories.Add(category);
        _context.SaveChanges();
    }

    return RedirectToAction(nameof(Categories));
}

    [HttpPost]
[ValidateAntiForgeryToken]
public IActionResult UpdateCategory(Category category)
{
    var existing = _context.Categories.FirstOrDefault(x => x.Id == category.Id);

    if (existing != null && !string.IsNullOrWhiteSpace(category.Name))
    {
        existing.Name = category.Name.Trim();
        existing.Description = category.Description?.Trim() ?? string.Empty;

        if (category.ParentCategoryId == category.Id)
            existing.ParentCategoryId = null;
        else
            existing.ParentCategoryId = category.ParentCategoryId;

        _context.SaveChanges();
    }

    return RedirectToAction(nameof(Categories));
}
    [HttpPost]
[ValidateAntiForgeryToken]
public IActionResult DeleteCategory(int id)
{
    var category = _context.Categories.FirstOrDefault(x => x.Id == id);

    if (category != null)
    {
        _context.Categories.Remove(category);
        _context.SaveChanges();
    }

    return RedirectToAction(nameof(Categories));
}

    [HttpPost]
[ValidateAntiForgeryToken]
public IActionResult AddBrand(string name)
{
    if (!string.IsNullOrWhiteSpace(name))
    {
        var brand = new Brand
        {
            Name = name.Trim()
        };

        _context.Brands.Add(brand);
        _context.SaveChanges();
    }

    return RedirectToAction(nameof(Categories));
}

    [HttpPost]
[ValidateAntiForgeryToken]
public IActionResult AddCollection(string name, string season)
{
    if (!string.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(season))
    {
        var collection = new Collection
        {
            Name = name.Trim(),
            Season = season.Trim()
        };

        _context.Collections.Add(collection);
        _context.SaveChanges();
    }

    return RedirectToAction(nameof(Categories));
}

    public IActionResult Products()
    {
        var model = new ProductManagementViewModel
    {
        Products = _context.Products.ToList(),
        Categories = _context.Categories.ToList(),
        Brands = _context.Brands.ToList()
    };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult AddProduct(Product product, List<IFormFile>? imageFiles)
    {
        if (!string.IsNullOrWhiteSpace(product.Name) && product.Price >= 0 && product.Stock >= 0 && product.OriginalPrice >= 0)
        {
            product.Name = product.Name.Trim();
            product.Description = product.Description?.Trim() ?? string.Empty;
            product.Size = product.Size?.Trim() ?? string.Empty;
            product.Color = product.Color?.Trim() ?? string.Empty;
            _context.Products.Add(product);
            _context.SaveChanges();

            var productId = product.Id;
            SaveUploadedImages(productId, imageFiles, "Ảnh sản phẩm");
        }

        return RedirectToAction(nameof(Products));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult UpdateProduct(Product product, List<IFormFile>? imageFiles)
    {
        if (!string.IsNullOrWhiteSpace(product.Name))
        {
            var existing = _context.Products.FirstOrDefault(p => p.Id == product.Id);
        if (existing != null)
        {
            existing.Name = product.Name;
            existing.Price = product.Price;
            existing.Stock = product.Stock;
            existing.Description = product.Description;
            existing.CategoryId = product.CategoryId;
            existing.OriginalPrice = product.OriginalPrice;
            existing.Size = product.Size;
            existing.Color = product.Color;
            _context.SaveChanges();
        }
                    SaveUploadedImages(product.Id, imageFiles, "Ảnh cập nhật");
        }
        return RedirectToAction(nameof(Products));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteProduct(int id)
    {
        var product = _context.Products.Find(id);
        if (product != null)
        {
            _context.Products.Remove(product);
            _context.SaveChanges();
        }
        return RedirectToAction(nameof(Products));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult AddVariant(int productId, string size, string color, decimal price, int stock)
    {
        if (productId > 0 && !string.IsNullOrWhiteSpace(size) && !string.IsNullOrWhiteSpace(color) && price >= 0 && stock >= 0)
        {
           var variant = new ProductVariant
    {
        ProductId = productId,
        Size = size,
        Color = color,
        Price = price,
        Stock = stock
    };

    _context.ProductVariants.Add(variant);
    _context.SaveChanges();
        }
        return RedirectToAction(nameof(Products));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult AddImage(int productId, string imageUrl, string caption)
    {
        if (productId > 0 && !string.IsNullOrWhiteSpace(imageUrl))
{
    var img = new ProductImage
    {
        ProductId = productId,
        ImageUrl = imageUrl,
        Caption = caption
    };

    _context.ProductImages.Add(img);
    _context.SaveChanges();
    }
        return RedirectToAction(nameof(Products));
    }

    private void SaveUploadedImages(int productId, List<IFormFile>? imageFiles, string captionPrefix)
{
    if (productId <= 0 || imageFiles is null || imageFiles.Count == 0)
    {
        return;
    }

    var uploadRoot = Path.Combine(_environment.WebRootPath, "uploads", "products");
    Directory.CreateDirectory(uploadRoot);

    foreach (var file in imageFiles.Where(f => f.Length > 0))
    {
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp" };

        if (!allowed.Contains(extension))
        {
            continue;
        }

        var fileName = $"{productId}_{Guid.NewGuid():N}{extension}";
        var absolutePath = Path.Combine(uploadRoot, fileName);

        using (var stream = new FileStream(absolutePath, FileMode.Create))
        {
            file.CopyTo(stream);
        }

        var relativePath = $"/uploads/products/{fileName}";

        // 🔥 LƯU VÀO DATABASE
        var image = new ProductImage
        {
            ProductId = productId,
            ImageUrl = relativePath,
            Caption = $"{captionPrefix} {DateTime.Now:dd/MM/yyyy HH:mm}"
        };

        _context.ProductImages.Add(image);
    }

    _context.SaveChanges(); // lưu 1 lần cuối
}

    public IActionResult Orders()
    {
        var orders = _context.Orders
        .Include(x => x.OrderDetails)
        .ToList();

        return View(orders);
    }

    [HttpPost]
[ValidateAntiForgeryToken]
public IActionResult AddOrder(string customerName, decimal totalAmount, OrderStatus status)
{
    if (!string.IsNullOrWhiteSpace(customerName) && totalAmount >= 0)
    {
        var order = new ClientOrder
        {
            CustomerName = customerName.Trim(),
            TotalAmount = totalAmount,
            Status = status,
            OrderDate = DateTime.Now
        };

        _context.Orders.Add(order);
        _context.SaveChanges();
    }

    return RedirectToAction(nameof(Orders));
}

    public IActionResult OrderDetail(int id)
{
    var order = _context.Orders
    .Include(o => o.OrderDetails)
    .FirstOrDefault(o => o.Id == id);

    if (order == null)
    {
        return RedirectToAction(nameof(Orders));
    }

    return View(order);
}

    [HttpPost]
[ValidateAntiForgeryToken]
public IActionResult UpdateOrderStatus(int orderId, OrderStatus status)
{
    var order = _context.Orders.FirstOrDefault(o => o.Id == orderId);

    if (order != null)
    {
        order.Status = OrderStatus.Pending; 
        _context.SaveChanges();
    }

    return RedirectToAction(nameof(Orders));
}

    [HttpPost]
[ValidateAntiForgeryToken]
public IActionResult ConfirmOrder(int orderId)
{
    var order = _context.Orders.FirstOrDefault(o => o.Id == orderId);

    if (order != null)
    {
       order.Status = OrderStatus.Shipping; 
        _context.SaveChanges();
    }

    return RedirectToAction(nameof(Orders));
}

    public IActionResult Users()
{
    var users = _context.Users
        .Select(u => new UserWithHistory
        {
            User = u,
            PurchaseHistory = new List<UserPurchaseHistory>()
        }).ToList();

    return View(new UserManagementViewModel
    {
        Users = users
    });
}

    public IActionResult Employees()
{
    var employees = _context.Users
        .Where(u => u.Role == UserRole.Admin || u.Role == UserRole.Staff)
        .ToList();

    return View(new EmployeeManagementViewModel
    {
        Employees = employees
    });
}

   [HttpPost]
[ValidateAntiForgeryToken]
public IActionResult AddEmployee(string fullName, string email, string phoneNumber, string password, EmployeeRole role)
{
    if (!string.IsNullOrWhiteSpace(fullName) &&
        !string.IsNullOrWhiteSpace(email) &&
        !string.IsNullOrWhiteSpace(phoneNumber) &&
        !string.IsNullOrWhiteSpace(password))
    {
        var employee = new UserAccount
        {
            FullName = fullName.Trim(),
            Email = email.Trim(),
            PhoneNumber = phoneNumber.Trim(),
            Password = BCrypt.Net.BCrypt.HashPassword(password.Trim()),
            Role = (UserRole)role,
            IsLocked = false
        };

        _context.Users.Add(employee);
        _context.SaveChanges();
    }

    return RedirectToAction(nameof(Employees));
}

    [HttpPost]
[ValidateAntiForgeryToken]
public IActionResult UpdateEmployee(int id, string fullName, string phoneNumber, EmployeeRole role)
{
    var user = _context.Users.FirstOrDefault(x => x.Id == id);

    if (user != null)
    {
        user.FullName = fullName.Trim();
        user.PhoneNumber = phoneNumber.Trim();
        user.Role = role == EmployeeRole.Admin 
        ? UserRole.Admin 
        : UserRole.Staff;

        _context.SaveChanges();
    }

    return RedirectToAction(nameof(Employees));
}

    [HttpPost]
[ValidateAntiForgeryToken]
public IActionResult ResetEmployeePassword(int id, string newPassword)
{
    var user = _context.Users.FirstOrDefault(x => x.Id == id);

    if (user != null && !string.IsNullOrWhiteSpace(newPassword))
    {
        user.Password = BCrypt.Net.BCrypt.HashPassword(newPassword.Trim());
        _context.SaveChanges();
    }

    return RedirectToAction(nameof(Employees));
}

    [HttpPost]
[ValidateAntiForgeryToken]
public IActionResult DeleteEmployee(int id)
{
    var user = _context.Users.FirstOrDefault(x => x.Id == id);

    if (user != null)
    {
        user.IsLocked = true;
        _context.SaveChanges();
    }

    return RedirectToAction(nameof(Employees));
}

   [HttpPost]
[ValidateAntiForgeryToken]
public IActionResult ToggleEmployeeStatus(int id)
{
    var user = _context.Users.FirstOrDefault(x => x.Id == id);

    if (user != null)
    {
        user.IsLocked = !user.IsLocked;
        _context.SaveChanges();
    }

    return RedirectToAction(nameof(Employees));
}

    [HttpPost]
[ValidateAntiForgeryToken]
public IActionResult AddInternalUser(string fullName, string email, string phoneNumber, UserRole role)
{
    if (!string.IsNullOrWhiteSpace(fullName) &&
        !string.IsNullOrWhiteSpace(email) &&
        !string.IsNullOrWhiteSpace(phoneNumber))
    {
        var user = new UserAccount
        {
            FullName = fullName.Trim(),
            Email = email.Trim(),
            PhoneNumber = phoneNumber.Trim(),
            Role = role,
            IsLocked = false
        };

        _context.Users.Add(user);
        _context.SaveChanges();
    }

    return RedirectToAction(nameof(Users));
}

    [HttpPost]
[ValidateAntiForgeryToken]
public IActionResult ToggleUserLock(int userId)
{
    var user = _context.Users.FirstOrDefault(x => x.Id == userId);

    if (user != null)
    {
        user.IsLocked = !user.IsLocked;
        _context.SaveChanges();
    }

    return RedirectToAction(nameof(Users));
}

    public IActionResult Promotions()
{
    var promotions = _context.Promotions.ToList();

    return View(new PromotionManagementViewModel
    {
        Promotions = promotions
    });
}

    [HttpPost]
[ValidateAntiForgeryToken]
public IActionResult AddPromotion(string code, PromotionType type, decimal value, DateTime startDate, DateTime endDate, int maxUse)
{
    if (!string.IsNullOrWhiteSpace(code) && maxUse > 0 && startDate <= endDate)
    {
        var promotion = new Promotion
        {
            Code = code.Trim(),
            Type = type,
            Value = value,
            StartDate = startDate,
            EndDate = endDate,
            MaxUse = maxUse
        };

        _context.Promotions.Add(promotion);
        _context.SaveChanges();
    }

    return RedirectToAction(nameof(Promotions));
}
}

