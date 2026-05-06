using FashionStoreAdmin.Data;
using Microsoft.AspNetCore.Mvc;

namespace FashionStoreAdmin.Controllers;

public class HomeController : Controller
{
    private readonly ClientOrdersDbContext _context;

    public HomeController(ClientOrdersDbContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        var products = _context.Products.ToList();

        Console.WriteLine("Số lượng sản phẩm: " + products.Count);

        return View(products); // truyền xuống View luôn
    }
}