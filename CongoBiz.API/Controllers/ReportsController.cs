using CongoBiz.Application.DTOs.Reports;
using CongoBiz.Domain.Enums;
using CongoBiz.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CongoBiz.API.Controllers;

[Route("api/reports")]
[Authorize(Roles = "Owner,Manager")]
public class ReportsController : ApiControllerBase
{
    private readonly AppDbContext _db;
    public ReportsController(AppDbContext db) => _db = db;

    // GET /api/reports/sales?from=2026-08-01&to=2026-08-21
    [HttpGet("sales")]
    public async Task<ActionResult<SalesSummaryDto>> GetSales(DateOnly? from, DateOnly? to)
    {
        var start = (from ?? DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-30))).ToDateTime(TimeOnly.MinValue);
        var end = (to ?? DateOnly.FromDateTime(DateTime.UtcNow)).ToDateTime(TimeOnly.MaxValue);

        var orders = await _db.Orders
            .Where(o => o.RestaurantId == RestaurantId
                && o.Status != OrderStatus.Cancelled
                && o.CreatedAt >= start && o.CreatedAt <= end)
            .Include(o => o.Items).ThenInclude(i => i.MenuItem)
            .ToListAsync();

        var totalSales = orders.Sum(o => o.Total);
        var orderCount = orders.Count;

        var bestSellers = orders
            .SelectMany(o => o.Items)
            .GroupBy(i => i.MenuItem.Name)
            .Select(g => new BestSellerDto
            {
                MenuItemName = g.Key,
                QuantitySold = g.Sum(i => i.Quantity),
                Revenue = g.Sum(i => i.Subtotal)
            })
            .OrderByDescending(b => b.QuantitySold)
            .Take(10)
            .ToList();

        var dailyBreakdown = orders
            .GroupBy(o => DateOnly.FromDateTime(o.CreatedAt))
            .Select(g => new DailySalesDto
            {
                Date = g.Key,
                Total = g.Sum(o => o.Total),
                OrderCount = g.Count()
            })
            .OrderBy(d => d.Date)
            .ToList();

        return Ok(new SalesSummaryDto
        {
            TotalSales = totalSales,
            OrderCount = orderCount,
            AverageOrderValue = orderCount > 0 ? Math.Round(totalSales / orderCount, 2) : 0,
            BestSellers = bestSellers,
            DailyBreakdown = dailyBreakdown
        });
    }

    // GET /api/reports/profit?from=2026-08-01&to=2026-08-21
    [HttpGet("profit")]
    public async Task<ActionResult<ProfitSummaryDto>> GetProfit(DateOnly? from, DateOnly? to)
    {
        var start = (from ?? DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-30))).ToDateTime(TimeOnly.MinValue);
        var end = (to ?? DateOnly.FromDateTime(DateTime.UtcNow)).ToDateTime(TimeOnly.MaxValue);

        var revenue = await _db.Orders
            .Where(o => o.RestaurantId == RestaurantId
                && o.Status != OrderStatus.Cancelled
                && o.CreatedAt >= start && o.CreatedAt <= end)
            .SumAsync(o => o.Total);

        var expenses = await _db.Expenses
            .Where(e => e.RestaurantId == RestaurantId && e.Date >= start && e.Date <= end)
            .ToListAsync();

        var expensesByCategory = expenses
            .GroupBy(e => e.Category)
            .Select(g => new ExpenseByCategoryDto { Category = g.Key, Total = g.Sum(e => e.Amount) })
            .OrderByDescending(e => e.Total)
            .ToList();

        var totalExpenses = expenses.Sum(e => e.Amount);

        return Ok(new ProfitSummaryDto
        {
            TotalRevenue = revenue,
            TotalExpenses = totalExpenses,
            NetProfit = revenue - totalExpenses,
            ExpensesByCategory = expensesByCategory
        });
    }

    // GET /api/reports/inventory-valuation
    [HttpGet("inventory-valuation")]
    public async Task<ActionResult<InventoryValuationDto>> GetInventoryValuation()
    {
        var items = await _db.InventoryItems
            .Where(i => i.RestaurantId == RestaurantId && i.IsActive)
            .Select(i => new InventoryValuationLineDto
            {
                Name = i.Name,
                Quantity = i.Quantity,
                Unit = i.Unit,
                CostPerUnit = i.CostPerUnit,
                Value = i.Quantity * i.CostPerUnit
            })
            .ToListAsync();

        return Ok(new InventoryValuationDto
        {
            TotalValue = items.Sum(i => i.Value),
            Items = items.OrderByDescending(i => i.Value).ToList()
        });
    }
}