using FashionStoreAdmin.Models;
using FashionStoreAdmin.Data;
using Microsoft.AspNetCore.Mvc;

namespace FashionStoreAdmin.Areas.FashionStoreClient.Controllers;

[ApiController]
[Area("FashionStoreClient")]
[Route("api")]
public class ApiController : ControllerBase
{
    private readonly ClientOrdersDbContext _context;

        public ApiController(ClientOrdersDbContext context)
        {
            _context = context;
        }
    [HttpGet("products")]
    public IActionResult GetProducts([FromQuery] string? search, [FromQuery] int? categoryId, [FromQuery] decimal? minPrice, [FromQuery] decimal? maxPrice)
    {
        var query = _context.Products.Where(p => p.IsActive);

if (!string.IsNullOrWhiteSpace(search))
    query = query.Where(p => p.Name.Contains(search));

if (categoryId.HasValue)
    query = query.Where(p => p.CategoryId == categoryId);

if (minPrice.HasValue)
    query = query.Where(p => p.Price >= minPrice);

if (maxPrice.HasValue)
    query = query.Where(p => p.Price <= maxPrice);

var items = query.ToList();

return Ok(items);
    }

    [HttpGet("products/{id:int}")]
    public IActionResult GetProduct(int id)
    {
        var product = _context.Products.FirstOrDefault(p => p.Id == id);

if (product == null)
{
    return NotFound(new { message = "Không tìm thấy sản phẩm." });
}

var variants = _context.ProductVariants
    .Where(v => v.ProductId == id).ToList();

var images = _context.ProductImages
    .Where(i => i.ProductId == id).ToList();

return Ok(new
{
    product,
    variants,
    images
});
    }

    [HttpPost("register")]
    public IActionResult Register([FromBody] ApiRegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FullName) ||
            string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { message = "Thiếu thông tin bắt buộc." });
        }

        var exists = _context.Users.Any(x => x.Email == request.Email);

if (exists)
{
    return BadRequest(new { message = "Email đã tồn tại." });
}

var user = new UserAccount
{
    FullName = request.FullName,
    Email = request.Email,
    PhoneNumber = request.PhoneNumber,
    Password = BCrypt.Net.BCrypt.HashPassword(request.Password),
    Role = UserRole.Staff,
    IsLocked = false
};

_context.Users.Add(user);
_context.SaveChanges();

return Ok(new { message = "Đăng ký thành công." });
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] ApiLoginRequest request)
    {
        var user = _context.Users
    .FirstOrDefault(x => x.Email == request.Email && !x.IsLocked);

if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
{
    return Unauthorized(new { message = "Sai tài khoản hoặc mật khẩu." });
}

return Ok(new
{
    message = "Đăng nhập thành công",
    user = new
    {
        user.Id,
        user.FullName,
        user.Email
    }
});
    }

    [HttpPost("orders")]
    public IActionResult CreateOrder([FromBody] ApiCreateOrderRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.UserEmail))
        {
            return Unauthorized(new { message = "Cần đăng nhập để đặt hàng." });
        }

        if (string.IsNullOrWhiteSpace(request.CustomerName) ||
            string.IsNullOrWhiteSpace(request.PhoneNumber) ||
            string.IsNullOrWhiteSpace(request.ShippingAddress) ||
            request.Items.Count == 0)
        {
            return BadRequest(new { message = "Thông tin đơn hàng không hợp lệ." });
        }

        var order = new ClientOrder
{
    CustomerName = request.CustomerName,
    PhoneNumber = request.PhoneNumber,
    UserEmail = request.UserEmail, // ✅
    ShippingAddress = request.ShippingAddress, // ✅
    TotalAmount = request.Items.Sum(x => x.Quantity * x.UnitPrice),
    OrderDate = DateTime.Now,
    Status = OrderStatus.Pending
};

_context.Orders.Add(order);
_context.SaveChanges();

foreach (var item in request.Items)
{
    _context.OrderDetails.Add(new ClientOrderDetail
    {
        OrderId = order.Id,
        ProductId = item.ProductId,
        Quantity = item.Quantity,
        UnitPrice = item.UnitPrice
    });
}

_context.SaveChanges();

return Ok(new { message = "Đặt hàng thành công", orderId = order.Id });
    }
}

