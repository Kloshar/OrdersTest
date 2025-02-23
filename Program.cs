using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
string conn = builder.Configuration.GetConnectionString("DefaultConnection")!;
conn = conn.Replace("replacement_name", Environment.MachineName);

builder.Services.AddDbContext<ApContext>(options => options.UseSqlServer(conn));

var app = builder.Build();

app.UseDefaultFiles(); //��������� ������ index.html ��-���������
app.UseStaticFiles(); //��������� ����������� ������ � ����� wwwroot

app.MapGet("/api/order", getOrders); //��������� GET �������

//app.MapGet("/api/order", (ApContext db) => db.Order.ToList()); //��������� GET �������

//app.Run(async (context) => await context.Response.WriteAsync("Page not found")); //����������,���� �������� ��-��������� �� ����� �������

app.Run();

 List<Order> getOrders(HttpContext context, ApContext db)
{
    HttpResponse response = context.Response; //��������� ������ ������
    response.Headers.ContentType = "application/json; charset=utf-8";
    //await response.WriteAsync("������� ������ GET! ��");
    return db.Order.ToList();
}
List<Product> getProducts(HttpContext context, ApContext db)
{
    HttpResponse response = context.Response; //��������� ������ ������
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
    public DbSet<Order> Order { get; set; } = null!; //��������� ��� ������� ������
    public DbSet<Product> Product { get; set; } = null!; //��������� ��� ������� ������
    public ApContext(DbContextOptions<ApContext> options) : base(options)
    {
        Database.EnsureCreated();
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

    }
}