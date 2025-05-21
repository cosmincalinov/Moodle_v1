using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moodle_v1.Areas.Identity.Data;
using Moodle_v1.Data;
using Moodle_v1.Models;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using System.IO;

namespace Moodle_v1.Controllers
{
    public class SecretaryController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly AuthDbContext _context;

        public List<Course> courses { get; set; }
        public List<Student> students { get; set; }
        public List<Professor> professors { get; set; }
        public Course course { get; set; }

        public SecretaryController(ILogger<HomeController> logger, UserManager<ApplicationUser> userManager, AuthDbContext context, RoleManager<IdentityRole> roleManager)
        {
            _logger = logger;
            this._userManager = userManager;
            _context = context;
            _roleManager = roleManager;
        }   


        [Authorize(Roles = "Secretar")]
        public async Task<IActionResult> StudentGrades()
        {
            var students = await _context.Students
                .Include(s => s.ApplicationUser)
                .Include(s => s.CoursesStudents)
                    .ThenInclude(cs => cs.Course)
                .ToListAsync();

            return View(students);
        }


    [Authorize(Roles = "Secretar")]
    public async Task<IActionResult> ExportStudentGradesPdf()
    {
        var students = await _context.Students
            .Include(s => s.ApplicationUser)
            .Include(s => s.CoursesStudents)
                .ThenInclude(cs => cs.Course)
            .ToListAsync();

        var document = new PdfDocument();
        var page = document.AddPage();
        var gfx = XGraphics.FromPdfPage(page);

        var titleFont = new XFont("Arial", 18, XFontStyle.Bold);
        var headerFont = new XFont("Arial", 12, XFontStyle.Bold);
        var cellFont = new XFont("Arial", 12, XFontStyle.Regular);

        double y = 40;
        gfx.DrawString("Student Grades", titleFont, XBrushes.DarkBlue, new XRect(0, y, page.Width, 30), XStringFormats.TopCenter);
        y += 40;

        foreach (var student in students)
        {
            gfx.DrawString($"{student.ApplicationUser.FirstName} {student.ApplicationUser.LastName}", headerFont, XBrushes.Black, 40, y);
            y += 25;

            // Table header
            gfx.DrawRectangle(XPens.Black, XBrushes.LightGray, 40, y, 300, 20);
            gfx.DrawString("Course", headerFont, XBrushes.Black, 45, y + 15);
            gfx.DrawString("Grade", headerFont, XBrushes.Black, 220, y + 15);
            y += 20;

            bool alternate = false;
            foreach (var cs in student.CoursesStudents)
            {
                var bgBrush = alternate ? XBrushes.White : XBrushes.LightBlue;
                gfx.DrawRectangle(XPens.Black, bgBrush, 40, y, 300, 20);
                gfx.DrawString(cs.Course.Title, cellFont, XBrushes.Black, 45, y + 15);
                string grade = cs.Grade.HasValue ? cs.Grade.Value.ToString() : "Not graded";
                gfx.DrawString(grade, cellFont, XBrushes.Black, 220, y + 15);
                y += 20;
                alternate = !alternate;
            }
            y += 15;

            if (y > page.Height - 60)
            {
                page = document.AddPage();
                gfx = XGraphics.FromPdfPage(page);
                y = 40;
            }
        }

        using (var stream = new MemoryStream())
        {
            document.Save(stream, false);
            return File(stream.ToArray(), "application/pdf", "StudentGrades.pdf");
        }
    }
    public IActionResult Index()
        {
            return View();
        }
    }
}
