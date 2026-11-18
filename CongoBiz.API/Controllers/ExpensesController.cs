using CongoBiz.Application.DTOs.Business;
using CongoBiz.Domain.Entities;
using CongoBiz.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CongoBiz.API.Controllers;

[Route("api/expenses")]
[Authorize(Roles = "Owner,Manager")]
public class ExpensesController : ApiControllerBase
{
    private readonly AppDbContext _db;
    public ExpensesController(AppDbContext db) => _db = db;

    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? User.FindFirst("sub")!.Value);

    [HttpGet]
    public async Task<ActionResult<List<ExpenseDto>>> GetAll()
    {
        var expenses = await _db.Expenses
            .Where(e => e.RestaurantId == RestaurantId)
            .OrderByDescending(e => e.Date)
            .Select(e => new ExpenseDto
            {
                Id = e.Id, Category = e.Category, Description = e.Description,
                Amount = e.Amount, Date = e.Date
            })
            .ToListAsync();

        return Ok(expenses);
    }

    [HttpPost]
    public async Task<ActionResult<ExpenseDto>> Create(CreateExpenseRequest request)
    {
        var expense = new Expense
        {
            RestaurantId = RestaurantId,
            Category = request.Category,
            Description = request.Description,
            Amount = request.Amount,
            Date = request.Date,
            CreatedByUserId = UserId
        };
        _db.Expenses.Add(expense);
        await _db.SaveChangesAsync();

        return Ok(new ExpenseDto
        {
            Id = expense.Id, Category = expense.Category, Description = expense.Description,
            Amount = expense.Amount, Date = expense.Date
        });
    }
}