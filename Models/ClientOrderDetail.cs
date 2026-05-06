namespace FashionStoreAdmin.Models;

public class ClientOrderDetail
{
    public int Id { get; set; }

    // FK to ClientOrder
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string Size { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;


    public ClientOrder Order { get; set; } = null!;
}

