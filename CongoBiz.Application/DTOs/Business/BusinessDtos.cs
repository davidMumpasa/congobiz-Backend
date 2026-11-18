namespace CongoBiz.Application.DTOs.Business;

// Tables
public class TableDto
{
    public Guid Id { get; set; }
    public int TableNumber { get; set; }
    public int Capacity { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class CreateTableRequest
{
    public int TableNumber { get; set; }
    public int Capacity { get; set; }
}

public class UpdateTableStatusRequest
{
    public string Status { get; set; } = string.Empty; // Available, Occupied, Reserved
}

// Customers
public class CustomerDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string? LastName { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Notes { get; set; }
}

public class CreateCustomerRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string? LastName { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Notes { get; set; }
}

// Inventory
public class InventoryItemDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal MinimumQuantity { get; set; }
    public decimal CostPerUnit { get; set; }
    public bool IsLow { get; set; }
}

public class CreateInventoryItemRequest
{
    public string Name { get; set; } = string.Empty;
    public string Unit { get; set; } = "unit";
    public decimal Quantity { get; set; }
    public decimal MinimumQuantity { get; set; }
    public decimal CostPerUnit { get; set; }
}

public class CreateStockMovementRequest
{
    public Guid InventoryItemId { get; set; }
    public string Type { get; set; } = string.Empty; // Purchase, Sale, Adjustment, Waste, Return
    public decimal Quantity { get; set; }
    public string? Reason { get; set; }
}

// Expenses
public class ExpenseDto
{
    public Guid Id { get; set; }
    public string Category { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
}

public class CreateExpenseRequest
{
    public string Category { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
}