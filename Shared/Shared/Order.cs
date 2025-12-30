using System.ComponentModel.DataAnnotations;

namespace Shared.Models;

public class Order
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    public int Quantity { get; set; }

    public decimal TotalPrice { get; set; }

    public DateTime Date { get; set; }

    public string Status { get; set; } // e.g., Pending, Confirmed, Cancelled
}