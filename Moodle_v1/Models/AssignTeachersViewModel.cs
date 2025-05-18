using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace Moodle_v1.Models
{
    public class AssignTeachersViewModel
    {
        [BindNever, ValidateNever]
        public List<Course> Courses { get; set; }
        [BindNever, ValidateNever]
        public List<Professor> Professors { get; set; }

        [Required]
        public int CourseId { get; set; }

        public int? MainProfessorId { get; set; }
        public int? AssistantProfessorId { get; set; }
    }
}
