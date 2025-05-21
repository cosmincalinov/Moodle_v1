using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moodle_v1.Areas.Identity.Data;
using Moodle_v1.Data;
using Moodle_v1.Models;

namespace Moodle_v1.Controllers
{
    [Authorize(Roles = "Profesor")]
    public class ProfessorController : Controller
    {

        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly AuthDbContext _context;
        public List<Course> courses { get; set; }
        public List<Student> students { get; set; }
        public List<Professor> professors { get; set; }
        public Course course { get; set; }

        public ProfessorController(ILogger<HomeController> logger, UserManager<ApplicationUser> userManager, AuthDbContext context, RoleManager<IdentityRole> roleManager)
        {
            _logger = logger;
            this._userManager = userManager;
            _context = context;
            _roleManager = roleManager;
        }

        [Authorize(Roles = "Profesor")]
        public async Task<IActionResult> GradeStudents(int courseId)
        {
            var userId = _userManager.GetUserId(User);
            var professor = await _context.Professors.FirstOrDefaultAsync(p => p.ApplicationUserId == userId);

            var course = await _context.Courses
                .Include(c => c.CoursesStudents)
                .ThenInclude(cs => cs.Student)
                .ThenInclude(s => s.ApplicationUser)
                .FirstOrDefaultAsync(c => c.Id == courseId && c.MainId == professor.Id);

            if (course == null)
                return Forbid();

            var viewModel = new GradeStudentViewModel
            {
                CourseId = course.Id,
                CourseTitle = course.Title,
                Students = course.CoursesStudents.Select(cs => new StudentGradeEntry
                {
                    StudentId = cs.StudentId,
                    StudentName = cs.Student.ApplicationUser.FirstName + " " + cs.Student.ApplicationUser.LastName,
                    Grade = cs.Grade
                }).ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        [Authorize(Roles = "Profesor")]
        public async Task<IActionResult> GradeStudents(GradeStudentViewModel model)
        {
            var userId = _userManager.GetUserId(User);
            var professor = await _context.Professors.FirstOrDefaultAsync(p => p.ApplicationUserId == userId);

            var course = await _context.Courses.FirstOrDefaultAsync(c => c.Id == model.CourseId && c.MainId == professor.Id);
            if (course == null)
                return Forbid();

            foreach (var entry in model.Students)
            {
                var courseStudent = await _context.CourseStudents
                    .FirstOrDefaultAsync(cs => cs.CourseId == model.CourseId && cs.StudentId == entry.StudentId);

                if (courseStudent != null)
                {
                    courseStudent.Grade = entry.Grade;
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Home");
        }

        [Authorize(Roles = "Profesor")]
        public async Task<IActionResult> CreateAnnouncement(int courseId)
        {
            var userId = _userManager.GetUserId(User);
            var professor = await _context.Professors.FirstOrDefaultAsync(p => p.ApplicationUserId == userId);

            // Check if professor is main or assistant for this course
            var course = await _context.Courses.FirstOrDefaultAsync(c =>
                c.Id == courseId && (c.MainId == professor.Id || c.AssistantId == professor.Id));
            if (course == null)
                return Forbid();

            ViewBag.CourseId = courseId;
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Profesor")]
        public async Task<IActionResult> CreateAnnouncement(int courseId, string title, string body)
        {
            var userId = _userManager.GetUserId(User);
            var professor = await _context.Professors
                .Include(p => p.ApplicationUser)
                .FirstOrDefaultAsync(p => p.ApplicationUserId == userId);

            var course = await _context.Courses
                .Include(c => c.CoursesStudents)
                    .ThenInclude(cs => cs.Student)
                .FirstOrDefaultAsync(c => c.Id == courseId && (c.MainId == professor.Id || c.AssistantId == professor.Id));
            if (course == null)
                return Forbid();

            var announcement = new Announcement
            {
                CourseId = courseId,
                ProfessorId = professor.Id,
                Title = title,
                Body = body,
                PostedAt = DateTime.Now
            };

            _context.Announcements.Add(announcement);
            await _context.SaveChangesAsync();

            foreach (var cs in course.CoursesStudents)
            {
                var notification = new Notification
                {
                    StudentUserId = cs.Student.ApplicationUserId,
                    AnnouncementId = announcement.Id,
                    CreatedAt = DateTime.Now,
                    IsRead = false
                };
                _context.Notifications.Add(notification);
            }
            await _context.SaveChangesAsync();

            return RedirectToAction("CoursePage", "Home", new { id = courseId });
        }



    }
}
