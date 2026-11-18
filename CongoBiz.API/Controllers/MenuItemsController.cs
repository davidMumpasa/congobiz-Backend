using CongoBiz.Application.DTOs.Menu;
using CongoBiz.Domain.Entities;
using CongoBiz.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CongoBiz.API.Controllers;

[Route("api/menu-items")]
public class MenuItemsController : ApiControllerBase
{
    private readonly AppDbContext _db;
    public MenuItemsController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<List<MenuItemDto>>> GetAll()
    {
        var items = await _db.MenuItems
            .Where(i => i.RestaurantId == RestaurantId)
            .Select(i => new MenuItemDto
            {
                Id = i.Id, CategoryId = i.CategoryId, Name = i.Name,
                Description = i.Description, Price = i.Price,
                ImageUrl = i.ImageUrl, IsAvailable = i.IsAvailable
            })
            .ToListAsync();

        return Ok(items);
    }

    [HttpPost]
    [Authorize(Roles = "Owner,Manager")]
    public async Task<ActionResult<MenuItemDto>> Create(CreateMenuItemRequest request)
    {
        var category = await _db.MenuCategories
            .FirstOrDefaultAsync(c => c.Id == request.CategoryId && c.RestaurantId == RestaurantId);
        if (category == null) return BadRequest("Category not found.");

        var item = new MenuItem
        {
            RestaurantId = RestaurantId,
            CategoryId = request.CategoryId,
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            ImageUrl = request.ImageUrl
        };
        _db.MenuItems.Add(item);
        await _db.SaveChangesAsync();

        return Ok(new MenuItemDto
        {
            Id = item.Id, CategoryId = item.CategoryId, Name = item.Name,
            Description = item.Description, Price = item.Price,
            ImageUrl = item.ImageUrl, IsAvailable = item.IsAvailable
        });
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Owner,Manager")]
    public async Task<ActionResult<MenuItemDto>> Update(Guid id, UpdateMenuItemRequest request)
    {
        var item = await _db.MenuItems
            .FirstOrDefaultAsync(i => i.Id == id && i.RestaurantId == RestaurantId);
        if (item == null) return NotFound();

        item.Name = request.Name;
        item.Description = request.Description;
        item.Price = request.Price;
        item.ImageUrl = request.ImageUrl;
        item.IsAvailable = request.IsAvailable;
        item.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return Ok(new MenuItemDto
        {
            Id = item.Id, CategoryId = item.CategoryId, Name = item.Name,
            Description = item.Description, Price = item.Price,
            ImageUrl = item.ImageUrl, IsAvailable = item.IsAvailable
        });
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Owner,Manager")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var item = await _db.MenuItems
            .FirstOrDefaultAsync(i => i.Id == id && i.RestaurantId == RestaurantId);
        if (item == null) return NotFound();

        _db.MenuItems.Remove(item);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}