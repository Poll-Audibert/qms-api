using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using api.Models;
using api.Data;

namespace api.Controllers;

[ApiController]
[Route("api/[controller]")]

public class TellerSessionController : ControllerBase
{
    private readonly AppDbContext _context;

    public TellerSessionController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetSession()
    {
        var session = await _context.TellerSessions
            .Include(ts => ts.user)
            .ToListAsync();

        return Ok(session);
    }

    [HttpPost]
    public async Task<IActionResult> CreateSession(TellerSession session)
    {
        _context.TellerSessions.Add(session);

        await _context.SaveChangesAsync();

        return Ok(session);
    }
}