using api.Data;
using api.Models;
using api.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using api.Services;
using Microsoft.AspNetCore.Authorization;


namespace api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,Supervisor")]

public class BranchesController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly AuditService _audit;

    public BranchesController(AppDbContext context, AuditService audit)
    {
        _context = context;
        _audit = audit;
    }

    [HttpGet]
    public async Task<IActionResult> GetBranches()
    {
        // We use a subquery to get services. No .Include() needed!
        var result = await _context.Branches
            .Select(b => new
            {
                id = b.id,
                branch_name = b.branch_name,
                town = b.town,
                address = b.address,
                total_counter = b.total_counter,
                active_counter = b.active_counter,
                status = b.status,
                created_at = b.created_at,

                // SUBQUERY: Get services for THIS specific branch
                services = _context.BranchServices
                    .Where(bs => bs.branch_id == b.id)
                    .Select(bs => new
                    {
                        id = bs.service.id,
                        service_name = bs.service.service_name,
                        ticket_prefix = bs.service.ticket_prefix,
                        description = bs.service.description,
                        status = bs.service.status
                    })
                    .ToList()
            })
            .ToListAsync();


        return Ok(result);
    }


    [HttpGet("{id}")]
    public async Task<IActionResult> GetBranch(int id)
    {
        var branch = await _context.Branches.FindAsync(id);

        if (branch == null)
        {
            return NotFound();
        }

        var branchDto = ToDto(branch);

        return Ok(branchDto);
    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> PatchBranch(int id, PatchBranchDto dto)
    {
        var branch = await _context.Branches.FindAsync(id);

        if (branch == null)
        {
            return NotFound();
        }
        if (dto.branch_name != null)
            branch.branch_name = dto.branch_name;
        if (dto.town != null)
            branch.town = dto.town;
        if (dto.address != null)
            branch.address = dto.address;
        if (dto.total_counter.HasValue)
            branch.total_counter = dto.total_counter.Value;
        if (dto.active_counter.HasValue)
            branch.active_counter = dto.active_counter.Value;
        if (dto.status != null)
            branch.status = dto.status.Value;

        await _context.SaveChangesAsync();

        await _audit.Log("Update", "Branch", branch.id, $"Updated branch {branch.branch_name}");

        var branchDto = ToDto(branch);
        return Ok(branchDto);

    }

    [HttpPost]

    public async Task<IActionResult> CreateBranch(CreateBranchDto dto)
    {
        var branch = new Branch
        {
            branch_name = dto.branch_name,
            town = dto.town,
            address = dto.address,
            total_counter = dto.total_counter,
            active_counter = dto.active_counter,
            status = dto.status,
            created_at = DateTime.UtcNow
        };
        _context.Branches.Add(branch);
        await _context.SaveChangesAsync();

        await _audit.Log("Create", "Branch", branch.id, $"Created branch {branch.branch_name}");

        var branchDto = ToDto(branch);
        return CreatedAtAction(
            nameof(GetBranch),
            new { id = branch.id },
            branchDto
            );
    }

    [HttpPut("{id}")]


    public async Task<IActionResult> UpdateBranch(int id, UpdateBranchDto dto)
    {
        var existingBranch = await _context.Branches.FindAsync(id);

        if (existingBranch == null)
        {
            return NotFound();
        }

        existingBranch.branch_name = dto.branch_name;
        existingBranch.town = dto.town;
        existingBranch.address = dto.address;
        existingBranch.total_counter = dto.total_counter;
        existingBranch.active_counter = dto.active_counter;
        existingBranch.status = dto.status;

        await _context.SaveChangesAsync();
        var branchDto = ToDto(existingBranch);
        //return Ok(existingBranch);
        return Ok(branchDto);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteBranch(int id)
    {
        var branch = await _context.Branches.FindAsync(id);

        if (branch == null)
        {
            return NotFound();

        }
        _context.Branches.Remove(branch);

        await _context.SaveChangesAsync();

        await _audit.Log("Delete", "Branch", branch.id, $"Deleted branch {branch.branch_name}");

        return NoContent();
    }

    private BranchDto ToDto(Branch branch)
    {
        return new BranchDto
        {
            id = branch.id,
            branch_name = branch.branch_name,
            town = branch.town,
            address = branch.address,
            total_counter = branch.total_counter,
            active_counter = branch.active_counter,
            status = branch.status,
            created_at = branch.created_at
        };
    }
}