using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using Shared.Models;

namespace SalesService.Services;

public class RabbitMQPublisher
{
    private readonly IConfiguration _configuration;

    public RabbitMQPublisher(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void PublishStockUpdate(int productId, int quantity)
    {
        var factory = new ConnectionFactory()
        {
            HostName = _configuration["RabbitMQ:HostName"],
            UserName = _configuration["RabbitMQ:UserName"],
            Password = _configuration["RabbitMQ:Password"]
        };

        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        channel.QueueDeclare(queue: "stock_update", durable: false, exclusive: false, autoDelete: false, arguments: null);

        var update = new StockUpdate { ProductId = productId, Quantity = quantity };
        var message = JsonSerializer.Serialize(update);
        var body = Encoding.UTF8.GetBytes(message);

        channel.BasicPublish(exchange: "", routingKey: "stock_update", basicProperties: null, body: body);
    }
}