namespace FashionStoreAdmin.Models;

public class DashboardViewModel
{
    public int TotalProducts { get; set; }
    public int TotalCategories { get; set; }
    public int TotalOrders { get; set; }
    public decimal RevenueThisMonth { get; set; }
    public List<ClientOrder> RecentOrders { get; set; } = new();
}
