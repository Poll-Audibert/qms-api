using System.ComponentModel.DataAnnotations;

using api.Models;

namespace api.DTOs;

public class UpdateBranchDto
{
    [Required]
    [StringLength(100)]
    public string branch_name { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string town { get; set; } = string.Empty;

    [StringLength(255)]
    public string? address { get; set; }

    [Range(1, 100)]
    public int total_counter { get; set; }

    [Range (0, 100)]
    public int active_counter { get; set; }


    public BranchStatus status { get; set; }
}