using FashionStoreAdmin.Data;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Net;
using System.Net.Sockets;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
//builder.Services.AddSingleton<FashionStoreAdmin.Services.FakeDataService>();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(8);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// SQLite database for client checkout orders
var dbPath = Path.Combine(AppContext.BaseDirectory, "App_Data", "fashionstore_client.db");
Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ClientOrdersDbContext>(options =>
{
    options.UseSqlServer(connectionString);
});
// Prevent "address already in use" by picking the first free port.
// If you already have the app running, a new instance will automatically move to another port.
var preferredPort = 5019;
var lastPort = 5100;
var selectedPort = preferredPort;
for (; selectedPort <= lastPort; selectedPort++)
{
    try
    {
        // Use IPv4 loopback explicitly so we match Kestrel binding to 127.0.0.1
        using var probe = new TcpListener(IPAddress.Parse("127.0.0.1"), selectedPort);
        probe.Start();
        break;
    }
    catch
    {
        // Port is in use
    }
}

var selectedUrl = $"http://127.0.0.1:{selectedPort}";
builder.WebHost.UseUrls(selectedUrl);
Console.WriteLine($"[Startup] Using URL: {selectedUrl}");

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseSession();

app.UseAuthorization();

app.MapStaticAssets();

// Create database tables on startup (no migrations for demo)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ClientOrdersDbContext>();
    
}

app.MapControllerRoute(
    name: "admin",
    pattern: "Admin/{action=Index}/{id?}",
    defaults: new { area = "FashionStoreAdmin", controller = "Admin" }
)
    .WithStaticAssets();

app.MapControllerRoute(
    name: "account",
    pattern: "Account/{action=Login}/{id?}",
    defaults: new { area = "FashionStoreAdmin", controller = "Account" }
)
    .WithStaticAssets();

app.MapControllerRoute(
    name: "client",
    pattern: "Client/{action=Index}/{id?}",
    defaults: new { area = "FashionStoreClient", controller = "Client" }
)
    .WithStaticAssets();

app.MapControllerRoute(
    name: "cart",
    pattern: "Cart/{action=Checkout}/{id?}",
    defaults: new { area = "FashionStoreClient", controller = "Cart" }
)
    .WithStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
