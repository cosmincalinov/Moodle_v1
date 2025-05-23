using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moodle_v1.Areas.Identity.Data;
using Moodle_v1.Data;
using Moodle_v1.Models;
var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("AuthDbContextConnection") ?? throw new InvalidOperationException("Connection string 'AuthDbContextConnection' not found.");

builder.Services.AddDbContext<AuthDbContext>(options => options.UseSqlServer(connectionString));

builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<AuthDbContext>();

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequireUppercase = false;
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

using (var scope = app.Services.CreateScope())
{
    var roleManager = 
        scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    var roles = new[] { "Admin", "Secretar", "Profesor", "Student" };

    foreach(var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));
    }
}

using (var scope = app.Services.CreateScope())
{
    var userManager =
        scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    string email = "admin@admin.com";
    string password = "abc123!!";

    if (await userManager.FindByEmailAsync(email) == null)
    {
        var user = new ApplicationUser();
        user.UserName = email;
        user.Email = email;
        user.FirstName = "Cosminel";
        user.LastName = "Cosminov";

        await userManager.CreateAsync(user, password);

        await userManager.AddToRoleAsync(user, "Admin");
    }

    string emailProf = "prof@prof.com";
    string passwordProf = "prof123!";

    if (await userManager.FindByEmailAsync(emailProf) == null)
    {
        var prof = new ApplicationUser();
        prof.UserName = emailProf;
        prof.Email = emailProf;
        prof.FirstName = "Dragan";
        prof.LastName = "Tudose";

        await userManager.CreateAsync(prof, passwordProf);

        await userManager.AddToRoleAsync(prof, "Profesor");
    }


    string emailStudent = "student@student.com";
    string passwordStudent = "student123!";

    if (await userManager.FindByEmailAsync(emailStudent) == null)
    {
        var stud = new ApplicationUser();
        stud.UserName = emailStudent;
        stud.Email = emailStudent;
        stud.FirstName = "Chija";
        stud.LastName = "Claudia";

        await userManager.CreateAsync(stud, password);

        await userManager.AddToRoleAsync(stud, "Student");
    }

    string emailSec = "secretar@secretar.com";
    string passwordSec = "sec123!";

    if (await userManager.FindByEmailAsync(emailSec) == null)
    {
        var secretar = new ApplicationUser();
        secretar.UserName = emailSec;
        secretar.Email = emailSec;
        secretar.FirstName = "Chef";
        secretar.LastName = "Bucatar";

        await userManager.CreateAsync(secretar, password);

        await userManager.AddToRoleAsync(secretar, "Secretar");
    }
}

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    var context = services.GetRequiredService<AuthDbContext>();

    var user = await userManager.FindByEmailAsync("student@student.com");
    if (user != null && await userManager.IsInRoleAsync(user, "Student"))
    {
        if (!context.Students.Any(s => s.ApplicationUserId == user.Id))
        {
            var student = new Student
            {
                ApplicationUserId = user.Id,
                CurrentYear = 1,
                Domain = null
            };
            context.Students.Add(student);
            await context.SaveChangesAsync();
        }
    }
}

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    var context = services.GetRequiredService<AuthDbContext>();

    var user = await userManager.FindByEmailAsync("prof@prof.com");
    if (user != null && await userManager.IsInRoleAsync(user, "Profesor"))
    {
        if (!context.Professors.Any(s => s.ApplicationUserId == user.Id))
        {
            var prof = new Professor
            {
                ApplicationUserId = user.Id,
                HireDate = DateTime.Now,
            };

            context.Professors.Add(prof);
            await context.SaveChangesAsync();
        }
    }
}

app.MapRazorPages();

app.Run();
