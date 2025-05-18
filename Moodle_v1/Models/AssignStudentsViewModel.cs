using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Moodle_v1.Models;
using System.ComponentModel.DataAnnotations;

public class AssignStudentsViewModel
{
    [BindNever, ValidateNever]
    public List<Course> Courses { get; set; }
    [BindNever, ValidateNever]
    public List<Student> Students { get; set; }

    [Required]
    public int CourseId { get; set; }

    [Required]
    public List<int> SelectedStudentIds { get; set; }
}
