using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using api.Models;
using api.Data;

namespace api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,Supervisor")]

public class  ServiceController: ControllerBase
{
    private readonly AppDbContext _context;

        public ServiceController (AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]

    public async Task<IActionResult> GetServices()
    {
        var services = await _context.Services.ToListAsync();

        return Ok(services);

    }

    [HttpGet("{id}")]

    public async Task<IActionResult> GetService(int id)
    {
        var service = await _context.Services.FindAsync(id);

        if(service == null)
        {
            return NotFound();
        }

        return Ok(service);
    }

    [HttpPost]

    public async Task<IActionResult> CreateService(Service service)
    {
        _context.Services.Add(service);

        await _context.SaveChangesAsync();

        return Ok(service);
    }

    [HttpPut("{id}")]

    public async Task<IActionResult> UpdateService(int id, Service updatedService)

    {
        var service = await _context.Services.FindAsync(id);

        if(service == null)
        {
            return NotFound();
        }

        service.service_name = updatedService.service_name;
        service.ticket_prefix = updatedService.ticket_prefix;
        service.priority = updatedService.priority;
        service.description = updatedService.description;
        service.status = updatedService.status;

        await _context.SaveChangesAsync();

        return Ok(service);
    }

    [HttpPatch("{id}")]

    public async Task<IActionResult> PatchService(int id, Service updatedService)

    {
        var service = await _context.Services.FindAsync(id);

        if (service == null)
        {
            return NotFound();
        }

        if(!string.IsNullOrEmpty(updatedService.service_name))
        {
            service.service_name = updatedService.service_name;
        }

        if(!string.IsNullOrEmpty(updatedService.ticket_prefix))
            {
            service.service_name = updatedService.service_name;
        }

        if(updatedService.description != null)

        {
            service.description = updatedService.description;
        }

        if(updatedService.priority != 0)
        {
            service.priority = updatedService.priority;
        }

        service.status = updatedService.status;

        await _context.SaveChangesAsync();

        return Ok(service);


    }

    [HttpDelete("{id}")]

    public async Task<IActionResult> DeleteService(int id)

    {
        var service = await _context.Services.FindAsync(id);

        if (service == null)
        {
            return NotFound();
        }

        _context.Services.Add(service);

        await _context.SaveChangesAsync();

        return Ok(service);
    }

}

