using CongoBiz.Application.DTOs.Business;
using CongoBiz.Domain.Entities;
using CongoBiz.Domain.Enums;
using CongoBiz.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CongoBiz.API.Controllers;

[Route("api/inventory")]
[Authorize(Roles = "Owner,Manager,Kitchen")]
public class InventoryController : ApiControllerBase
{
    private readonly AppDbContext _db;
    public InventoryController(AppDbContext db) => _db = db;

    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? User.FindFirst("sub")!.Value);

    [HttpGet]
    public async Task<ActionResult<List<InventoryItemDto>>> GetAll()
    {
        var items = await _db.InventoryItems
            .Where(i => i.RestaurantId == RestaurantId && i.IsActive)
            .OrderBy(i => i.Name)
            .Select(i => new InventoryItemDto
            {
                Id = i.Id, Name = i.Name, Unit = i.Unit,
                Quantity = i.Quantity, MinimumQuantity = i.MinimumQuantity,
                CostPerUnit = i.CostPerUnit, IsLow = i.Quantity <= i.MinimumQuantity
            })
            .ToListAsync();

        return Ok(items);
    }

    [HttpPost]
    public async Task<ActionResult<InventoryItemDto>> Create(CreateInventoryItemRequest request)
    {
        var item = new InventoryItem
        {
            RestaurantId = RestaurantId,
            Name = request.Name,
            Unit = request.Unit,
            Quantity = request.Quantity,
            MinimumQuantity = request.MinimumQuantity,
            CostPerUnit = request.CostPerUnit
        };
        _db.InventoryItems.Add(item);
        await _db.SaveChangesAsync();

        return Ok(new InventoryItemDto
        {
            Id = item.Id, Name = item.Name, Unit = item.Unit,
            Quantity = item.Quantity, MinimumQuantity = item.MinimumQuantity,
            CostPerUnit = item.CostPerUnit, IsLow = item.Quantity <= item.MinimumQuantity
        });
    }

    // Every stock change goes through here — never edit Quantity directly.
    // This keeps a full audit trail via StockMovement, matching the original spec.
    [HttpPost("movements")]
    public async Task<ActionResult<InventoryItemDto>> RecordMovement(CreateStockMovementRequest request)
    {
        var item = await _db.InventoryItems
            .FirstOrDefaultAsync(i => i.Id == request.InventoryItemId && i.RestaurantId == RestaurantId);
        if (item == null) return NotFound("Inventory item not found.");

        if (!Enum.TryParse<StockMovementType>(request.Type, out var type))
            return BadRequest("Invalid movement type. Use Purchase, Sale, Adjustment, Waste, or Return.");

        // Purchases and returns add stock; sales, waste, and adjustments (as entered) subtract it.
        var delta = type is StockMovementType.Purchase or StockMovementType.Return
            ? request.Quantity
            : -request.Quantity;

        item.Quantity += delta;
        if (item.Quantity < 0) item.Quantity = 0;

        _db.StockMovements.Add(new StockMovement
        {
            RestaurantId = RestaurantId,
            InventoryItemId = item.Id,
            Type = type,
            Quantity = request.Quantity,
            Reason = request.Reason,
            CreatedByUserId = UserId
        });

        await _db.SaveChangesAsync();

        return Ok(new InventoryItemDto
        {
            Id = item.Id, Name = item.Name, Unit = item.Unit,
            Quantity = item.Quantity, MinimumQuantity = item.MinimumQuantity,
            CostPerUnit = item.CostPerUnit, IsLow = item.Quantity <= item.MinimumQuantity
        });
    }
}