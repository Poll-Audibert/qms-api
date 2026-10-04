using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace api.Models;

[Table("branches")]
public class Branch
{
    public int id { get; set; }

    [Required]
    [StringLength(100)]
    public string branch_name { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string town { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    public string address { get; set; } = string.Empty; 

    [Range(1, 100)]
    public int total_counter { get; set; }

    [Range(0, 100)]
    public int active_counter { get; set; }

    public BranchStatus status { get; set; }

    public DateTime created_at { get; set; }

    public ICollection<BranchService> BranchServices { get; set; } = new List<BranchService>();
}