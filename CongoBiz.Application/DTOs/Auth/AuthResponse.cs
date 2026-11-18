using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CongoBiz.Application.DTOs.Auth;

public class AuthResponse
{
    public string Token { get; set; } = string.Empty;
    public Guid RestaurantId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}