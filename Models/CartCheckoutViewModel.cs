using System.ComponentModel.DataAnnotations;

namespace FashionStoreAdmin.Models;

public class CartCheckoutViewModel
{
    [Required]
    [StringLength(200)]
    public string CustomerName { get; set; } = string.Empty;

    [Required]
    [StringLength(30)]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required]
    [StringLength(400)]
    public string Address { get; set; } = string.Empty;

    [Required]
    public CartPaymentMethod PaymentMethod { get; set; }

    // Display only (computed from session cart)
    public decimal TotalAmount { get; set; }
}

