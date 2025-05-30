using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Moodle_v1.Areas.Identity.Data;
using Moodle_v1.Data;
using Moodle_v1.Models;

namespace Moodle_v1.Controllers;
[Authorize]
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly AuthDbContext _context;
    public List<Course> courses { get; set; }
    public List<Student> students { get; set; }
    public List<Professor> professors { get; set; }
    public Course course { get; set; }

    public HomeController(ILogger<HomeController> logger,UserManager<ApplicationUser> userManager, AuthDbContext context, RoleManager<IdentityRole> roleManager)
    {
        _logger = logger;
        this._userManager = userManager;
        _context = context;
        _roleManager = roleManager;
    }

    public IActionResult Index(string searchString, string sortOrder)
    {
        ViewData["UserID"] = _userManager.GetUserId(this.User);
        ViewBag.StudentId = _userManager.GetUserId(this.User);

        IQueryable<Course> courseQuery = _context.Courses
            .Include(c => c.Main).ThenInclude(p => p.ApplicationUser)
            .Include(c => c.Assistant).ThenInclude(p => p.ApplicationUser);

        if (User.IsInRole("Student"))
        {
            var userId = _userManager.GetUserId(this.User);

            var alerts = _context.Alerts
                .Where(a => a.StudentUserId == userId && !a.IsRead)
                .OrderByDescending(a => a.CreatedAt)
                .ToList();
            foreach (var alert in alerts)
            {
                alert.IsRead = true;
            }
            ViewBag.Alerts = alerts;

            var notifications = _context.Notifications
                .Include(n => n.Announcement)
                .Where(n => n.StudentUserId == userId && !n.IsRead)
                .OrderByDescending(n => n.CreatedAt)
                .ToList();
            foreach (var notification in notifications)
            {
                notification.IsRead = true;
            }
            ViewBag.Notifications = notifications;

            _context.SaveChanges();

            var student = _context.Students
                .Include(s => s.CoursesStudents)
                    .ThenInclude(cs => cs.Course)
                        .ThenInclude(c => c.Main).ThenInclude(p => p.ApplicationUser)
                .Include(s => s.CoursesStudents)
                    .ThenInclude(cs => cs.Course)
                        .ThenInclude(c => c.Assistant).ThenInclude(p => p.ApplicationUser)
                .FirstOrDefault(s => s.ApplicationUserId == userId);

            if (student != null)
            {
                courseQuery = student.CoursesStudents
                    .Select(cs => cs.Course)
                    .AsQueryable()
                    .Distinct();
            }
            else
            {
                courseQuery = Enumerable.Empty<Course>().AsQueryable();
            }
        }
        else if (User.IsInRole("Profesor"))
        {
            var userId = _userManager.GetUserId(this.User);
            var professor = _context.Professors.FirstOrDefault(p => p.ApplicationUserId == userId);
            if (professor != null)
            {
                courseQuery = courseQuery.Where(c => c.MainId == professor.Id || c.AssistantId == professor.Id);
            }
            else
            {
                courseQuery = Enumerable.Empty<Course>().AsQueryable();
            }
        }

        if (!string.IsNullOrWhiteSpace(searchString))
        {
            var loweredSearch = searchString.ToLower();

            courseQuery = courseQuery.Where(c =>
                c.Title.ToLower().Contains(loweredSearch) ||
                (c.Main != null && c.Main.ApplicationUser != null &&
                    (c.Main.ApplicationUser.FirstName + " " + c.Main.ApplicationUser.LastName).ToLower().Contains(loweredSearch)) ||
                (c.Assistant != null && c.Assistant.ApplicationUser != null &&
                    (c.Assistant.ApplicationUser.FirstName + " " + c.Assistant.ApplicationUser.LastName).ToLower().Contains(loweredSearch))
            );
        }

        ViewData["CurrentSort"] = sortOrder == "desc" ? "asc" : "desc";
        if (sortOrder == "desc")
            courseQuery = courseQuery.OrderByDescending(c => c.Title);
        else
            courseQuery = courseQuery.OrderBy(c => c.Title);

        courses = courseQuery
            .Include(c => c.Main).ThenInclude(p => p.ApplicationUser)
            .Include(c => c.Assistant).ThenInclude(p => p.ApplicationUser)
            .ToList();

        return View(courses);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    public IActionResult CoursePage(int id)
    {
        var course = _context.Courses
            .Include(c => c.Main)
            .Include(c => c.Assistant)
            .Include(c => c.Announcements)
                .ThenInclude(a => a.Professor)
                    .ThenInclude(p => p.ApplicationUser)
            .FirstOrDefault(c => c.Id == id);

        if (course == null)
        {
            return NotFound();
        }

        if (User.IsInRole("Student"))
        {
            var userId = _userManager.GetUserId(User);
            var student = _context.Students.FirstOrDefault(s => s.ApplicationUserId == userId);
            if (student != null)
            {
                var courseStudent = _context.CourseStudents
                    .FirstOrDefault(cs => cs.CourseId == id && cs.StudentId == student.NrMatricol);

                ViewBag.Grade = courseStudent?.Grade;
            }
        }

        return View("CoursePage", course);
    }
}
