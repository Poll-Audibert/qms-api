using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using api.Data;
using api.Models;

namespace api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CounterManagementController : ControllerBase
{
    private readonly AppDbContext _context;

    public CounterManagementController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Supervisor")]
    public async Task<IActionResult> GetCounters(
    [FromQuery] string branch_id = "all")
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
        var userBranchId = user.branch_id;

        int? targetBranchId = null;

        if (isSupervisor)
        {
            targetBranchId = userBranchId;
        }
        else if (branch_id != "all")
        {
            targetBranchId = int.Parse(branch_id);
        }

        var branchesQuery = _context.Branches.AsQueryable();

        if (isSupervisor)
        {
            branchesQuery = branchesQuery
                .Where(b => b.id == userBranchId);
        }

        var branches = await branchesQuery
            .OrderBy(b => b.branch_name)
            .Select(b => new
            {
                id = b.id,
                name = b.branch_name
            })
            .ToListAsync();

        var counterQuery = _context.Devices
    .Where(d =>
        (d.device_type == "teller_terminal" ||
         d.device_type == "counter_display") &&
        d.counter_name != null);

        if (targetBranchId.HasValue)
        {
            counterQuery = counterQuery
                .Where(d => d.branch_id == targetBranchId.Value);
        }

        var counters = await counterQuery
           .Select(d => new
           {
               counter_name = d.counter_name,

               terminal_device = _context.Devices
                   .Where(x =>
                       x.counter_name == d.counter_name &&
                       x.device_type == "teller_terminal")
                   .Select(x => x.device_code)
                   .FirstOrDefault(),

               display_device = _context.Devices
                   .Where(x =>
                       x.counter_name == d.counter_name &&
                       x.device_type == "counter_display")
                   .Select(x => x.device_code)
                   .FirstOrDefault(),

               teller = _context.TellerSessions
             .Where(ts =>
                 ts.counter_name == d.counter_name &&
                 (ts.status == TellerStatus.Available ||
                  ts.status == TellerStatus.Busy ||
                  ts.status == TellerStatus.Break))
             .Select(ts => new
             {
                 id = ts.user_id,
                 name = ts.user!.full_name,
                 status = ts.status
             })
             .FirstOrDefault(),

         currentTicket = _context.Tickets
             .Where(t =>
                 t.counter_name == d.counter_name &&
                 (t.status == TicketStatus.Called ||
                  t.status == TicketStatus.Serving) &&
                 t.created_at >= DateTime.Today &&
                 t.created_at < DateTime.Today.AddDays(1))
             .OrderByDescending(t => t.called_at)
             .Select(t => new
             {
                 ticket = t.ticket_number,
                 service = t.service!.service_name,
                 status = t.status,
                 serving_at = t.serving_at
             })
             .FirstOrDefault()
     })
     .OrderBy(c => c.counter_name)
     .ToListAsync();

        var counterData = counters.Select(c =>
        {
            var duration = "—";

            if (c.teller?.status == TellerStatus.Busy &&
                c.currentTicket?.serving_at != null)
            {
                var diff = DateTime.Now - c.currentTicket.serving_at.Value;

                duration = $"{(int)diff.TotalMinutes}m {diff.Seconds}s";
            }


            return new
            {
                counter_name = c.counter_name,
                terminal_device = c.terminal_device,
                display_device = c.display_device,

                teller = c.teller?.name,
                teller_id = c.teller?.id,

                state = c.teller?.status.ToString().ToLower() ?? "offline",

                ticket = c.currentTicket?.ticket,
                service = c.currentTicket?.service,
                ticket_status = c.currentTicket?.status.ToString().ToLower(),
                duration,

                services = c.teller == null
    ? new List<string>()
    : _context.UserServices
        .Where(us => us.user_id == c.teller.id)
        .Include(us => us.service)
        .Select(us => us.service!.service_name)
        .ToList()
            };
        }).ToList();
        return Ok(new
        {
            success = true,
            data = new
            {
                branches,
                counters = counterData
            }
        });
    }
  }