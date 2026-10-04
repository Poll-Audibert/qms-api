using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using api.Data;
using api.DTOs;
using api.Models;
using api.Services;

namespace api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly AuditService _audit;


    public AuthController(AppDbContext context, IConfiguration configuration, AuditService audit)
    {
        _context = context;
        _configuration = configuration;
        _audit = audit;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto user)
    {
        var existingUser = await _context.Users
            .FirstOrDefaultAsync(u => u.email == user.email);

        if (existingUser == null)
        {
            return Unauthorized("Invalid email or password");
        }

        var passwordHasher = new PasswordHasher<User>();

        var passwordResult = passwordHasher.VerifyHashedPassword(
            existingUser,
            existingUser.password,
            user.password
        );

        if (passwordResult == PasswordVerificationResult.Failed)
        {
            return Unauthorized("Invalid email or password");
        }

        if (existingUser.status != UserStatus.Active)
        {
            return Unauthorized("User account is not active");
        }

        Device? device = null;

        if (existingUser.role == UserRole.Teller ||
            existingUser.role == UserRole.Viewer)
        {
            device = await _context.Devices
                .FirstOrDefaultAsync(d => d.device_code == user.device_code);

            if (device == null)
            {
                return BadRequest("This terminal has not yet been configured! Please contact admin.");
            }
        }

        if (existingUser.role == UserRole.Teller)
        {
            var session = new TellerSession
            {
                user_id = existingUser.id,
                counter_name = device!.counter_name,
                status = TellerStatus.Available,
                logged_in_at = DateTime.UtcNow
            };

            _context.TellerSessions.Add(session);

            var statusHistory = new TellerStatusHistory
            {
                user_id = existingUser.id,
                status = TellerStatus.Available
            };

            _context.TellerStatusHistories.Add(statusHistory);

            await _context.SaveChangesAsync();

           // await _audit.Log(
           //    "LOGIN",
           //    "TellerSession",
           //    existingUser.id,
           //    $"Teller {existingUser.full_name} logged in at {session.logged_in_at} on counter {device!.counter_name}",
           //    existingUser.branch_id
           //);
        }

        await _audit.Log(
    "LOGIN",
    "User",
    existingUser.id,
    $"{existingUser.role} {existingUser.full_name} logged in",
    existingUser.branch_id
);

        var claims = new[]
        {
        new Claim(ClaimTypes.NameIdentifier, existingUser.id.ToString()),
        new Claim(ClaimTypes.Name, existingUser.full_name),
        new Claim(ClaimTypes.Email, existingUser.email),
        new Claim(ClaimTypes.Role, existingUser.role.ToString()),
        new Claim("branch_id", existingUser.branch_id?.ToString() ?? "")
    };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!)
        );

        var credentials = new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256
        );

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: credentials
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        return Ok(new
        {
            message = "Login successful",
            token = tokenString,
            user = new
            {
                existingUser.id,
                existingUser.full_name,
                existingUser.email,
                existingUser.role,
                existingUser.branch_id,
                existingUser.status,
                device_code = device?.device_code,
                counter_name = device?.counter_name
            }
        });
    }
}