namespace StockOrders.Domain.Entities;

public class Stock
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public uint RowVersion { get; set; } // Optimistic concurrency token

    // Navigation
    public Product? Product { get; set; }
}
