using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using api.Data;

namespace api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuditLogController : ControllerBase
{
    private readonly AppDbContext _context;

    public AuditLogController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Supervisor")]
    public async Task<IActionResult> GetAuditLogs()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userIdClaim == null)
        {
            return Unauthorized("User ID not found");
        }

        int userId = int.Parse(userIdClaim);

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.id == userId);

        if (user == null)
        {
            return NotFound("User not found");
        }

        var isSupervisor = User.IsInRole("Supervisor");

        if (isSupervisor && user.branch_id == null)
        {
            return StatusCode(403, new
            {
                success = false,
                message = "Supervisor not assigned to a branch"
            });
        }

        var query = _context.AuditLogs
            .Include(a => a.user)
            .Include(a => a.branch)
            .AsQueryable();

        // Supervisor can only see logs from their branch
        if (isSupervisor)
        {
            query = query.Where(a => a.branch_id == user.branch_id);
        }

        var logs = await query
            .OrderByDescending(a => a.created_at)
            .Take(1000)
            .Select(a => new
            {
                id = a.id,
                userId = a.user_id,
                branchId = a.branch_id,
                action = a.action,
                resource = a.resource,
                resourceId = a.resource_id,
                description = a.description,
                ipAddress = a.ip_address,
                userAgent = a.user_agent,
                device = a.device,
                createdAt = a.created_at,
                userName = a.user != null
                    ? a.user.full_name
                    : "System",
                branchName = a.branch != null
                    ? a.branch.branch_name
                    : "System"
            })
            .ToListAsync();

        return Ok(new
        {
            success = true,
            data = logs
        });
    }
}