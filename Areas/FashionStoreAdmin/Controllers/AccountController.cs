using FashionStoreAdmin.Models;
using Microsoft.AspNetCore.Mvc;
using FashionStoreAdmin.Data;

namespace FashionStoreAdmin.Areas.FashionStoreAdmin.Controllers;

[Area("FashionStoreAdmin")]
public class AccountController : Controller
{
    private readonly ClientOrdersDbContext _context;

    public AccountController(ClientOrdersDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public IActionResult Login()
    {
        if (HttpContext.Session.GetInt32("EmployeeId") != null)
        {
            return RedirectToAction("Index", "Admin");
        }

        return View(new LoginViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Login(LoginViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Email) || string.IsNullOrWhiteSpace(model.Password))
        {
            model.ErrorMessage = "Vui lòng nhập email và mật khẩu.";
            return View(model);
        }

        var employee = _context.Users
            .FirstOrDefault(x => x.Email == model.Email && x.IsLocked == false);

        if (employee == null)
        {
            model.ErrorMessage = "Sai tài khoản hoặc mật khẩu";
            return View(model);
        }

        var isValidPassword =
            employee.Password == model.Password || // tạm cho DB cũ
            BCrypt.Net.BCrypt.Verify(model.Password, employee.Password);

        if (!isValidPassword)
        {
            model.ErrorMessage = "Sai mật khẩu.";
            return View(model);
        }

        HttpContext.Session.SetInt32("EmployeeId", employee.Id);
        HttpContext.Session.SetString("EmployeeName", employee.FullName);
        HttpContext.Session.SetString("EmployeeRole", employee.Role.ToString());

        return RedirectToAction("Index", "Admin");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction(nameof(Login));
    }
}