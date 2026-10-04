using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using api.Data;
using api.Models;

namespace api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
    private readonly AppDbContext _context;

    public AdminController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("dashboard")]
    [Authorize(Roles = "Admin,Supervisor")]
    public async Task<IActionResult> GetDashboard()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if(userIdClaim == null)
        {
            return Unauthorized("User ID not found");
        }

        int userId = int.Parse(userIdClaim);

        var user = await _context.Users
            .Include(u => u.branch)
            .FirstOrDefaultAsync(u =>u.id == userId);

        if(user == null)
        {
            return NotFound("User not found");
        }

        var today = DateTime.Today;
        var tomorrow = today.AddDays(1);

        var isSupervisor = User.IsInRole("Supervisor");
        var branchId = user.branch_id;

        var totalToday = await _context.Tickets
            .CountAsync(t =>
                t.created_at >= today &&
                t.created_at < tomorrow &&
                (!isSupervisor || t.branch_id == branchId));


        var currentlyWaiting = await _context.Tickets
            .CountAsync(t =>
                t.created_at >= today &&
                t.created_at < tomorrow &&
                t.status == TicketStatus.Waiting &&
                (!isSupervisor || t.branch_id == branchId));

        var beingServed = await _context.Tickets
            .CountAsync(t =>
                t.created_at >= today &&
                t.created_at < tomorrow &&
                (t.status == TicketStatus.Called ||
                 t.status == TicketStatus.Serving) &&
                (!isSupervisor || t.branch_id == branchId));

        var served = await _context.Tickets
            .CountAsync(t =>
                t.created_at >= today &&
                t.created_at < tomorrow &&
                t.status == TicketStatus.Completed &&
                (!isSupervisor || t.branch_id == branchId));

        var noShows = await _context.Tickets
            .CountAsync(t =>
                t.created_at >= today &&
                t.created_at < tomorrow &&
                t.status == TicketStatus.NoShow &&
                (!isSupervisor || t.branch_id == branchId));

        var waitTimes = await _context.Tickets
    .Where(t =>
        t.created_at >= today &&
        t.created_at < tomorrow &&
        t.status == TicketStatus.Completed &&
        t.called_at != null)
    .Select(t => new
    {
        t.created_at,
        t.called_at
    })
    .ToListAsync();

        double avgWait = 0;

        if (waitTimes.Count > 0)
        {
            avgWait = waitTimes
                .Average(t =>
                    (t.called_at!.Value - t.created_at).TotalMinutes);

            avgWait = Math.Round(avgWait);
        }
        var serviceTimes = await _context.Tickets
    .Where(t =>
        t.created_at >= today &&
        t.created_at < tomorrow &&
        t.status == TicketStatus.Completed &&
        t.serving_at != null &&
        t.completed_at != null)
    .Select(t => new
    {
        t.serving_at,
        t.completed_at
    })
    .ToListAsync();

        double avgService = 0;

        if (serviceTimes.Count > 0)
        {
            avgService = serviceTimes
                .Average(t =>
                    (t.completed_at!.Value - t.serving_at!.Value).TotalMinutes);

            avgService = Math.Round(avgService);
        }

        var serviceDistribution = await _context.Tickets
    .Where(t =>
        t.created_at >= today &&
        t.created_at < tomorrow &&
        (!isSupervisor || t.branch_id == branchId))
    .Include(t => t.service)
    .GroupBy(t => t.service!.service_name)
    .Select(g => new
    {
        label = g.Key,
        count = g.Count()
    })
    .ToListAsync();

        var serviceDistributionData = new
        {
            labels = serviceDistribution.Select(s => s.label).ToList(),
            data = serviceDistribution.Select(s => s.count).ToList()
        };

        var hourlyTickets = await _context.Tickets
    .Where(t =>
        t.created_at >= today &&
        t.created_at < tomorrow &&
        (!isSupervisor || t.branch_id == branchId))
    .Select(t => t.created_at.Hour)
    .ToListAsync();

        var hourlyTraffic = Enumerable
    .Range(8, 11)
    .Select(hour => new
    {
        label = $"{hour:D2}:00",
        count = hourlyTickets.Count(h => h == hour)
    })
    .ToList();

        var hourlyTrafficData = new
        {
            labels = hourlyTraffic.Select(h => h.label).ToList(),
            data = hourlyTraffic.Select(h => h.count).ToList()
        };

        var branches = await _context.Branches
    .Where(b => !isSupervisor || b.id == branchId)
    .ToListAsync();

        var branchPerformance = new List<object>();

        foreach (var branch in branches)
        {
            var branchTickets = await _context.Tickets
                .Where(t =>
                    t.branch_id == branch.id &&
                    t.created_at >= today &&
                    t.created_at < tomorrow)
                .ToListAsync();

            var waiting = branchTickets.Count(t =>
                t.status == TicketStatus.Waiting);

            var serving = branchTickets.Count(t =>
                t.status == TicketStatus.Called ||
                t.status == TicketStatus.Serving);

            var branchServed = branchTickets.Count(t =>
                t.status == TicketStatus.Completed);

            var branchWaitTimes = branchTickets
                .Where(t =>
                    t.status == TicketStatus.Completed &&
                    t.called_at != null)
                .Select(t =>
                    (t.called_at!.Value - t.created_at).TotalMinutes)
                .ToList();

            var branchAvgWait = branchWaitTimes.Count > 0
                ? Math.Round(branchWaitTimes.Average())
                : 0;

            branchPerformance.Add(new
            {
                id = branch.id,
                name = branch.branch_name,
                waiting,
                serving,
                served = branchServed,
                avgWait = branchAvgWait
            });
        }

        return Ok(new
        {
            success = true,

            data = new
            {
                kpis = new
                {
                    totalToday,
                    currentlyWaiting,
                    beingServed,
                    served,
                    avgWait,
                    avgService,
                    noShows
                },

                serviceDistribution = serviceDistributionData,
                hourlyTraffic = hourlyTrafficData,
                branches = branchPerformance
            }
        });
    }
}