using CongoBiz.Domain.Enums;

namespace CongoBiz.Application.DTOs.Orders;

public class OrderItemRequest
{
    public Guid MenuItemId { get; set; }
    public int Quantity { get; set; }
    public string? Notes { get; set; }
}

public class CreateOrderRequest
{
    public Guid? TableId { get; set; }
    public OrderType OrderType { get; set; }
    public List<OrderItemRequest> Items { get; set; } = new();
}

public class UpdateOrderStatusRequest
{
    public OrderStatus Status { get; set; }
}

public class OrderItemDto
{
    public Guid MenuItemId { get; set; }
    public string MenuItemName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Subtotal { get; set; }
    public string? Notes { get; set; }
}

public class OrderDto
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string OrderType { get; set; } = string.Empty;
    public Guid? TableId { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Tax { get; set; }
    public decimal Total { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<OrderItemDto> Items { get; set; } = new();
}