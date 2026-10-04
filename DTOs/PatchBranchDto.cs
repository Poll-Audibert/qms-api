using api.Models;

namespace api.DTOs;

    public class PatchBranchDto
{
    public string? branch_name { get; set; }

    public string? town { get; set; }

    public string? address { get; set; }

    public int? total_counter { get; set; }

    public int? active_counter { get; set; }

    public BranchStatus? status { get; set; }       
}