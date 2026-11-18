using CongoBiz.Domain.Enums;

namespace CongoBiz.Domain.Entities;

public class RestaurantTable
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid RestaurantId { get; set; }
    public Restaurant Restaurant { get; set; } = null!;
    public int TableNumber { get; set; }
    public int Capacity { get; set; }
    public TableStatus Status { get; set; } = TableStatus.Available;
}