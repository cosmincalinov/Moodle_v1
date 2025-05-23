using System.ComponentModel.DataAnnotations;

public class ChatNotification
{
    [Key]
    public int Id { get; set; }
    public string UserId { get; set; }
    public int ChatMessageId { get; set; }
    public ChatMessage ChatMessage { get; set; }
    public bool IsRead { get; set; } = false;
    public DateTime CreatedAt { get; set; }
}