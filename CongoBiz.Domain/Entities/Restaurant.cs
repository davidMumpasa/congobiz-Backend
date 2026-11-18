using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CongoBiz.Domain.Entities
{
    public class Restaurant
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Province { get; set; }
    public string? LogoUrl { get; set; }
    public string Currency { get; set; } = "ZAR";
    public string Language { get; set; } = "en";
    public string Slug { get; set; } = string.Empty; // used in QR menu URL
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public ICollection<MenuCategory> MenuCategories { get; set; } = new List<MenuCategory>();
}
}