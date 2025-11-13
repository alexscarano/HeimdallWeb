using System.ComponentModel.DataAnnotations;

namespace HeimdallWeb.Models;

public class UserUsageModel
{
    [Key]
    public int user_usage_id { get; set; }
    public DateTime date { get; set; }
    public int request_counts { get; set; } = 0;
    public int  user_id { get; set; }

    // Navigation property
    public UserModel User { get; set; }
}
