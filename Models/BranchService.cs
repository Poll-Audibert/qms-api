using System.ComponentModel.DataAnnotations;

using System.ComponentModel.DataAnnotations.Schema;

namespace api.Models;

    [Table("branch_services")]

    public class BranchService
{
    public int id { get; set; }

    public int branch_id { get; set; }

    public int service_id { get; set; }

    public Branch? branch { get; set; }

    public Service? service { get; set; }

    public DateTime created_at { get; set; }
}