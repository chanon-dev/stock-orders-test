namespace StockOrders.Domain.Entities;

public class Stock
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }

    // Navigation
    public Product? Product { get; set; }
}
