using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace api.Models;
[Table("devices")]

public class Device
{
    public int id { get; set; }

    [Required]
    [StringLength(20)]
    public string device_code { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string device_name { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string device_type { get; set; } = string.Empty;

    public int? branch_id { get; set; }

    public Branch? branch { get; set; }

    public string? counter_name { get; set; }

    public DeviceStatus status { get; set; }

    public DateTime? last_seen_at { get; set; }

    public DateTime created_at { get; set; }

}

