using CongoBiz.Application.DTOs.Business;
using CongoBiz.Domain.Entities;
using CongoBiz.Domain.Enums;
using CongoBiz.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CongoBiz.API.Controllers;

[Route("api/tables")]
[Authorize(Roles = "Owner,Manager,Cashier,Waiter")]
public class TablesController : ApiControllerBase
{
    private readonly AppDbContext _db;
    public TablesController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<List<TableDto>>> GetAll()
    {
        var tables = await _db.RestaurantTables
            .Where(t => t.RestaurantId == RestaurantId)
            .OrderBy(t => t.TableNumber)
            .Select(t => new TableDto
            {
                Id = t.Id, TableNumber = t.TableNumber,
                Capacity = t.Capacity, Status = t.Status.ToString()
            })
            .ToListAsync();

        return Ok(tables);
    }

    [HttpPost]
    [Authorize(Roles = "Owner,Manager,Cashier,Waiter")]
    public async Task<ActionResult<TableDto>> Create(CreateTableRequest request)
    {
        var table = new RestaurantTable
        {
            RestaurantId = RestaurantId,
            TableNumber = request.TableNumber,
            Capacity = request.Capacity
        };
        _db.RestaurantTables.Add(table);
        await _db.SaveChangesAsync();

        return Ok(new TableDto
        {
            Id = table.Id,
            TableNumber = table.TableNumber,
            Capacity = table.Capacity,
            Status = table.Status.ToString()
        });
    }

    [HttpPut("{id}/status")]
    public async Task<ActionResult<TableDto>> UpdateStatus(Guid id, UpdateTableStatusRequest request)
    {
        var table = await _db.RestaurantTables
            .FirstOrDefaultAsync(t => t.Id == id && t.RestaurantId == RestaurantId);
        if (table == null) return NotFound();

        if (!Enum.TryParse<TableStatus>(request.Status, out var status))
            return BadRequest("Invalid status. Use Available, Occupied, or Reserved.");

        table.Status = status;
        await _db.SaveChangesAsync();

        return Ok(new TableDto
        {
            Id = table.Id, TableNumber = table.TableNumber,
            Capacity = table.Capacity, Status = table.Status.ToString()
        });
    }

    [HttpDelete("{id}")]
[Authorize(Roles = "Owner,Manager")]
public async Task<IActionResult> Delete(Guid id)
    {
        var table = await _db.RestaurantTables
            .FirstOrDefaultAsync(t => t.Id == id && t.RestaurantId == RestaurantId);
        if (table == null) return NotFound();

        _db.RestaurantTables.Remove(table);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}