using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using api.Models;
using api.Data;

namespace api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CounterController : ControllerBase
{
    private readonly AppDbContext _context;

    public CounterController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/Counter
    [HttpGet]
    public async Task<IActionResult> GetCounters()
    {
        //var counters = await _context.Counters.ToListAsync();
        var counters = await _context.Counters
            .Include(c => c.branch)
            .ToListAsync();

        return Ok(counters);
    }

    // GET: api/Counter/1
    [HttpGet("{id}")]
    public async Task<IActionResult> GetCounter(int id)
    {
        //var counter = await _context.Counters.FindAsync(id);

        var counter =  await _context.Counters
            .Include(c => c.branch)
            .FirstOrDefaultAsync(c => c.id == id);

        if (counter == null)
        {
            return NotFound();
        }

        return Ok(counter);
    }

    // POST: api/Counter
    [HttpPost]
    public async Task<IActionResult> CreateCounter(Counter counter)
    {
        var branch = await _context.Branches.FindAsync(counter.branch_id);

            if (branch == null)
        {
            return BadRequest("The scpecific branch does not exist");
        }

        var existingCounter = await _context.Counters.FirstOrDefaultAsync(c =>
        c.counter_name == counter.counter_name &&
        c.branch_id == counter.branch_id);

        if(existingCounter != null)
        {
            return BadRequest("The specified branch already exist");
        }
     
        _context.Counters.Add(counter);

        await _context.SaveChangesAsync();

        var createdCounter = await _context.Counters
    .Include(c => c.branch)
    .FirstOrDefaultAsync(c => c.id == counter.id);

        return Ok(createdCounter);

        //return Ok(counter);
    }

    // PUT: api/Counter/1
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCounter(int id, Counter updatedCounter)
    {
        var counter = await _context.Counters.FindAsync(id);

        if (counter == null)
        {
            return NotFound();
        }

        var branch = await _context.Branches.FindAsync(updatedCounter.branch_id);

        if(branch == null)
        {
            return BadRequest("The specific branch does not exist");
        }

        counter.branch_id = updatedCounter.branch_id;
        counter.counter_name = updatedCounter.counter_name;
        counter.status = updatedCounter.status;

        await _context.SaveChangesAsync();

        return Ok(counter);
    }

    // PATCH: api/Counter/1
    [HttpPatch("{id}")]
    public async Task<IActionResult> PatchCounter(int id, Counter updatedCounter)
    {
        var counter = await _context.Counters.FindAsync(id);

        if (counter == null)
        {
            return NotFound();
        }

        if (updatedCounter.branch_id != 0)
        {
            var branch = await _context.Counters.FindAsync(updatedCounter.branch_id);

            if(branch == null)
            {
                return BadRequest("The specific branch does not exist");
            }
            counter.branch_id = updatedCounter.branch_id;
        }

        if (!string.IsNullOrEmpty(updatedCounter.counter_name))
        {
            counter.counter_name = updatedCounter.counter_name;
        }

        counter.status = updatedCounter.status;

        await _context.SaveChangesAsync();

        var updated = await _context.Counters
            .Include(c => c.branch)
            .FirstOrDefaultAsync(c => c.id == id);

        return Ok(updated);

        //return Ok(counter);
    }

    [HttpDelete("{id}")]

    public async Task<IActionResult> DeleteCounter(int id)
    {
        var counter = await _context.Counters.FindAsync(id);

        if (counter  == null)
        {
            return NotFound();
        }

        _context.Counters.Remove(counter);

        await _context.SaveChangesAsync();

        return NoContent();
    }
}

