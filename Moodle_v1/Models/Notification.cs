using Moodle_v1.Models;
using System.ComponentModel.DataAnnotations;

public class Notification
{
    [Key]
    public int Id { get; set; }
    public string StudentUserId { get; set; }
    public int AnnouncementId { get; set; }
    public bool IsRead { get; set; } = false;
    public DateTime CreatedAt { get; set; }

    public Announcement Announcement { get; set; }
}
