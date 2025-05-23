using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moodle_v1.Areas.Identity.Data;
using Moodle_v1.Data;

[Authorize(Roles = "Profesor,Secretar")]
public class ChatController : Controller
{
    private readonly AuthDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public ChatController(AuthDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index(string withUserId)
    {
        var currentUserId = _userManager.GetUserId(User);

        var users = await _userManager.Users
            .Where(u => (User.IsInRole("Profesor") && _context.Secretaries.Any(s => s.ApplicationUserId == u.Id)) ||
                        (User.IsInRole("Secretar") && _context.Professors.Any(p => p.ApplicationUserId == u.Id)))
            .ToListAsync();
        ViewBag.Users = users;
        ViewBag.WithUserId = withUserId;

        var messages = string.IsNullOrEmpty(withUserId)
            ? new List<ChatMessage>()
            : await _context.ChatMessages
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .Where(m =>
                    (m.SenderId == currentUserId && m.ReceiverId == withUserId) ||
                    (m.SenderId == withUserId && m.ReceiverId == currentUserId))
                .OrderBy(m => m.SentAt)
                .ToListAsync();

        return View(messages);
    }

    [HttpPost]
    [Authorize(Roles = "Profesor,Secretar")]
    public async Task<IActionResult> Send(string withUserId, string message)
    {
        var currentUserId = _userManager.GetUserId(User);
        if (!string.IsNullOrWhiteSpace(message) && !string.IsNullOrWhiteSpace(withUserId))
        {
            var chatMessage = new ChatMessage
            {
                SenderId = currentUserId,
                ReceiverId = withUserId,
                Message = message,
                SentAt = DateTime.Now
            };
            _context.ChatMessages.Add(chatMessage);
            await _context.SaveChangesAsync();

            var chatNotification = new ChatNotification
            {
                UserId = withUserId,
                ChatMessageId = chatMessage.Id,
                IsRead = false,
                CreatedAt = DateTime.Now
            };
            _context.ChatNotifications.Add(chatNotification);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction("Index", new { withUserId });
    }
}
