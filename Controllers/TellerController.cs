using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using api.Data;
using api.Models;

namespace api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Teller")]

public class TellerController : ControllerBase
{
    private readonly AppDbContext _context;

    public TellerController(AppDbContext context)
    {
        _context = context;
    }
    private static string FormatDuration(double seconds)
    {
        var totalSeconds = (int)Math.Round(seconds);
        var minutes = totalSeconds / 60;
        var remainingSeconds = totalSeconds % 60;

        return $"{minutes}m {remainingSeconds}s";
    }


    [HttpGet("dashboard")]
    public async Task<IActionResult> GetTellerDashboard()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null)
        {
            return Unauthorized("User ID not found");
        }

        int userId = int.Parse(userIdClaim);

        var teller = await _context.Users
            .Include(u => u.branch)
            .FirstOrDefaultAsync(u => u.id == userId);

        if (teller == null)
        {
            return NotFound("Teller not found");
        }
        var session = await _context.TellerSessions
            .FirstOrDefaultAsync(s => s.user_id == userId);

        if (session == null)
        {
            return NotFound("No active teller session found");
        }

        var queue = await _context.Tickets
            .Include(t => t.service)
            .Where(t =>
                t.branch_id == teller.branch_id &&
                t.status == TicketStatus.Waiting)
            .OrderBy(t => t.created_at)
            .Take(20)
            .ToListAsync();

        var today = DateTime.Today;
        var tomorrow = today.AddDays(1);

        var history = await _context.Tickets
            .Where(t =>
                t.teller_id == userId &&
                t.completed_at >= today &&
                t.completed_at < tomorrow &&
                (t.status == TicketStatus.Completed ||
                 t.status == TicketStatus.NoShow ||
                 t.status == TicketStatus.Transferred))
            .OrderByDescending(t => t.completed_at)
            .Take(20)
            .ToListAsync();

        var servedToday = await _context.Tickets
    .CountAsync(t =>
        t.teller_id == userId &&
        t.completed_at >= today &&
        t.completed_at < tomorrow &&
        t.status == TicketStatus.Completed);

        var serviceTimes = await _context.Tickets
    .Where(t =>
        t.teller_id == userId &&
        t.serving_at != null &&
        t.completed_at != null &&
        t.completed_at >= today &&
        t.completed_at < tomorrow)
    .Select(t => new
    {
        serving_at = t.serving_at!.Value,
        completed_at = t.completed_at!.Value
    })
    .ToListAsync();

        double averageServiceSeconds = 0;

        if (serviceTimes.Count > 0)
        {
            averageServiceSeconds = serviceTimes
                .Average(t => (t.completed_at - t.serving_at).TotalSeconds);
        }

        var avgServiceTime = FormatDuration(averageServiceSeconds);

        var waitTimes = await _context.Tickets
    .Where(t =>
        t.teller_id == userId &&
        t.created_at >= today &&
        t.created_at < tomorrow &&
        t.called_at != null)
    .Select(t => new
    {
        created_at = t.created_at,
        called_at = t.called_at!.Value
    })
    .ToListAsync();


        double averageWaitSeconds = 0;

        if (waitTimes.Count > 0)
        {
            averageWaitSeconds = waitTimes
                .Average(t => (t.called_at - t.created_at).TotalSeconds);
        }

        var avgWaitTime = FormatDuration(averageWaitSeconds);

        double availableSeconds = 0;

        if (session.logged_in_at != null)
        {
            var endTime = session.logged_out_at ?? DateTime.UtcNow;

            availableSeconds =
                (endTime - session.logged_in_at.Value).TotalSeconds;
        }

        var availableTime = FormatDuration(availableSeconds);

        var servingTickets = await _context.Tickets
    .Where(t =>
        t.teller_id == userId &&
        t.serving_at != null &&
        t.created_at >= today &&
        t.created_at < tomorrow)
    .Select(t => new
    {
        serving_at = t.serving_at!.Value,
        completed_at = t.completed_at
    })
    .ToListAsync();

        double servingSeconds = 0;

        foreach (var ticket in servingTickets)
        {
            var endTime = ticket.completed_at ?? DateTime.Now;

            servingSeconds +=
                (endTime - ticket.serving_at).TotalSeconds;
        }

        var servingTime = FormatDuration(servingSeconds);

        double breakSeconds = 0;

        var statusHistory = await _context.TellerStatusHistories
            .Where(h =>
                h.user_id == userId &&
                h.changed_at >= today &&
                h.changed_at < tomorrow)
            .OrderBy(h => h.changed_at)
            .ToListAsync();

        foreach (var historyItem in statusHistory)
        {
            if (historyItem.status == TellerStatus.Break)
            {
                var nextHistory = statusHistory
                    .FirstOrDefault(h => h.changed_at > historyItem.changed_at);

                var endTime = nextHistory?.changed_at ?? DateTime.Now;

                breakSeconds +=
                    (endTime - historyItem.changed_at).TotalSeconds;
            }
        }

        var breakTime = FormatDuration(breakSeconds);

        double offlineSeconds = 0;

        foreach (var historyItem in statusHistory)
        {
            if (historyItem.status == TellerStatus.Offline)
            {
                var nextHistory = statusHistory
                    .FirstOrDefault(h => h.changed_at > historyItem.changed_at);

                var endTime = nextHistory?.changed_at ?? DateTime.Now;

                offlineSeconds +=
                    (endTime - historyItem.changed_at).TotalSeconds;
            }
        }

        var offlineTime = FormatDuration(offlineSeconds);

        var currentTicket = await _context.Tickets
    .Include(t => t.service)
    .FirstOrDefaultAsync(t =>
        t.teller_id == userId &&
        (t.status == TicketStatus.Called ||
         t.status == TicketStatus.Serving));

        return Ok(new
        {
            success = true,
            user_id = userId,
            full_name = teller.full_name,
            branch_id = teller.branch_id,
            branch_name = teller.branch?.branch_name,
            counter_name = session.counter_name,
            status = session.status,
            current = currentTicket,
            queue = queue,
            history = history,
            servedToday = servedToday,
            avgServiceTime = avgServiceTime,
            avgWaitTime = avgWaitTime,
            timeMetrics = new
            {
                available = availableTime,
                serving = servingTime,
                @break = breakTime,
                offline = offlineTime
            }
        });
    }

    [HttpPatch("status")]
    public async Task<IActionResult> ChangeTellerStatus([FromBody] TellerStatus newStatus)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userIdClaim == null)
        {
            return Unauthorized("User ID not found");
        }

        int userId = int.Parse(userIdClaim);

        Console.WriteLine($"NEW STATUS RECEIVED: {newStatus}");

        var session = await _context.TellerSessions
            .FirstOrDefaultAsync(s => s.user_id == userId);

        if (session == null)
        {
            return NotFound("No active teller session found");
        }

        Console.WriteLine($"CURRENT SESSION STATUS: {session.status}");

        session.status = newStatus;

        Console.WriteLine($"NEW SESSION STATUS: {session.status}");

        var statusHistory = new TellerStatusHistory
        {
            user_id = userId,
            status = newStatus
        };

        _context.TellerStatusHistories.Add(statusHistory);

        await _context.SaveChangesAsync();

        return Ok(new
        {
            success = true,
            status = session.status
        });
    }

}
