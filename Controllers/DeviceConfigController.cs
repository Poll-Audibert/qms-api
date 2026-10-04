using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using api.Data;
using api.Models;

namespace api.Controllers;

[ApiController]
[Route("api/[controller]")]

public class DeviceConfigController: ControllerBase
{
    private readonly AppDbContext _context;

    public DeviceConfigController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return BadRequest("The device code is required");
        }

        var device = await _context.Devices
            .Include(d => d.branch)
            .FirstOrDefaultAsync(d => d.device_code == code);

        if (device == null)
        {
            return NotFound("Device not Found");
        }

        if (device.status != DeviceStatus.Active)
        {
            return StatusCode(403, new
            {
                success = false,
                message = "The device has been disabled by the admin"
            });
        }

        return Ok(device);
    }

    [HttpPost]
public async Task<IActionResult> ConfigureDevice([FromBody] string code)
    {
        // Check device code
        if (string.IsNullOrWhiteSpace(code))
        {
            return BadRequest(new
            {
                success = false,
                message = "Device code is required."
            });
        }

        // Find device
        var device = await _context.Devices
            .Include(d => d.branch)
            .FirstOrDefaultAsync(d => d.device_code == code);

        // Device doesn't exist
        if (device == null)
        {
            return NotFound(new
            {
                success = false,
                message = "Device not found. Please verify the device code or contact your administrator."
            });
        }

        // Device is disabled
        if (device.status != DeviceStatus.Active)
        {
            return StatusCode(403, new
            {
                success = false,
                message = "This device has been disabled by the administrator. Please contact support."
            });
        }

        // Update last seen
        //device.LastSeenAt = DateTime.Now;

        //await _context.SaveChangesAsync();

        // Return device configuration
        //return Ok(new
        //{
        //    success = true,
        //    device = new
        //    {
        //        id = device.Id,
        //        device_code = device.device_code,
        //        device_name = device.device_name,
        //        device_type = device.device_type,
        //        branch_id = device.branch_id

        //    }
        //});

        return Ok(device);
    }

}