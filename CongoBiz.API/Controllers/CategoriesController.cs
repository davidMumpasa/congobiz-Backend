using CongoBiz.Application.DTOs.Menu;
using CongoBiz.Domain.Entities;
using CongoBiz.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CongoBiz.API.Controllers;

[Route("api/categories")]
public class CategoriesController : ApiControllerBase
{
    private readonly AppDbContext _db;
    public CategoriesController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<List<CategoryDto>>> GetAll()
    {
        var categories = await _db.MenuCategories
            .Where(c => c.RestaurantId == RestaurantId)
            .OrderBy(c => c.DisplayOrder)
            .Select(c => new CategoryDto
            {
                Id = c.Id, Name = c.Name, Description = c.Description,
                DisplayOrder = c.DisplayOrder, IsActive = c.IsActive
            })
            .ToListAsync();

        return Ok(categories);
    }

    [HttpPost]
    [Authorize(Roles = "Owner,Manager")]
    public async Task<ActionResult<CategoryDto>> Create(CreateCategoryRequest request)
    {
        var category = new MenuCategory
        {
            RestaurantId = RestaurantId,
            Name = request.Name,
            Description = request.Description,
            DisplayOrder = request.DisplayOrder
        };
        _db.MenuCategories.Add(category);
        await _db.SaveChangesAsync();

        return Ok(new CategoryDto
        {
            Id = category.Id, Name = category.Name, Description = category.Description,
            DisplayOrder = category.DisplayOrder, IsActive = category.IsActive
        });
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Owner,Manager")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var category = await _db.MenuCategories
            .FirstOrDefaultAsync(c => c.Id == id && c.RestaurantId == RestaurantId);
        if (category == null) return NotFound();

        _db.MenuCategories.Remove(category);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}