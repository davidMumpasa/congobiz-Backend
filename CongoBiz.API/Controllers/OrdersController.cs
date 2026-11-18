using CongoBiz.API.Hubs;
using CongoBiz.Application.DTOs.Orders;
using CongoBiz.Domain.Entities;
using CongoBiz.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace CongoBiz.API.Controllers;

[Route("api/orders")]
public class OrdersController : ApiControllerBase
{
    private readonly AppDbContext _db;
    private readonly IHubContext<OrderHub> _hub;

    public OrdersController(AppDbContext db, IHubContext<OrderHub> hub)
    {
        _db = db;
        _hub = hub;
    }

    [HttpGet]
    public async Task<ActionResult<List<OrderDto>>> GetAll()
    {
        var orders = await _db.Orders
            .Where(o => o.RestaurantId == RestaurantId)
            .Include(o => o.Items).ThenInclude(i => i.MenuItem)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        return Ok(orders.Select(MapToDto));
    }

    [HttpPost]
    public async Task<ActionResult<OrderDto>> Create(CreateOrderRequest request)
    {
        var menuItemIds = request.Items.Select(i => i.MenuItemId).ToList();
        var menuItems = await _db.MenuItems
            .Where(m => menuItemIds.Contains(m.Id) && m.RestaurantId == RestaurantId)
            .ToListAsync();

        if (menuItems.Count != menuItemIds.Distinct().Count())
            return BadRequest("One or more menu items are invalid.");

        var order = new Order
        {
            RestaurantId = RestaurantId,
            TableId = request.TableId,
            OrderType = request.OrderType,
            OrderNumber = $"#{Random.Shared.Next(1000, 9999)}"
        };

        foreach (var line in request.Items)
        {
            var menuItem = menuItems.First(m => m.Id == line.MenuItemId);
            var subtotal = menuItem.Price * line.Quantity;
            order.Items.Add(new OrderItem
            {
                MenuItemId = menuItem.Id,
                Quantity = line.Quantity,
                UnitPrice = menuItem.Price,
                Subtotal = subtotal,
                Notes = line.Notes
            });
            order.Subtotal += subtotal;
        }
        order.Total = order.Subtotal; // tax/discount logic comes later

        _db.Orders.Add(order);
        await _db.SaveChangesAsync();

        var dto = MapToDto(order);
        await _hub.Clients.Group(RestaurantId.ToString()).SendAsync("OrderCreated", dto);

        return Ok(dto);
    }

    [HttpPut("{id}/status")]
    public async Task<ActionResult<OrderDto>> UpdateStatus(Guid id, UpdateOrderStatusRequest request)
    {
        var order = await _db.Orders
            .Include(o => o.Items).ThenInclude(i => i.MenuItem)
            .FirstOrDefaultAsync(o => o.Id == id && o.RestaurantId == RestaurantId);
        if (order == null) return NotFound();

        order.Status = request.Status;
        order.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        var dto = MapToDto(order);
        await _hub.Clients.Group(RestaurantId.ToString()).SendAsync("OrderStatusChanged", dto);

        return Ok(dto);
    }

    private static OrderDto MapToDto(Order o) => new()
    {
        Id = o.Id,
        OrderNumber = o.OrderNumber,
        Status = o.Status.ToString(),
        OrderType = o.OrderType.ToString(),
        TableId = o.TableId,
        Subtotal = o.Subtotal,
        Tax = o.Tax,
        Total = o.Total,
        CreatedAt = o.CreatedAt,
        Items = o.Items.Select(i => new OrderItemDto
        {
            MenuItemId = i.MenuItemId,
            MenuItemName = i.MenuItem.Name,
            Quantity = i.Quantity,
            UnitPrice = i.UnitPrice,
            Subtotal = i.Subtotal,
            Notes = i.Notes
        }).ToList()
    };
}