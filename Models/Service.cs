using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace api.Models;

public class Service
{
    public int id { get; set; }

    [Required]
    [StringLength(100)]

    public string service_name { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]

    public string ticket_prefix { get; set; } = string.Empty;

    public string? description { get; set; }

    public int priority { get; set; }

    public ServiceStatus status { get; set; }

    public DateTime created_at { get; set; }


}
