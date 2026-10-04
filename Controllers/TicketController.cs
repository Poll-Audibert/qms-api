using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using api.Data;
using api.Models;

namespace api.Controllers;

[ApiController]

[Route("api/[controller]")]

public class TicketController : ControllerBase
{
    private readonly AppDbContext _context;


    public TicketController (AppDbContext context)
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

    private static string FormatTicketStatus(TicketStatus status)
    {
        return status switch
        {
            TicketStatus.Completed => "Completed",
            TicketStatus.Called => "Called",
            TicketStatus.Serving => "Serving",
            TicketStatus.Waiting => "Waiting",
            _ => status.ToString()
        };
    }


   

    [HttpGet("{id}")]
    public async Task<IActionResult> GetTicket(int id)
    {
        var ticket = await _context.Tickets
        .Include(t => t.service)
        .Include(t => t.branch)
        .Include(t => t.user)
        .FirstOrDefaultAsync(t => t.id == id);

        if (ticket == null)
        {
            return NotFound("The specified Ticket does not exist");
        }
        return Ok(ticket);
    }

   
    [HttpPost]
    public async Task<IActionResult> CreateTicket(Ticket ticket)
    {
        // 1. Validate service and branch
        if (ticket.service_id <= 0)
        {
            return BadRequest(new
            {
                success = false,
                message = "Service ID is required"
            });
        }

        if (ticket.branch_id <= 0)
        {
            return BadRequest(new
            {
                success = false,
                message = "Branch ID is required"
            });
        }


        // 2. Get service
        var service = await _context.Services
            .FirstOrDefaultAsync(s =>
                s.id == ticket.service_id &&
                s.status == ServiceStatus.Active);

        if (service == null)
        {
            return NotFound(new
            {
                success = false,
                message = "Service not found or inactive"
            });
        }


        // 3. Get branch
        var branch = await _context.Branches
            .FirstOrDefaultAsync(b => b.id == ticket.branch_id);

        if (branch == null)
        {
            return NotFound(new
            {
                success = false,
                message = "Branch not found"
            });
        }


        // 4. Get ticket prefix
        var ticketPrefix = service.ticket_prefix?.Trim().ToUpper();

        if (string.IsNullOrEmpty(ticketPrefix))
        {
            return StatusCode(500, new
            {
                success = false,
                message = "Service does not have a ticket prefix configured"
            });
        }


        // 5. Get today's tickets for this service and branch
        var today = DateTime.Today;
        var tomorrow = today.AddDays(1);

        var lastTicket = await _context.Tickets
            .Where(t =>
                t.service_id == ticket.service_id &&
                t.branch_id == ticket.branch_id &&
                t.created_at >= today &&
                t.created_at < tomorrow &&
                t.ticket_number.StartsWith(ticketPrefix + "-"))
            .OrderByDescending(t => t.ticket_number)
            .FirstOrDefaultAsync();


        // 6. Calculate next sequence
        int sequence = 1;

        if (lastTicket != null)
        {
            var dashPosition = lastTicket.ticket_number.LastIndexOf('-');

            if (dashPosition >= 0)
            {
                var numberPart = lastTicket.ticket_number[(dashPosition + 1)..];

                if (int.TryParse(numberPart, out int lastSequence))
                {
                    sequence = lastSequence + 1;
                }
            }
        }


        // 7. Generate ticket number
        var ticketNumber = $"{ticketPrefix}-{sequence:D3}";


        // 8. Set ticket values
        ticket.ticket_number = ticketNumber;
        ticket.status = TicketStatus.Waiting;
        ticket.created_at = DateTime.Now;


        // 9. Save ticket
        _context.Tickets.Add(ticket);

        await _context.SaveChangesAsync();


        // 10. Get current queue count
        var queueCount = await _context.Tickets
            .CountAsync(t =>
                t.service_id == ticket.service_id &&
                t.branch_id == ticket.branch_id &&
                t.created_at >= today &&
                t.created_at < tomorrow &&
                (t.status == TicketStatus.Waiting ||
                 t.status == TicketStatus.Called));


        // 11. Calculate estimated wait time
        var estimatedTimePerTicket = 5;

        var estimatedWaitTime =
            queueCount * estimatedTimePerTicket;


        // 12. Return response
        return Ok(new
        {
            success = true,
            message = "Ticket created successfully",

            data = new
            {
                ticket_number = ticket.ticket_number,

                service_id = ticket.service_id,

                service_name = service.service_name,

                code = ticketPrefix,

                branch_id = ticket.branch_id,

                branch_name = branch.branch_name,

                status = ticket.status,

                queue_count = queueCount,

                estimated_wait_time = estimatedWaitTime,

                phone_number = ticket.phone_number
            }
        });
    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> PatchTicket(int id, Ticket updatedTicket)
    {
        var ticket = await _context.Tickets.FindAsync(id);

        if (ticket == null)
        {
            return NotFound("The specified ticket does not exist");
        }

        ticket.status = updatedTicket.status;

        if (updatedTicket.teller_id.HasValue)
        {
            ticket.teller_id = updatedTicket.teller_id;
        }

        if (updatedTicket.status == TicketStatus.Called)
        {
            ticket.called_at = DateTime.Now;
        }

        if (updatedTicket.status == TicketStatus.Serving)
        {
            ticket.serving_at = DateTime.Now;
        }

        if (updatedTicket.status == TicketStatus.Completed)
        {
            ticket.completed_at = DateTime.Now;
        }

        await _context.SaveChangesAsync();

        return Ok(ticket);
    }


    

}


