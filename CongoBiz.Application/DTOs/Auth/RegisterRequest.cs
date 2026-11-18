using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CongoBiz.Application.DTOs.Auth;

public class RegisterRequest
{
    public string RestaurantName { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}