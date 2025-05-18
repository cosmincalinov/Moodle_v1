using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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
    private readonly AuthDbContext _context;
    public List<Course> courses { get; set; }
    public List<Student> students { get; set; }
    public List<Professor> professors { get; set; }
    public Course course { get; set; }

    public HomeController(ILogger<HomeController> logger,UserManager<ApplicationUser> userManager, AuthDbContext context)
    {
        _logger = logger;
        this._userManager = userManager;
        _context = context;
    }

    public IActionResult Index()
    {
        ViewData["UserID"] = _userManager.GetUserId(this.User);
        if(User.IsInRole("Student"))
        {
            courses = _context.Courses.ToList();
        }
        return View(courses);
    }

    public async Task<IActionResult> IndexSorted(string sortOrder)
    {
        ViewData["UserID"] = _userManager.GetUserId(this.User);

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var student = await _context.Students
            .Include(s => s.CoursesStudents)
            .ThenInclude(cs => cs.Course)
            .FirstOrDefaultAsync(s => s.ApplicationUserId == userId);

        if (student == null)
        {
            return NotFound();
        }

        var courses = student.CoursesStudents
            .Select(cs => cs.Course);

        courses = sortOrder == "desc"
            ? courses.OrderByDescending(c => c.Title)
            : courses.OrderBy(c => c.Title);

        ViewData["CurrentSort"] = sortOrder == "desc" ? "asc" : "desc"; // Toggle sort order

        return View("Index", courses.ToList());
    }

    public IActionResult CoursePage(int id)
    {
        course = _context.Courses.FirstOrDefault(c => c.Id == id);
        if (course == null)
        {
            return NotFound();
        }
        return View("CoursePage",course);
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AssignStudents()
    {
        var viewModel = new AssignStudentsViewModel
        {
            Courses = await _context.Courses.ToListAsync(),
            Students = await _context.Students.Include(s => s.ApplicationUser).ToListAsync()
        };

        return View(viewModel);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AssignStudents(AssignStudentsViewModel model)
    {
        if (ModelState.IsValid)
        {
            foreach (var studentId in model.SelectedStudentIds)
            {
                var exists = _context.CourseStudents.Any(cs => cs.StudentId == studentId && cs.CourseId == model.CourseId);
                if (!exists)
                {
                    _context.CourseStudents.Add(new CourseStudent
                    {
                        StudentId = studentId,
                        CourseId = model.CourseId
                    });
                }
            }
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        // Repopulate dropdowns if model state is invalid
        model.Courses = await _context.Courses.ToListAsync();
        model.Students = await _context.Students.Include(s => s.ApplicationUser).ToListAsync();
        return View(model);
    }


    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AssignTeachers()
    {
        var viewModel = new AssignTeachersViewModel
        {
            Courses = await _context.Courses.ToListAsync(),
            Professors = await _context.Professors.Include(s => s.ApplicationUser).ToListAsync()
        };

        return View(viewModel);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AssignTeachers(AssignTeachersViewModel model)
    {
        if (ModelState.IsValid)
        {
            var course = await _context.Courses.FindAsync(model.CourseId);
            if (course == null)
            {
                return NotFound();
            }

            course.MainId = model.MainProfessorId;
            course.AssistantId = model.AssistantProfessorId;

            // Optionally, update navigation properties if you need them in memory
            course.Main = model.MainProfessorId.HasValue
                ? await _context.Professors.FindAsync(model.MainProfessorId.Value)
                : null;
            course.Assistant = model.AssistantProfessorId.HasValue
                ? await _context.Professors.FindAsync(model.AssistantProfessorId.Value)
                : null;

            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        model.Courses = await _context.Courses.ToListAsync();
        model.Professors = await _context.Professors
            .Include(p => p.ApplicationUser)
            .ToListAsync();
        return View(model);
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
}
