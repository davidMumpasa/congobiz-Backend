using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CongoBiz.API.Controllers;

[Authorize]
[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    protected Guid RestaurantId =>
        Guid.Parse(User.FindFirst("restaurantId")!.Value);
}