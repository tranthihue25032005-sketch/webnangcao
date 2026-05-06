namespace FashionStoreAdmin.Models;




public class ClientOrder
{
    public int Id { get; set; }
    public string CustomerName { get; set; } = "";
    public string PhoneNumber { get; set; } = "";

    public string UserEmail { get; set; } = ""; // ✅ đổi tên
    public string ShippingAddress { get; set; } = ""; // ✅ đổi tên

    public decimal TotalAmount { get; set; }
    public DateTime OrderDate { get; set; }

    public OrderStatus Status { get; set; } // ✅ dùng enum
    public string? PaymentMethod { get; set; } = "";
    public List<ClientOrderDetail> OrderDetails { get; set; } = [];
}


