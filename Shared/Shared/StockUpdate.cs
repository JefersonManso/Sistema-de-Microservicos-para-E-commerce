using System.ComponentModel.DataAnnotations;

namespace Shared.Models;

public class StockUpdate
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}