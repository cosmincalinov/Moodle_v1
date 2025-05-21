using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Moodle_v1.Areas.Identity.Data;
using Moodle_v1.Models;

namespace Moodle_v1.Data;

public class AuthDbContext : IdentityDbContext<ApplicationUser>
{
    public DbSet<Course> Courses { get; set; }
    public DbSet<Student> Students { get; set; }
    public DbSet<Professor> Professors { get; set; }
    public DbSet<CourseStudent> CourseStudents { get; set; }
    public DbSet<Announcement> Announcements { get; set; }
    public DbSet<Secretary> Secretaries { get; set; }
    public DbSet<ChatMessage> ChatMessages { get; set; }

    public AuthDbContext(DbContextOptions<AuthDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<CourseStudent>()
            .HasKey(cs => new { cs.StudentId, cs.CourseId });

        builder.Entity<CourseStudent>()
            .HasOne(cs => cs.Student)
            .WithMany(s => s.CoursesStudents)
            .HasForeignKey(cs => cs.StudentId);

        builder.Entity<CourseStudent>()
            .HasOne(cs => cs.Course)
            .WithMany(c => c.CoursesStudents)
            .HasForeignKey(cs => cs.CourseId);

        builder.Entity<Course>()
            .HasOne(c => c.Main)
            .WithMany()
            .HasForeignKey(c => c.MainId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Course>()
            .HasOne(c => c.Assistant)
            .WithMany()
            .HasForeignKey(c => c.AssistantId)
            .OnDelete(DeleteBehavior.Restrict);
}


}


