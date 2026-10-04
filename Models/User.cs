using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace api.Models;

[Table("users")]

public class User
{
    public int id { get; set; }

    [Required]
    [StringLength(150)]
    public string full_name { get; set; } = string.Empty;

    [Required]
    [StringLength(150)]
    public string email { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    public string password { get; set; } = string.Empty;

    [StringLength(20)]
    public string? counter { get; set; }

    public UserRole role { get; set; }

    public int? branch_id { get; set; }

    public Branch? branch { get; set; }

    public UserStatus status { get; set; }

    public DateTime created_at { get; set; }
}
