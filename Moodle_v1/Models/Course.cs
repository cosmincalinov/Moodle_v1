using Microsoft.EntityFrameworkCore;

namespace Moodle_v1.Models
{
    public class Course
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public uint NoCredits { get; set; }
        public int? MainId { get; set; }
        public Professor? Main { get; set; }
        public int? AssistantId { get; set; }
        public Professor? Assistant { get; set; }

        public List<CourseStudent>? CoursesStudents { get; set; } = [];
    }
}
