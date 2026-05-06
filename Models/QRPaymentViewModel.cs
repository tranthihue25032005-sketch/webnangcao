namespace FashionStoreAdmin.Models;

public class QRPaymentViewModel
{
    public int OrderId { get; set; }
    public decimal TotalAmount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;

    // base64 string to embed into <img>
    public string QrCodeImageBase64 { get; set; } = string.Empty;
}

