using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
string conn = builder.Configuration.GetConnectionString("DefaultConnection")!;
conn = conn.Replace("replacement_name", Environment.MachineName);

builder.Services.AddDbContext<ApContext>(options => options.UseSqlServer(conn));

var app = builder.Build();

app.UseDefaultFiles(); //поддержка файлов index.html по-умолчанию
app.UseStaticFiles(); //поддержка статических файлов в папке wwwroot

app.MapGet("/api/order", getOrders); //обработка GET запроса

//app.MapGet("/api/order", (ApContext db) => db.Order.ToList()); //обработка GET запроса

//app.Run(async (context) => await context.Response.WriteAsync("Page not found")); //выполнится,если страница по-умолчанию не будет найдена

app.Run();

 List<Order> getOrders(HttpContext context, ApContext db)
{
    HttpResponse response = context.Response; //сокращаем запись ответа
    response.Headers.ContentType = "application/json; charset=utf-8";
    //await response.WriteAsync("Получен запрос GET! хм");
    return db.Order.ToList();
}
List<Product> getProducts(HttpContext context, ApContext db)
{
    HttpResponse response = context.Response; //сокращаем запись ответа
    response.Headers.ContentType = "application/json; charset=utf-8";
    return db.Product.ToList();
}


public class Order
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public string Number { get; set; } = string.Empty;
    public string Customer { get; set; } = string.Empty;
}
public class Product
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int Amount { get; set; }
    public decimal Price { get; set; }
}

public class ApContext : DbContext
{
    public DbSet<Order> Order { get; set; } = null!; //коллекция для таблицы заказы
    public DbSet<Product> Product { get; set; } = null!; //коллекция для таблицы заказы
    public ApContext(DbContextOptions<ApContext> options) : base(options)
    {
        Database.EnsureCreated();
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

    }
}