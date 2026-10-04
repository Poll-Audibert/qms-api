using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using api.Models;
using api.Data;

namespace api.Controllers;

[ApiController]

[Route("api/[controller]")]
public class DeviceController : ControllerBase
{
    private readonly AppDbContext _context;

    public DeviceController (AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task <IActionResult> GetDevices()
        {

            var devices = await _context.Devices
            .Include(d => d.branch)
            .ToListAsync();

            return Ok(devices);
        }
    [HttpGet("id")]
    public async Task<IActionResult> GetDevice(int id)
    {
        var device = await _context.Devices
            .Include(d => d.branch)
            .FirstOrDefaultAsync(d => d.id == id);

        if(device == null)
        {
            return NotFound();
        }

        return Ok(device);
    }

    [HttpPost]
    public async Task<IActionResult> CreateDevice(Device device)
    {
        if(string.IsNullOrEmpty(device.device_code) ||
            string.IsNullOrEmpty(device.device_name) ||
            string.IsNullOrEmpty(device.device_type))
            {
            return BadRequest("These fields are required");
            }

        var existingDevice = await _context.Devices
            .FirstOrDefaultAsync(d =>
            d.device_name == device.device_name &&
            d.device_code == device.device_code &&
            d.branch_id == device.branch_id);

        if(existingDevice != null)
        {
            return BadRequest("Device already exist");
        }

        var branch = await _context.Branches.FindAsync(device.branch_id);

            if(branch == null)
        {
            return BadRequest("The specified branch does not exist");
        }

        _context.Devices.Add(device);

        await _context.SaveChangesAsync();

        return Ok(device);
            
    }

    [HttpPut("{id}")]
    public async Task <IActionResult> updateCounter(int id, Device updatedDevice)
        {
        var device = await _context.Devices.FindAsync(id);

        if(device == null)
        {
            return BadRequest("The specified device does not exist");
        }

        var branch = await _context.Branches.FindAsync(updatedDevice.branch_id);
        if(branch == null)
        {
            return BadRequest("The specified branch does not exist");
        }

        if(string.IsNullOrEmpty(updatedDevice.device_name) ||
            string.IsNullOrEmpty(updatedDevice.device_code) ||
            string.IsNullOrEmpty(updatedDevice.device_type))
        {
            return BadRequest("These fields cannot be empty");
        }

        var existingDevice = await _context.Devices.FirstOrDefaultAsync(d =>
        d.device_name == updatedDevice.device_name &&
        d.device_code == updatedDevice.device_code &&
        d.branch_id == updatedDevice.branch_id);

        if(existingDevice != null)
        {
            return BadRequest("The specified names already exist");
        }

        device.device_name = updatedDevice.device_name;
        device.device_code = updatedDevice.device_code;
        device.device_type = updatedDevice.device_type;
        device.branch_id = updatedDevice.branch_id;
        device.counter_name = updatedDevice.counter_name;
        device.status = updatedDevice.status;

        await _context.SaveChangesAsync();

        return Ok(device);

    }
    [HttpPatch("{id}")]
    public async Task<IActionResult> PactchCounter(int id, Device updatedDevice)
    {
        var device = await _context.Devices.FindAsync(id);

        if (device == null)
        {
            return BadRequest("The specified device does not exist");
        }

        var branch = await _context.Branches.FindAsync(updatedDevice.branch_id);

        if (branch == null)
        {
            return BadRequest("The specified branch does not exist");
        }

        if (string.IsNullOrEmpty(updatedDevice.device_name) ||
            string.IsNullOrEmpty(updatedDevice.device_code) ||
            string.IsNullOrEmpty(updatedDevice.device_type))
        {
            return BadRequest("THese fields are required");
        }

        var existingDevice = await _context.Devices.FirstOrDefaultAsync(d =>
        d.device_name == updatedDevice.device_name &&
        d.device_code == updatedDevice.device_code &&
        d.counter_name == updatedDevice.counter_name &&
        d.branch_id == updatedDevice.branch_id);

        if (existingDevice != null)
        {
            return BadRequest("The specified names already exist in this branch");

        }
        device.device_name = updatedDevice.device_name;
        device.device_code = updatedDevice.device_name;
        device.device_type = updatedDevice.device_type;
        device.branch_id = updatedDevice.branch_id;
        device.counter_name = updatedDevice.counter_name;
        device.status = updatedDevice.status;

        await _context.SaveChangesAsync();

        return Ok(device);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteDevice(int id)
    {
        var device = await _context.Devices.FindAsync(id);

        if (device == null)
        {
            return BadRequest("The Specified Device doest not exist");
        }

        _context.Devices.Remove(device);

        await _context.SaveChangesAsync();

        return NoContent();
    }

}