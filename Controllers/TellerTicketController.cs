using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using api.Data;
using api.Models;
using Microsoft.AspNetCore.Authorization;
namespace api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Teller")]

public class TellerTicketController : ControllerBase
{
    private readonly AppDbContext _context;

    public TellerTicketController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost("call-next")]
    public async Task<IActionResult> CallNext()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userIdClaim == null)
        {
            return Unauthorized("User ID not found");
        }

        int userId = int.Parse(userIdClaim);

        var session = await _context.TellerSessions
            .FirstOrDefaultAsync(s =>
                s.user_id == userId &&
                s.status != TellerStatus.Offline);

        if (session == null)
        {
            return NotFound("No active teller session found");
        }

        var teller = await _context.Users
            .FirstOrDefaultAsync(u => u.id == userId);

        if (teller == null)
        {
            return NotFound("Teller not found");
        }

        var ticket = await _context.Tickets
            .Where(t =>
                t.branch_id == teller.branch_id &&
                t.status == TicketStatus.Waiting)
            .OrderBy(t => t.created_at)
            .FirstOrDefaultAsync();

        if (ticket == null)
        {
            return Ok(new
            {
                success = false,
                message = "No customers waiting in queue"
            });
        }

        ticket.status = TicketStatus.Called;
        ticket.counter_name = session.counter_name;
        ticket.teller_id = userId;
        ticket.called_at = DateTime.Now;

        session.status = TellerStatus.Busy;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            success = true,
            message = "Customer called",
            ticket = ticket.ticket_number
        });
    }

    [HttpPatch("serve")]
    public async Task<IActionResult> ServeTicket(int ticketId)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userIdClaim == null)
        {
            return Unauthorized("User ID not found");
        }

        int userId = int.Parse(userIdClaim);

        var ticket = await _context.Tickets
            .FirstOrDefaultAsync(t =>
                t.id == ticketId &&
                t.teller_id == userId &&
                t.status == TicketStatus.Called);

        if (ticket == null)
        {
            return NotFound("Called ticket not found");
        }

        ticket.status = TicketStatus.Serving;
        ticket.serving_at = DateTime.Now;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            success = true,
            message = "Serving started"
        });
    }

    [HttpPatch("recall")]
    public async Task<IActionResult> RecallTicket(int ticketId)
    {
        var ticket = await _context.Tickets
            .FirstOrDefaultAsync(t => t.id == ticketId);

        if (ticket == null)
        {
            return NotFound("Ticket not found");
        }

        ticket.called_at = DateTime.Now;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            success = true,
            message = "Recalled"
        });
    }

    [HttpPatch("complete")]
    public async Task<IActionResult> CompleteTicket(int ticketId)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userIdClaim == null)
        {
            return Unauthorized("User ID not found");
        }

        int userId = int.Parse(userIdClaim);

        var ticket = await _context.Tickets
            .FirstOrDefaultAsync(t =>
                t.id == ticketId &&
                t.teller_id == userId &&
                t.status == TicketStatus.Serving);

        if (ticket == null)
        {
            return NotFound("Serving ticket not found");
        }

        ticket.status = TicketStatus.Completed;
        ticket.completed_at = DateTime.Now;

        var session = await _context.TellerSessions
            .FirstOrDefaultAsync(s =>
                s.user_id == userId &&
                s.status != TellerStatus.Offline);

        if (session != null)
        {
            session.status = TellerStatus.Available;
        }

        await _context.SaveChangesAsync();

        return Ok(new
        {
            success = true,
            message = "Service completed"
        });
    }

    [HttpPatch("no-show")]
    public async Task<IActionResult> NoShowTicket(int ticketId)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userIdClaim == null)
        {
            return Unauthorized("User ID not found");
        }

        int userId = int.Parse(userIdClaim);

        var ticket = await _context.Tickets
            .FirstOrDefaultAsync(t =>
                t.id == ticketId &&
                t.teller_id == userId &&
                t.status == TicketStatus.Serving);

        if (ticket == null)
        {
            return NotFound("Serving ticket not found");
        }

        ticket.status = TicketStatus.NoShow;
        ticket.completed_at = DateTime.Now;

        var session = await _context.TellerSessions
            .FirstOrDefaultAsync(s =>
                s.user_id == userId &&
                s.status != TellerStatus.Offline);

        if (session != null)
        {
            session.status = TellerStatus.Available;
        }

        await _context.SaveChangesAsync();

        return Ok(new
        {
            success = true,
            message = "Marked as no-show"
        });
    }
    [HttpPatch("transfer")]
    public async Task<IActionResult> TransferTicket(int ticketId)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userIdClaim == null)
        {
            return Unauthorized("User ID not found");
        }

        int userId = int.Parse(userIdClaim);

        var ticket = await _context.Tickets
            .FirstOrDefaultAsync(t =>
                t.id == ticketId &&
                t.teller_id == userId &&
                t.status == TicketStatus.Serving);

        if (ticket == null)
        {
            return NotFound("Serving ticket not found");
        }

        ticket.status = TicketStatus.Transferred;
        ticket.completed_at = DateTime.Now;

        var session = await _context.TellerSessions
            .FirstOrDefaultAsync(s =>
                s.user_id == userId &&
                s.status != TellerStatus.Offline);

        if (session != null)
        {
            session.status = TellerStatus.Available;
        }

        await _context.SaveChangesAsync();

        return Ok(new
        {
            success = true,
            message = "Ticket transferred"
        });
    }

}