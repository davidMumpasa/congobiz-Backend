namespace CongoBiz.Domain.Entities;

public class InventoryItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid RestaurantId { get; set; }
    public Restaurant Restaurant { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public string Unit { get; set; } = "unit"; // kg, l, unit, etc.
    public decimal Quantity { get; set; }
    public decimal MinimumQuantity { get; set; }
    public decimal CostPerUnit { get; set; }
    public bool IsActive { get; set; } = true;
}