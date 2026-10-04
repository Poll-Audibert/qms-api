using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using api.Models;

[Table("teller_sessions")]
public class TellerSession
{
    public int id { get; set; }
    
    [Range(1, int.MaxValue)]
    [Required]
    public int user_id { get; set; }
    public User? user { get; set; }
    public string? counter_name { get; set; }
    public TellerStatus status { get; set; }
    public DateTime? logged_in_at { get; set; }
    public DateTime? logged_out_at { get; set; }

}