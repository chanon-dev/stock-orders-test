namespace StockOrders.Domain.Entities;

public class CartItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public string SessionId { get; set; } = string.Empty;

    // Navigation
    public Product? Product { get; set; }
}
