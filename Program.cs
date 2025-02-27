using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
string conn = builder.Configuration.GetConnectionString("DefaultConnection")!;
conn = conn.Replace("replacement_name", Environment.MachineName);
builder.Services.AddDbContext<ApContext>(options => options.UseSqlServer(conn));
var app = builder.Build();
app.UseDefaultFiles(); //поддержка файлов index.html по-умолчанию
app.UseStaticFiles(); //поддержка статических файлов в папке wwwroot

app.MapGet("/api/order", getOrders); //обработка GET запроса
app.MapGet("/api/productnames", getProductNames); //обработка GET запроса
app.MapDelete("/api/deleteOrder/{id:int}", deleteOrder); //обработка GET запроса

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
    if (order == null) return Results.NotFound(new { message = "Заказ не найден!" });
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
    public DbSet<Order> Order { get; set; } = null!; //коллекция для таблицы заказы
    public DbSet<Product> Product { get; set; } = null!; //коллекция для таблицы товары
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
        if(Object.ReferenceEquals(x, y)) return true; //проверка, что объекты ссылаются на одни данные
        if(Object.ReferenceEquals(x,null) ||  Object.ReferenceEquals(y,null)) return false; //проверка, что какой-либо объект равен нулю
        return x.Title == y.Title && x.Price == y.Price; //проверка на равенство свойств объектов
    }
    public int GetHashCode(ProductName product) 
    { 
        if(Object.ReferenceEquals(product, null)) return 0; //проверка, что объект на null
        int hashProductName = product.Title == null ? 0: product.Title.GetHashCode(); //хеш для названия
        int hashProductPrice = product.Price.GetHashCode(); //хеш для цены
        return hashProductName ^ hashProductPrice; //хеш для всего продукта
    }
}