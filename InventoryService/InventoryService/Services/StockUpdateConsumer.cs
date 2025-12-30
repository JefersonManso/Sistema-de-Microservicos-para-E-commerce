using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using InventoryService.Data;
using Microsoft.Extensions.DependencyInjection;
using Shared.Models;

namespace InventoryService.Services;

public class StockUpdateConsumer : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;

    public StockUpdateConsumer(IServiceProvider serviceProvider, IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _configuration = configuration;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory()
        {
            HostName = _configuration["RabbitMQ:HostName"],
            UserName = _configuration["RabbitMQ:UserName"],
            Password = _configuration["RabbitMQ:Password"]
        };

        var connection = factory.CreateConnection();
        var channel = connection.CreateModel();

        channel.QueueDeclare(queue: "stock_update", durable: false, exclusive: false, autoDelete: false, arguments: null);

        var consumer = new EventingBasicConsumer(channel);

        consumer.Received += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            var update = JsonSerializer.Deserialize<StockUpdate>(message);

            using (var scope = _serviceProvider.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
                var product = await db.Products.FindAsync(update.ProductId);
                if (product != null)
                {
                    product.StockQuantity -= update.Quantity;
                    await db.SaveChangesAsync();
                }
            }
        };

        channel.BasicConsume(queue: "stock_update", autoAck: true, consumer: consumer);

        return Task.CompletedTask;
    }
}