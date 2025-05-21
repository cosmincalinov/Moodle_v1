using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moodle_v1.Areas.Identity.Data;
using Moodle_v1.Data;
using Moodle_v1.Models;

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

        public IActionResult Index()
        {
            return View();
        }
    }
}
