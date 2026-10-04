using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace api.Models;

[Table("counters")]
public class Counter
{
    
    public int id { get; set; }

    [Range(1, int.MaxValue)]
    public int branch_id { get; set; }

    public Branch? branch { get; set; }

    [Required]
    [StringLength(100)]
    public string counter_name { get; set; } = string.Empty;

    public CounterStatus status { get; set; }

    public DateTime created_at { get; set; }
}
