using Moodle_v1.Areas.Identity.Data;
using System.ComponentModel.DataAnnotations;

namespace Moodle_v1.Models
{
    public class Student
{
        [Key]
        public int NrMatricol { get; set; }
        public string? ApplicationUserId { get; set; }
        public ApplicationUser? ApplicationUser { get; set; }
        public uint CurrentYear { get; set; }
        public Domain? Domain { get; set; }
        public List<CourseStudent> CoursesStudents { get; set; }
}
}
