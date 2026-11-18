using CongoBiz.Application.DTOs.Business;
using CongoBiz.Domain.Entities;
using CongoBiz.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CongoBiz.API.Controllers;

[Route("api/customers")]
[Authorize(Roles = "Owner,Manager,Cashier,Waiter")]
public class CustomersController : ApiControllerBase
{
    private readonly AppDbContext _db;
    public CustomersController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<List<CustomerDto>>> GetAll()
    {
        var customers = await _db.Customers
            .Where(c => c.RestaurantId == RestaurantId)
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new CustomerDto
            {
                Id = c.Id, FirstName = c.FirstName, LastName = c.LastName,
                Phone = c.Phone, Email = c.Email, Notes = c.Notes
            })
            .ToListAsync();

        return Ok(customers);
    }

    [HttpPost]
    public async Task<ActionResult<CustomerDto>> Create(CreateCustomerRequest request)
    {
        var customer = new Customer
        {
            RestaurantId = RestaurantId,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Phone = request.Phone,
            Email = request.Email,
            Notes = request.Notes
        };
        _db.Customers.Add(customer);
        await _db.SaveChangesAsync();

        return Ok(new CustomerDto
        {
            Id = customer.Id, FirstName = customer.FirstName, LastName = customer.LastName,
            Phone = customer.Phone, Email = customer.Email, Notes = customer.Notes
        });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var customer = await _db.Customers
            .FirstOrDefaultAsync(c => c.Id == id && c.RestaurantId == RestaurantId);
        if (customer == null) return NotFound();

        _db.Customers.Remove(customer);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}