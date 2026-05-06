using System.Text.Json;
using FashionStoreAdmin.Data;
using FashionStoreAdmin.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QRCoder;

namespace FashionStoreAdmin.Areas.FashionStoreClient.Controllers;

[Area("FashionStoreClient")]
public class CartController : Controller
{
    private const string CartSessionKey = "ClientCart";

    private readonly ClientOrdersDbContext _db;

    public CartController(ClientOrdersDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Checkout()
    {
        var email = HttpContext.Session.GetString("ClientUserEmail");
        if (string.IsNullOrWhiteSpace(email))
        {
            TempData["LoginRequiredMessage"] = "Vui lòng đăng nhập để thanh toán.";
            return RedirectToAction("Login", "Client");
        }

        var cart = GetCart();
        if (cart.Count == 0)
        {
            return RedirectToAction("Products", "Client");
        }

        var total = cart.Sum(x => x.UnitPrice * x.Quantity);

        return View(new CartCheckoutViewModel
        {
            TotalAmount = total,
            PaymentMethod = CartPaymentMethod.COD
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Checkout(CartCheckoutViewModel model)
    {
        var email = HttpContext.Session.GetString("ClientUserEmail");
        if (string.IsNullOrWhiteSpace(email))
        {
            TempData["LoginRequiredMessage"] = "Vui lòng đăng nhập để thanh toán.";
            return RedirectToAction("Login", "Client");
        }

        var cart = GetCart();
        if (cart.Count == 0)
        {
            return RedirectToAction("Products", "Client");
        }

        model.TotalAmount = cart.Sum(x => x.UnitPrice * x.Quantity);

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var paymentMethod = model.PaymentMethod == CartPaymentMethod.QR ? "QR" : "COD";
        var status = model.PaymentMethod == CartPaymentMethod.QR 
    ? OrderStatus.Pending 
    : OrderStatus.Pending;

        var order = new ClientOrder
{
        CustomerName = model.CustomerName.Trim(),
        PhoneNumber = model.PhoneNumber.Trim(),
        UserEmail = email!, // ✅
        ShippingAddress = model.Address.Trim(), // ✅
        TotalAmount = model.TotalAmount,
        OrderDate = DateTime.Now,
        Status = OrderStatus.Pending
    };

        _db.Orders.Add(order);
        await _db.SaveChangesAsync(); // ensure OrderId exists

        foreach (var item in cart)
        {
            _db.OrderDetails.Add(new ClientOrderDetail
            {
                OrderId = order.Id,
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice
            });
        }

        await _db.SaveChangesAsync();

        SaveCart([]);

        return model.PaymentMethod == CartPaymentMethod.QR
            ? RedirectToAction(nameof(QRPayment), new { orderId = order.Id })
            : RedirectToAction(nameof(Success), new { orderId = order.Id });
    }

    public async Task<IActionResult> Success(int orderId)
    {
        var order = await _db.Orders.AsNoTracking().FirstOrDefaultAsync(o => o.Id == orderId);
        if (order is null)
        {
            return NotFound();
        }

        return View(new CartSuccessViewModel
        {
            OrderId = order.Id,
            TotalAmount = order.TotalAmount,
            PaymentMethod = order.PaymentMethod,
           Status = order.Status.ToString()
        });
    }

    public async Task<IActionResult> QRPayment(int orderId)
    {
        var order = await _db.Orders.AsNoTracking().FirstOrDefaultAsync(o => o.Id == orderId);
        if (order is null)
        {
            return NotFound();
        }

        // Mock QR payload: encode order id + amount
        var qrPayload = $"ORDER_ID={order.Id}&AMOUNT={order.TotalAmount:0.##}";
        var qrBase64 = GenerateQrBase64(qrPayload);

        return View(new QRPaymentViewModel
        {
            OrderId = order.Id,
            TotalAmount = order.TotalAmount,
            PaymentMethod = order.PaymentMethod,
            Status = order.Status.ToString(),
            QrCodeImageBase64 = qrBase64
        });
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

    private static string GenerateQrBase64(string payload)
    {
        using var qrGenerator = new QRCodeGenerator();
        using var qrData = qrGenerator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.Q);
        using var pngQr = new PngByteQRCode(qrData);
        var bytes = pngQr.GetGraphic(20);
        return $"data:image/png;base64,{Convert.ToBase64String(bytes)}";
    }
}

