using CongoBiz.Application.DTOs.Auth;
using CongoBiz.Domain.Entities;
using CongoBiz.Infrastructure.Data;
using CongoBiz.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CongoBiz.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;
    private readonly AppDbContext _db;
    private readonly TokenService _tokenService;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole<Guid>> roleManager,
        AppDbContext db,
        TokenService tokenService)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _db = db;
        _tokenService = tokenService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request)
    {
        var existing = await _userManager.FindByEmailAsync(request.Email);
        if (existing != null)
            return BadRequest("An account with this email already exists.");

        var slug = request.RestaurantName
            .ToLower()
            .Replace(" ", "-")
            .Replace("'", "");

        var restaurant = new Restaurant
        {
            Name = request.RestaurantName,
            Slug = $"{slug}-{Guid.NewGuid().ToString()[..6]}"
        };
        _db.Restaurants.Add(restaurant);
        await _db.SaveChangesAsync();

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            RestaurantId = restaurant.Id
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            return BadRequest(result.Errors.Select(e => e.Description));

        if (!await _roleManager.RoleExistsAsync("Owner"))
            await _roleManager.CreateAsync(new IdentityRole<Guid>("Owner"));

        await _userManager.AddToRoleAsync(user, "Owner");

        var token = _tokenService.GenerateToken(user, new List<string> { "Owner" });

        return Ok(new AuthResponse
        {
            Token = token,
            RestaurantId = restaurant.Id,
            Email = user.Email!,
            Role = "Owner"
        });
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password))
            return Unauthorized("Invalid email or password.");

        var roles = await _userManager.GetRolesAsync(user);
        var token = _tokenService.GenerateToken(user, roles);

        return Ok(new AuthResponse
        {
            Token = token,
            RestaurantId = user.RestaurantId,
            Email = user.Email!,
            Role = roles.FirstOrDefault() ?? string.Empty
        });
    }
}