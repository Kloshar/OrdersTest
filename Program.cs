using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
string conn = builder.Configuration.GetConnectionString("DefaultConnection")!;
conn = conn.Replace("replacement_name", Environment.MachineName);
builder.Services.AddDbContext<ApContext>(options => options.UseSqlServer(conn));
var app = builder.Build();
app.UseDefaultFiles(); //��������� ������ index.html ��-���������
app.UseStaticFiles(); //��������� ����������� ������ � ����� wwwroot

app.MapGet("/api/order", getOrders); //��������� GET �������
app.MapGet("/api/productnames", getProductNames); //��������� GET �������
app.MapDelete("/api/deleteOrder/{id:int}", deleteOrder); //��������� GET �������

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
IResult getProductNames(HttpContext context, ApContext db)
{
    List<Product> products = db.Product.ToList();
    List<ProductName> names = new List<ProductName>();

    foreach(Product p in products)
    {
        ProductName name = new ProductName();
        name.Title = p.Title;
        name.Price = p.Price;
        names.Add(name);
    }
    var articleNames = names.Distinct(new ProductComparer()).ToList();

    foreach(ProductName n in articleNames)
    {
        Console.WriteLine($"{n.Title}, {n.Price}");
    }
    Console.WriteLine(articleNames);

    return Results.Json(articleNames);
}
async Task<IResult> deleteOrder(HttpRequest request, ApContext db)
{
    int? id = Convert.ToInt32(request.RouteValues["id"]);
    Order? order = await db.Order.FirstOrDefaultAsync(o => o.Id == id);
    if (order == null) return Results.NotFound(new { message = "����� �� ������!" });
    db.Order.Remove(order);
    await db.SaveChangesAsync();
    return Results.Json(order);
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
public class ProductName
{
    public string Title { get; set; } = string.Empty;
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
public class ProductComparer : IEqualityComparer<ProductName>
{
    public bool Equals(ProductName x, ProductName y)
    {
        if(Object.ReferenceEquals(x, y)) return true; //��������, ��� ������� ��������� �� ���� ������
        if(Object.ReferenceEquals(x,null) ||  Object.ReferenceEquals(y,null)) return false; //��������, ��� �����-���� ������ ����� ����
        return x.Title == y.Title && x.Price == y.Price; //�������� �� ��������� ������� ��������
    }
    public int GetHashCode(ProductName product) 
    { 
        if(Object.ReferenceEquals(product, null)) return 0; //��������, ��� ������ �� null
        int hashProductName = product.Title == null ? 0: product.Title.GetHashCode(); //��� ��� ��������
        int hashProductPrice = product.Price.GetHashCode(); //��� ��� ����
        return hashProductName ^ hashProductPrice; //��� ��� ����� ��������
    }
}