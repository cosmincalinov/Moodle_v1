using System.ComponentModel.DataAnnotations;
using Moodle_v1.Areas.Identity.Data;

public class Secretary
{
    [Key]
    public int Id { get; set; }
    public string? ApplicationUserId { get; set; }
    public ApplicationUser? ApplicationUser { get; set; }
}
