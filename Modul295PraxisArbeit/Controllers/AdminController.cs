using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize(Roles = "Admin")]  // ✅ Restrict access to Admins only
[ApiController]
[Route("api/admin")]
public class AdminController : ControllerBase
{
    [HttpGet("dashboard")]
    public IActionResult GetAdminDashboard()
    {
        return Ok("Welcome to the Admin Dashboard!");
    }
}
