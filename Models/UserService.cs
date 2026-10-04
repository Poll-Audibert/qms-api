using System.ComponentModel.DataAnnotations.Schema;


namespace api.Models;

    [Table("user_service")]

    public class UserService
{
    public int id { get; set; }

    public int user_id { get; set; }

    public int service_id { get; set; }
    
    public User? user { get; set; }

    public Service? service { get; set; }

    public DateTime created_at { get; set; }
}