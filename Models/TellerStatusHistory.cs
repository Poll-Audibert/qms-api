using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using api.Models;

[Table("teller_status_history")]
public class TellerStatusHistory
{
    public int id { get; set; }

    [Required]
    public int user_id { get; set; }

    public User? user { get; set; }

    [Required]
    public TellerStatus status { get; set; }

    [Required]
    public DateTime changed_at { get; set; }
}