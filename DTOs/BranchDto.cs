using api.Models;

namespace api.DTOs;

public class BranchDto
{
    public int id { get; set; }

    public string branch_name { get; set; } = string.Empty;

    public string town { get; set; } = string.Empty;

    public string? address { get; set; }

    public int total_counter { get; set; }

    public int active_counter { get; set; }

    public BranchStatus status { get; set; }

    public DateTime created_at { get; set; }
}