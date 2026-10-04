using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace api.Models;

[Table("audit_logs")]
public class AuditLog
{
    public int id { get; set; }

    public int? user_id { get; set; }
    public User? user { get; set; }

    public int? branch_id { get; set; }
    public Branch? branch { get; set; }

    public string? action { get; set; }

    public string? resource { get; set; }

    public int? resource_id { get; set; }

    public string? description { get; set; }

    public string? ip_address { get; set; }

    public string? user_agent { get; set; }

    public string? device { get; set; }

    public DateTime created_at { get; set; }
}