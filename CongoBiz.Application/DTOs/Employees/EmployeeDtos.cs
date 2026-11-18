namespace CongoBiz.Application.DTOs.Employees;

public class EmployeeDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

public class InviteEmployeeRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string TemporaryPassword { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty; // Manager, Cashier, Waiter, Kitchen
}

public class UpdateEmployeeRoleRequest
{
    public string Role { get; set; } = string.Empty;
}