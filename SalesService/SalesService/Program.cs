using SalesService.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using RabbitMQ.Client;
using Shared.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
using SalesService.Services;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddDbContext<SalesDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddHttpClient();
builder.Services.AddSingleton<SalesService.Services.RabbitMQPublisher>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// Map endpoints
var ordersGroup = app.MapGroup("/api/orders").RequireAuthorization();

ordersGroup.MapGet("/", async (SalesDbContext db) =>
    await db.Orders.ToListAsync());

ordersGroup.MapGet("/{id}", async (int id, SalesDbContext db) =>
    await db.Orders.FindAsync(id) is Order order ? Results.Ok(order) : Results.NotFound());

// POST to create order
ordersGroup.MapPost("/", async (Order order, [FromServices] SalesDbContext db, [FromServices] IHttpClientFactory clientFactory, [FromServices] RabbitMQPublisher publisher) =>
{
    var client = clientFactory.CreateClient();
    var response = await client.GetAsync("http://localhost:5001/api/products/" + order.ProductId);
    if (!response.IsSuccessStatusCode)
    {
        Log.Warning("Product not found: {ProductId}", order.ProductId);
        return Results.NotFound("Product not found");
    }

    var product = await response.Content.ReadFromJsonAsync<Product>();
    if (product == null || product.StockQuantity < order.Quantity)
    {
        Log.Warning("Insufficient stock for product {ProductId}, requested {Quantity}, available {Stock}", order.ProductId, order.Quantity, product?.StockQuantity);
        return Results.BadRequest("Insufficient stock");
    }

    order.TotalPrice = product.Price * order.Quantity;
    order.Date = DateTime.UtcNow;
    order.Status = "Confirmed";

    db.Orders.Add(order);
    await db.SaveChangesAsync();

    publisher.PublishStockUpdate(order.ProductId, order.Quantity);

    Log.Information("Order created: {OrderId} for product {ProductId}", order.Id, order.ProductId);
    return Results.Created($"/api/orders/{order.Id}", order);
});

ordersGroup.MapPut("/{id}", async (int id, Order inputOrder, SalesDbContext db) =>
{
    var order = await db.Orders.FindAsync(id);
    if (order is null) return Results.NotFound();
    order.Status = inputOrder.Status;
    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
