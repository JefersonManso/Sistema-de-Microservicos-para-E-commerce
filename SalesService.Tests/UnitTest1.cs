using Xunit;
using SalesService.Data;
using Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace SalesService.Tests;

public class SalesServiceTests
{
    private SalesDbContext GetDbContext()
    {
        var options = new DbContextOptionsBuilder<SalesDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new SalesDbContext(options);
    }

    [Fact]
    public async Task AddOrder_ShouldAddOrderToDatabase()
    {
        // Arrange
        var db = GetDbContext();
        var order = new Order { ProductId = 1, Quantity = 2, TotalPrice = 20.0m, Date = DateTime.UtcNow, Status = "Pending" };

        // Act
        db.Orders.Add(order);
        await db.SaveChangesAsync();

        // Assert
        var savedOrder = await db.Orders.FindAsync(order.Id);
        Assert.NotNull(savedOrder);
        Assert.Equal(1, savedOrder.ProductId);
        Assert.Equal(2, savedOrder.Quantity);
    }

    [Fact]
    public async Task UpdateOrder_ShouldUpdateOrderInDatabase()
    {
        // Arrange
        var db = GetDbContext();
        var order = new Order { ProductId = 1, Quantity = 2, TotalPrice = 20.0m, Date = DateTime.UtcNow, Status = "Pending" };
        db.Orders.Add(order);
        await db.SaveChangesAsync();

        // Act
        order.Status = "Confirmed";
        await db.SaveChangesAsync();

        // Assert
        var updatedOrder = await db.Orders.FindAsync(order.Id);
        Assert.Equal("Confirmed", updatedOrder.Status);
    }
}