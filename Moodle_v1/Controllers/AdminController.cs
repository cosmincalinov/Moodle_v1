using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Moodle_v1.Areas.Identity.Data;
using Moodle_v1.Data;
using Moodle_v1.Models;

namespace Moodle_v1.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly AuthDbContext _context;

    public AdminController(UserManager<ApplicationUser> userManager, AuthDbContext context, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _context = context;
        _roleManager = roleManager;
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

                    var student = await _context.Students.FindAsync(studentId);
                    var course = await _context.Courses.FindAsync(model.CourseId);
                    if (student != null && course != null)
                    {
                        var alert = new Alert
                        {
                            StudentUserId = student.ApplicationUserId,
                            Message = $"You have been added to the course '{course.Title}'.",
                            CreatedAt = DateTime.Now,
                            IsRead = false
                        };
                        _context.Alerts.Add(alert);
                    }
                }
            }
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Home");
        }

        model.Courses = await _context.Courses.ToListAsync();
        model.Students = await _context.Students.Include(s => s.ApplicationUser).ToListAsync();
        return View(model);
    }


    public async Task<IActionResult> AssignTeachers()
    {
        var viewModel = new AssignTeachersViewModel
        {
            Courses = await _context.Courses
                .Where(c => c.MainId == null || c.AssistantId == null)
                .ToListAsync(),
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

            course.Main = model.MainProfessorId.HasValue
                ? await _context.Professors.FindAsync(model.MainProfessorId.Value)
                : null;
            course.Assistant = model.AssistantProfessorId.HasValue
                ? await _context.Professors.FindAsync(model.AssistantProfessorId.Value)
                : null;

            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Home");
        }

        model.Courses = await _context.Courses.ToListAsync();
        model.Professors = await _context.Professors
            .Include(p => p.ApplicationUser)
            .ToListAsync();
        return View(model);
    }

    [Authorize(Roles = "Admin")]
    public IActionResult AddCourse()
    {
        ViewBag.Professors = _context.Professors
            .Include(p => p.ApplicationUser)
            .Select(a => new SelectListItem
            {
                Value = a.Id.ToString(),
                Text = a.ApplicationUser.LastName
            }).ToList();
        return View();
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public IActionResult AddCourse(Course course)
    {
        if (ModelState.IsValid)
        {
            _context.Courses.Add(course);
            _context.SaveChanges();
            return RedirectToAction("Index", "Home");
        }
        ViewBag.Professors = _context.Professors
            .Include(p => p.ApplicationUser)
            .Select(a => new SelectListItem
            {
                Value = a.Id.ToString(),
                Text = a.ApplicationUser.LastName
            }).ToList();
        return View(course);
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AssignRole()
    {
        var users = _userManager.Users.ToList();
        var usersWithoutRoles = new List<SelectListItem>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            if (!roles.Any())
            {
                usersWithoutRoles.Add(new SelectListItem
                {
                    Value = user.Id,
                    Text = user.Email
                });
            }
        }

        ViewBag.Users = usersWithoutRoles;

        ViewBag.Roles = _roleManager.Roles
            .Select(r => new SelectListItem
            {
                Value = r.Name,
                Text = r.Name
            }).ToList();

        return View();
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AssignRole(string userId, string role)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            ModelState.AddModelError("", "User not found.");
        }
        else
        {
            var userRoles = await _userManager.GetRolesAsync(user);
            if (userRoles.Any())
            {
                ModelState.AddModelError("", "User already has a role assigned.");
            }
            else
            {
                await _userManager.AddToRoleAsync(user, role);

                if (role == "Student" && !_context.Students.Any(s => s.ApplicationUserId == user.Id))
                {
                    var student = new Student
                    {
                        ApplicationUserId = user.Id,
                        CurrentYear = 1,
                        Domain = null
                    };
                    _context.Students.Add(student);
                    await _context.SaveChangesAsync();
                }

                if (role == "Profesor" && !_context.Professors.Any(p => p.ApplicationUserId == user.Id))
                {
                    var professor = new Professor
                    {
                        ApplicationUserId = user.Id,
                        HireDate = DateTime.Now
                    };
                    _context.Professors.Add(professor);
                    await _context.SaveChangesAsync();
                }

                if (role == "Secretar" && !_context.Secretaries.Any(s => s.ApplicationUserId == user.Id))
                {
                    var secretary = new Secretary
                    {
                        ApplicationUserId = user.Id
                    };
                    _context.Secretaries.Add(secretary);
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction("Index", "Home");
            }
        }

        ViewBag.Users = _userManager.Users
            .Select(u => new SelectListItem
            {
                Value = u.Id,
                Text = u.Email
            }).ToList();

        ViewBag.Roles = _roleManager.Roles
            .Select(r => new SelectListItem
            {
                Value = r.Name,
                Text = r.Name
            }).ToList();

        return View();
    }

    [Authorize(Roles = "Admin")]
    public IActionResult EditCourse(int id)
    {
        var course = _context.Courses
            .Include(c => c.Main)
            .Include(c => c.Assistant)
            .FirstOrDefault(c => c.Id == id);

        if (course == null)
            return NotFound();

        ViewBag.Professors = _context.Professors
            .Include(p => p.ApplicationUser)
            .Select(a => new SelectListItem
            {
                Value = a.Id.ToString(),
                Text = a.ApplicationUser.LastName
            }).ToList();

        return View(course);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public IActionResult EditCourse(Course course)
    {
        if (ModelState.IsValid)
        {
            _context.Courses.Update(course);
            _context.SaveChanges();
            return RedirectToAction("Index", "Home");
        }
        ViewBag.Professors = _context.Professors
            .Include(p => p.ApplicationUser)
            .Select(a => new SelectListItem
            {
                Value = a.Id.ToString(),
                Text = a.ApplicationUser.LastName
            }).ToList();
        return View(course);
    }

    [Authorize(Roles = "Admin")]
    public IActionResult DeleteCourse(int id)
    {
        var course = _context.Courses.Find(id);
        if (course != null)
        {
            _context.Courses.Remove(course);
            _context.SaveChanges();
        }
        return RedirectToAction("Index", "Home");
    }


}
