using Xunit;
using InventoryService.Data;
using Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace InventoryService.Tests;

public class InventoryServiceTests
{
    private InventoryDbContext GetDbContext()
    {
        var options = new DbContextOptionsBuilder<InventoryDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new InventoryDbContext(options);
    }

    [Fact]
    public async Task AddProduct_ShouldAddProductToDatabase()
    {
        // Arrange
        var db = GetDbContext();
        var product = new Product { Name = "Test Product", Description = "Test Description", Price = 10.0m, StockQuantity = 100 };

        // Act
        db.Products.Add(product);
        await db.SaveChangesAsync();

        // Assert
        var savedProduct = await db.Products.FindAsync(product.Id);
        Assert.NotNull(savedProduct);
        Assert.Equal("Test Product", savedProduct.Name);
        Assert.Equal(100, savedProduct.StockQuantity);
    }

    [Fact]
    public async Task UpdateProduct_ShouldUpdateProductInDatabase()
    {
        // Arrange
        var db = GetDbContext();
        var product = new Product { Name = "Test Product", Description = "Test", Price = 10.0m, StockQuantity = 100 };
        db.Products.Add(product);
        await db.SaveChangesAsync();

        // Act
        product.StockQuantity = 50;
        await db.SaveChangesAsync();

        // Assert
        var updatedProduct = await db.Products.FindAsync(product.Id);
        Assert.Equal(50, updatedProduct.StockQuantity);
    }

    [Fact]
    public async Task DeleteProduct_ShouldRemoveProductFromDatabase()
    {
        // Arrange
        var db = GetDbContext();
        var product = new Product { Name = "Test Product", Description = "Test", Price = 10.0m, StockQuantity = 100 };
        db.Products.Add(product);
        await db.SaveChangesAsync();

        // Act
        db.Products.Remove(product);
        await db.SaveChangesAsync();

        // Assert
        var deletedProduct = await db.Products.FindAsync(product.Id);
        Assert.Null(deletedProduct);
    }
}