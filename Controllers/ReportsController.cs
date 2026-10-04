using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using api.Data;
using api.Models;

namespace api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly AppDbContext _context;

    public ReportsController(AppDbContext context)
    {
        _context = context;
    }



    [HttpGet]
    [Authorize(Roles = "Admin,Supervisor")]
    public async Task<IActionResult> GetReport(
        [FromQuery] string branch_id = "all",
        [FromQuery] string user_id = "all",
        [FromQuery] string service_id = "all",
        [FromQuery] DateTime? date_from = null,
        [FromQuery] DateTime? date_to = null)
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

        if (isSupervisor)
        {
            branch_id = user.branch_id!.Value.ToString();
        }

        var branchesQuery = _context.Branches.AsQueryable();

        if (isSupervisor)
        {
            branchesQuery = branchesQuery
                .Where(b => b.id == user.branch_id);
        }

        var branches = await branchesQuery
            .OrderBy(b => b.branch_name)
            .Select(b => new
            {
                id = b.id,
                name = b.branch_name
            })
            .ToListAsync();

        var tellersQuery = _context.Users
    .Where(u => u.role == UserRole.Teller);

        if (isSupervisor || branch_id != "all")
        {
            int targetBranchId = int.Parse(branch_id);

            tellersQuery = tellersQuery
                .Where(u => u.branch_id == targetBranchId);
        }

        var tellers = await tellersQuery
            .OrderBy(u => u.full_name)
            .Select(u => new
            {
                id = u.id,
                name = u.full_name
            })
            .ToListAsync();
        var services = await _context.Services
    .OrderBy(s => s.service_name)
    .Select(s => new
    {
        id = s.id,
        name = s.service_name
    })
    .ToListAsync();

        var ticketQuery = _context.Tickets.AsQueryable();

        if (branch_id != "all")
        {
            int selectedBranchId = int.Parse(branch_id);

            ticketQuery = ticketQuery
                .Where(t => t.branch_id == selectedBranchId);
        }

        if (user_id != "all")
        {
            int selectedUserId = int.Parse(user_id);

            ticketQuery = ticketQuery
                .Where(t => t.teller_id == selectedUserId);
        }

        if (service_id != "all")
        {
            int selectedServiceId = int.Parse(service_id);

            ticketQuery = ticketQuery
                .Where(t => t.service_id == selectedServiceId);
        }

        if (date_from.HasValue)
        {
            var from = date_from.Value.Date;

            ticketQuery = ticketQuery
                .Where(t => t.created_at >= from);
        }

        if (date_to.HasValue)
        {
            var to = date_to.Value.Date.AddDays(1);

            ticketQuery = ticketQuery
                .Where(t => t.created_at < to);
        }

        var tickets = await ticketQuery
    .Include(t => t.branch)
    .Include(t => t.user)
    .Include(t => t.service)
    .ToListAsync();

        var overview = tickets
     .GroupBy(t => new
     {
         Date = t.created_at.Date,
         BranchId = t.branch_id,
         BranchName = t.branch?.branch_name,
         TellerId = t.teller_id,
         TellerName = t.user?.full_name,
         ServiceId = t.service_id,
         ServiceName = t.service?.service_name
     })
     .Select(g => new
     {
         date = g.Key.Date,
         branchId = g.Key.BranchId,
         branchName = g.Key.BranchName,
         tellerId = g.Key.TellerId,
         tellerName = g.Key.TellerName,
         serviceId = g.Key.ServiceId,
         serviceName = g.Key.ServiceName,

         totalTickets = g.Count(),

         completedTickets = g.Count(t =>
             t.status == TicketStatus.Completed ||
             t.status == TicketStatus.NoShow ||
             t.status == TicketStatus.Transferred),

         avgWaitMinutes = g
             .Where(t =>
                 t.called_at != null &&
                 t.status == TicketStatus.Completed)
             .Select(t =>
                 (double?)(t.called_at!.Value - t.created_at).TotalMinutes)
             .Average(),

         avgServiceMinutes = g
             .Where(t =>
                 t.serving_at != null &&
                 t.completed_at != null &&
                 t.status == TicketStatus.Completed)
             .Select(t =>
                 (double?)(t.completed_at!.Value - t.serving_at!.Value).TotalMinutes)
             .Average()
     })
     .OrderByDescending(x => x.date)
     .ToList();



        return Ok(new
        {
            success = true,
            data = new
            {
                branches,
                tellers,
                services,
                overview
            }
        });
    }
}