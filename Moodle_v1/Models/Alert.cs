using System.ComponentModel.DataAnnotations;

public class Alert
{
    [Key]
    public int Id { get; set; }
    public string StudentUserId { get; set; }
    public string Message { get; set; }
    public bool IsRead { get; set; } = false;
    public DateTime CreatedAt { get; set; }
}
