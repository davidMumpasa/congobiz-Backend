namespace CongoBiz.Application.DTOs.Reports;

public class SalesSummaryDto
{
    public decimal TotalSales { get; set; }
    public int OrderCount { get; set; }
    public decimal AverageOrderValue { get; set; }
    public List<BestSellerDto> BestSellers { get; set; } = new();
    public List<DailySalesDto> DailyBreakdown { get; set; } = new();
}

public class BestSellerDto
{
    public string MenuItemName { get; set; } = string.Empty;
    public int QuantitySold { get; set; }
    public decimal Revenue { get; set; }
}

public class DailySalesDto
{
    public DateOnly Date { get; set; }
    public decimal Total { get; set; }
    public int OrderCount { get; set; }
}

public class ProfitSummaryDto
{
    public decimal TotalRevenue { get; set; }
    public decimal TotalExpenses { get; set; }
    public decimal NetProfit { get; set; }
    public List<ExpenseByCategoryDto> ExpensesByCategory { get; set; } = new();
}

public class ExpenseByCategoryDto
{
    public string Category { get; set; } = string.Empty;
    public decimal Total { get; set; }
}

public class InventoryValuationDto
{
    public decimal TotalValue { get; set; }
    public List<InventoryValuationLineDto> Items { get; set; } = new();
}

public class InventoryValuationLineDto
{
    public string Name { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public string Unit { get; set; } = string.Empty;
    public decimal CostPerUnit { get; set; }
    public decimal Value { get; set; }
}