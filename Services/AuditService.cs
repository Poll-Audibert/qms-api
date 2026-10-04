using System.Security.Claims;
using api.Data;
using api.Models;

namespace api.Services;

public class AuditService
{
    private readonly AppDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuditService(
        AppDbContext context,
        IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task Log(
        string action,
        string resource,
        int? resourceId,
        string description,
        int? branchId = null)
    {
        var httpContext = _httpContextAccessor.HttpContext;

        var userIdClaim = httpContext?
            .User
            .FindFirst(ClaimTypes.NameIdentifier)?
            .Value;

        int? userId = userIdClaim != null
            ? int.Parse(userIdClaim)
            : null;

        var log = new AuditLog
        {
            user_id = userId,
            branch_id = branchId,
            action = action,
            resource = resource,
            resource_id = resourceId,
            description = description,
            ip_address = httpContext?.Connection.RemoteIpAddress?.ToString(),
            user_agent = httpContext?.Request.Headers["User-Agent"].ToString(),
            device = "Web",
            created_at = DateTime.Now
        };

        _context.AuditLogs.Add(log);

        await _context.SaveChangesAsync();
    }
}