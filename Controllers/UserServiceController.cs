using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

using api.Models;
using api.Data;

namespace api.Contollers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,Supervisor")]

public class UserServiceController : ControllerBase
{
    private readonly AppDbContext _context;

    public UserServiceController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetUserService()
    {
        var userService = await _context.UserServices
            .Include(us => us.user)
            .Include(us => us.service)
            .ToListAsync();

        return Ok(userService);
    }

    [HttpPost]
    public async Task<IActionResult> CreateUserService(UserService userService)
    {
        var user = await _context.Users.FindAsync(userService.user_id);
        if (user == null)
        {
            return BadRequest();
        }

        var service = await _context.Services.FindAsync(userService.service_id);
        {
            if (service == null)
            {
                return BadRequest();
            }
        }

        var existing = await _context.UserServices
            .FirstOrDefaultAsync(us =>
            us.service_id == userService.service_id &&
            us.user_id == userService.user_id);


        if (existing != null)
        {
            return BadRequest("The user is already assisgned to the service");
        }
        _context.UserServices.Add(userService);

        await _context.SaveChangesAsync();

        return Ok(userService);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUserService(int id)
    {
        var userService = await _context.UserServices
            .Include(us => us.user)
            .Include(us => us.service)
            .FirstOrDefaultAsync(us => us.id == id);

        if (userService == null)
        {
            return NotFound();
        }

        return Ok(userService);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUserService(int id, UserService updatedUserService)
    {
        var userService = await _context.UserServices.FindAsync(id);

        if (userService == null)
        {
            return NotFound();
        }

        var user = await _context.Users.FindAsync(updatedUserService.user_id);

        if (user == null)
        {
            return BadRequest("The specified user does not exist.");
        }

        var service = await _context.Services.FindAsync(updatedUserService.service_id);

        if (service == null)
        {
            return BadRequest("The specified service does not exist.");
        }

        var existing = await _context.UserServices
            .FirstOrDefaultAsync(us =>
                us.user_id == updatedUserService.user_id &&
                us.service_id == updatedUserService.service_id &&
                us.id != id);

        if (existing != null)
        {
            return BadRequest("This user is already assigned to this service.");
        }

        userService.user_id = updatedUserService.user_id;
        userService.service_id = updatedUserService.service_id;

        await _context.SaveChangesAsync();

        var updated = await _context.UserServices
            .Include(us => us.user)
            .Include(us => us.service)
            .FirstOrDefaultAsync(us => us.id == id);

        return Ok(updated);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUserService(int id)
    {
        var userService = await _context.UserServices.FindAsync(id);

        if (userService == null)
        {
            return NotFound();
        }

        _context.UserServices.Remove(userService);

        await _context.SaveChangesAsync();

        return NoContent();
    }
}