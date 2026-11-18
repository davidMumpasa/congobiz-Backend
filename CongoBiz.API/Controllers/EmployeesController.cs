using CongoBiz.Application.DTOs.Employees;
using CongoBiz.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CongoBiz.API.Controllers;

[Route("api/employees")]
[Authorize(Roles = "Owner,Manager")]
public class EmployeesController : ApiControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;

    private static readonly string[] AllowedRoles =
        { "Manager", "Cashier", "Waiter", "Kitchen" };

    public EmployeesController(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole<Guid>> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    [HttpGet]
    public async Task<ActionResult<List<EmployeeDto>>> GetAll()
    {
        var users = await _userManager.Users
            .Where(u => u.RestaurantId == RestaurantId)
            .ToListAsync();

        var result = new List<EmployeeDto>();
        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            result.Add(new EmployeeDto
            {
                Id = user.Id, FirstName = user.FirstName, LastName = user.LastName,
                Email = user.Email!, Role = roles.FirstOrDefault() ?? "",
                IsActive = user.IsActive
            });
        }

        return Ok(result);
    }

    [HttpPost("invite")]
    public async Task<ActionResult<EmployeeDto>> Invite(InviteEmployeeRequest request)
    {
        if (!AllowedRoles.Contains(request.Role))
            return BadRequest($"Role must be one of: {string.Join(", ", AllowedRoles)}.");

        var existing = await _userManager.FindByEmailAsync(request.Email);
        if (existing != null) return BadRequest("An account with this email already exists.");

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            RestaurantId = RestaurantId
        };

        var result = await _userManager.CreateAsync(user, request.TemporaryPassword);
        if (!result.Succeeded) return BadRequest(result.Errors.Select(e => e.Description));

        if (!await _roleManager.RoleExistsAsync(request.Role))
            await _roleManager.CreateAsync(new IdentityRole<Guid>(request.Role));

        await _userManager.AddToRoleAsync(user, request.Role);

        return Ok(new EmployeeDto
        {
            Id = user.Id, FirstName = user.FirstName, LastName = user.LastName,
            Email = user.Email!, Role = request.Role, IsActive = user.IsActive
        });
    }

    [HttpPut("{id}/role")]
    public async Task<IActionResult> UpdateRole(Guid id, UpdateEmployeeRoleRequest request)
    {
        if (!AllowedRoles.Contains(request.Role))
            return BadRequest($"Role must be one of: {string.Join(", ", AllowedRoles)}.");

        var user = await _userManager.Users
            .FirstOrDefaultAsync(u => u.Id == id && u.RestaurantId == RestaurantId);
        if (user == null) return NotFound();

        var currentRoles = await _userManager.GetRolesAsync(user);
        await _userManager.RemoveFromRolesAsync(user, currentRoles);

        if (!await _roleManager.RoleExistsAsync(request.Role))
            await _roleManager.CreateAsync(new IdentityRole<Guid>(request.Role));

        await _userManager.AddToRoleAsync(user, request.Role);
        return NoContent();
    }

    [HttpPut("{id}/deactivate")]
    public async Task<IActionResult> Deactivate(Guid id)
    {
        var user = await _userManager.Users
            .FirstOrDefaultAsync(u => u.Id == id && u.RestaurantId == RestaurantId);
        if (user == null) return NotFound();

        user.IsActive = false;
        await _userManager.UpdateAsync(user);
        return NoContent();
    }
}