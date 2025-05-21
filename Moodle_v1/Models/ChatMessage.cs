using System;
using System.ComponentModel.DataAnnotations;
using Moodle_v1.Areas.Identity.Data;

public class ChatMessage
{
    [Key]
    public int Id { get; set; }
    public string SenderId { get; set; }
    public string ReceiverId { get; set; }
    public string Message { get; set; }
    public DateTime SentAt { get; set; }

    public ApplicationUser Sender { get; set; }
    public ApplicationUser Receiver { get; set; }
}
