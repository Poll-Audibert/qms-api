using Microsoft.AspNetCore.Mvc;

using Microsoft.EntityFrameworkCore;

using api.Models;

using api.Data;

namespace api.Controllers;

[ApiController]
[Route("api/[controller]")]

public class BranchServiceController: ControllerBase

{
    private readonly AppDbContext _context;

    public BranchServiceController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]

    public async Task<IActionResult> GetBranchServices()
    {
        var branchServices = await _context.BranchServices
            .Include(bs => bs.branch)
            .Include(bs => bs.service)
            .ToListAsync();

        return Ok(branchServices);
    }

    [HttpGet("{id}")]

    public async Task<IActionResult> GetBranchService(int id)

    {
        var brancService = await _context.BranchServices
            .Include(bs => bs.branch)
            .Include(bs => bs.service)
            .FirstOrDefaultAsync(bs => bs.id == id);

        return Ok(brancService);
    }

    [HttpPost]

    public async Task<IActionResult> CreateBranchService(BranchService branchService)
    {
        var branch = await _context.Branches.FindAsync(branchService.branch_id);

        if (branch == null)
        {
            return BadRequest("The sepecified branc does not exist");
        }

        var service = await _context.Services.FindAsync(branchService.service_id);

        if(service == null)
        {
            return BadRequest("THe specified service does not exist");
        }

        _context.BranchServices.Add(branchService);

        await _context.SaveChangesAsync();

        return Ok(branchService);
    }

    [HttpPut("{id}")]

    public async Task<IActionResult> UpdateBranchService(int id, BranchService updatedBranchService)
    {
        var branchService = await _context.BranchServices.FindAsync(id);

        if (branchService == null)
        {
            return NotFound();
        }

        var branch = await _context.Branches.FindAsync(id);

        if (branch == null)
        {
            return BadRequest("The specified branch does not exist");
        }

        var service = await _context.Services.FindAsync(id);

        if (service == null)
        {
            return BadRequest("The specified service does not exist");
        }

        branchService.branch_id = updatedBranchService.branch_id;

        branchService.service_id = updatedBranchService.service_id;



        await _context.SaveChangesAsync();

        var updated = await _context.BranchServices
            .Include(bs => bs.branch)
            .Include(bs => bs.service)
            .FirstOrDefaultAsync(bs => bs.id == id);



        return Ok(updated);
    }

    [HttpPatch("{id}")]

    public async Task<IActionResult> PatchBranchService(int id, BranchService updatedBranchService)
    {
        var branchService = await _context.BranchServices.FindAsync(id);

        if(branchService == null)
        {
            return NotFound();
        }

        if(updatedBranchService.service_id != 0)
        {
            var service = await _context.Services.FindAsync(updatedBranchService.service_id);

            if(service == null)
            {
                return BadRequest("The sepecified Branch does not exist");
            }

            branchService.service_id = updatedBranchService.service_id;
        }

        if(updatedBranchService.branch_id != 0)
        {
            var branch = await _context.Branches.FindAsync(updatedBranchService.branch_id);

            if(branch == null)
            {
                return BadRequest("The specified Branch does not exist");
            }

            branchService.branch_id = updatedBranchService.branch_id;

           
        }
        await _context.SaveChangesAsync();

        var updated = await _context.BranchServices
            .Include(bs => bs.branch)
            .Include(bs => bs.service)
            .FirstOrDefaultAsync(bs => bs.id == id);

        return Ok(updated);
    }

    [HttpDelete("{id}")]

    public async Task<IActionResult> DeleteBranchService(int id)

    {
        var branchService = await _context.BranchServices.FindAsync(id);

        if (branchService == null)

        {
            return NotFound();
        }

        _context.BranchServices.Remove(branchService);

        await _context.SaveChangesAsync();

        return NoContent();
    }
}