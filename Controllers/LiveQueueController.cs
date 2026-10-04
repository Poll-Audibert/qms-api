using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using api.Data;
using api.Models;

namespace api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,Supervisor")]

public class LiveQueueController : ControllerBase
{
    private readonly AppDbContext _context;

    public LiveQueueController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetLiveQueue(
    [FromQuery] string branch_id = "all",
    [FromQuery] string? service_name = null)
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
        var branchId = user.branch_id;
        var requestedBranchId = branch_id;

        int? targetBranchId = null;

        if (isSupervisor)
        {
            targetBranchId = branchId;
        }
        else if (requestedBranchId != "all")
        {
            targetBranchId = int.Parse(requestedBranchId);
        }

        var branchesQuery = _context.Branches.AsQueryable();

        if (isSupervisor)
        {
            branchesQuery = branchesQuery
                .Where(b => b.id == branchId);
        }

        var branches = await branchesQuery
            .OrderBy(b => b.branch_name)
            .Select(b => new
            {
                id = b.id,
                name = b.branch_name
            })
            .ToListAsync();

        var serviceQuery = _context.Tickets
    .Include(t => t.service)
    .Where(t =>
        t.created_at >= DateTime.Today &&
        t.created_at < DateTime.Today.AddDays(1) &&
        (t.status == TicketStatus.Waiting ||
         t.status == TicketStatus.Called ||
         t.status == TicketStatus.Serving));

        if (targetBranchId.HasValue)
        {
            serviceQuery = serviceQuery
                .Where(t => t.branch_id == targetBranchId.Value);
        }

        var serviceBreakdown = await serviceQuery
            .GroupBy(t => t.service!.service_name)
            .Select(g => new
            {
                label = g.Key,
                waiting = g.Count(t => t.status == TicketStatus.Waiting),
                serving = g.Count(t =>
                    t.status == TicketStatus.Called ||
                    t.status == TicketStatus.Serving)
            })
            .OrderByDescending(x => x.waiting)
            .ThenByDescending(x => x.serving)
            .ToListAsync();

        var ticketQuery = _context.Tickets
    .Include(t => t.service)
    .Where(t =>
        t.created_at >= DateTime.Today &&
        t.created_at < DateTime.Today.AddDays(1) &&
        (t.status == TicketStatus.Waiting ||
         t.status == TicketStatus.Called ||
         t.status == TicketStatus.Serving));

        if (targetBranchId.HasValue)
        {
            ticketQuery = ticketQuery
                .Where(t => t.branch_id == targetBranchId.Value);
        }

        if (!string.IsNullOrEmpty(service_name))
        {
            ticketQuery = ticketQuery
                .Where(t => t.service!.service_name == service_name);
        }

        var tickets = await ticketQuery
    .OrderBy(t => t.created_at)
    .Select(t => new
    {
        id = t.id,
        ticket_number = t.ticket_number,
        service = t.service!.service_name,
        status = t.status,
        created_at = t.created_at,
        called_at = t.called_at,
        counter_name = t.counter_name
    })
    .ToListAsync();

        var counterQuery = _context.Devices
           .Where(d =>
               d.device_type == "teller_terminal" ||
               d.device_type == "counter_display");

        if (targetBranchId.HasValue)
        {
            counterQuery = counterQuery
                .Where(d => d.branch_id == targetBranchId.Value);
        }

        var counters = await counterQuery
            .Select(d => new
            {
                counter_name = d.counter_name,
                device_name = d.device_name,
                device_type = d.device_type,

                teller = _context.TellerSessions
                    .Where(ts =>
                        ts.counter_name == d.counter_name &&
                        (ts.status == TellerStatus.Available ||
                         ts.status == TellerStatus.Busy ||
                         ts.status == TellerStatus.Break))
                    .Select(ts => new
                    {
                        status = ts.status,
                        name = ts.user!.full_name
                    })
                    .FirstOrDefault(),

                current_ticket = _context.Tickets
                    .Where(t =>
                        t.counter_name == d.counter_name &&
                        (t.status == TicketStatus.Called ||
                         t.status == TicketStatus.Serving) &&
                        t.created_at >= DateTime.Today &&
                        t.created_at < DateTime.Today.AddDays(1))
                    .OrderByDescending(t => t.called_at)
                    .Select(t => t.ticket_number)
                    .FirstOrDefault()
            })
            .OrderBy(c => c.counter_name)
            .ToListAsync();

        var counterData = counters
    .Select(c => new
    {
        counter_name = c.counter_name,
        device_name = c.device_name,
        device_type = c.device_type,
        teller_status = c.teller?.status.ToString() ?? "Offline",
        teller_name = c.teller?.name ?? "No Teller",
        current_ticket = c.current_ticket ?? "---"
    })
    .ToList();

        return Ok(new
        {
            success = true,
            data = new
            {
                branches,
                serviceBreakdown,
                tickets,
                counters = counterData
            }
        });
    }
}