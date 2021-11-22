using Microsoft.EntityFrameworkCore;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<RXData>(opt => opt.UseInMemoryDatabase("RxData"));
builder.Services.AddDataProtection();

// Add services to the container.

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.MapGet("/", () => "Hello, World");

app.MapGet("/rxitems", async (RXData rxData) =>
{
    await rxData.Data.ToListAsync();
});

app.MapGet("/rxitems/complete", async (RXData rxData) =>
{
    await rxData.Data.Where(x => x.isComplete).ToListAsync();
});

app.MapGet("/rxitems/{id}", async (int id, RXData rxData) =>
{
   return await rxData.Data.FindAsync(id) is RXD rxd ? Results.Ok(rxd) : Results.NotFound();
});

app.MapPost("/rxitems/add", async (RXD rxd, RXData rxData) =>
{
    rxData.Add(rxd);
    await rxData.SaveChangesAsync();

    return Results.Created($"/rxitems/{rxd.Id}", rxd);
});

app.MapPut("/rxitems/update/{id}", async (int id, RXD rxd, RXData rxData) =>
{
   var rx = await rxData.Data.FindAsync(id);
   if (rx is null)
    {
        return Results.NotFound();
    }

    rx.Name = rxd.Name;
    rx.isComplete = rxd.isComplete;

    await rxData.SaveChangesAsync();
    
    return Results.NoContent();

});

app.MapDelete("/rxitems/delete/{id}", async (int id, RXData rxData) =>
{
    if (await rxData.Data.FindAsync(id) is RXD rxd)
    {
        rxData.Data.Remove(rxd);
        await rxData.SaveChangesAsync();
        return Results.Ok(rxd);
    }

    return Results.NotFound();
});

app.Run();

class RXD
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public bool isComplete { get; set; }
}

class RXData : DbContext
{
    public RXData(DbContextOptions<RXData> options) : base(options) { }

    public DbSet<RXD> Data => Set<RXD>();
}