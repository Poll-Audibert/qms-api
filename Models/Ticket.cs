using System.ComponentModel.DataAnnotations;
using api.Models;

namespace api.Models;

public class Ticket
{
    public int id { get; set; }

    public string ticket_number { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int service_id { get; set; }

    public Service? service { get; set; }

    [Range(1, int.MaxValue)]
    [Required]
    public int branch_id { get; set; }

    public Branch? branch { get; set; }

    public int? teller_id { get; set; }

    public User? user { get; set; }

    public string? counter_name { get; set; }

    public TicketStatus status { get; set; }

    public string? phone_number { get; set; }

    public DateTime created_at { get; set; }

    public DateTime? called_at { get; set; }

    public DateTime? serving_at { get; set; }

    public DateTime? completed_at { get; set; }
}