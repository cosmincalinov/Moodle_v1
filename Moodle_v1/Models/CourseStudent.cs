using Microsoft.EntityFrameworkCore;

namespace Moodle_v1.Models
{
    [PrimaryKey(nameof(StudentId), nameof(CourseId))]
    public class CourseStudent
    {
        public int StudentId { get; set; }
        public int CourseId { get; set; }
        public Student Student { get; set; } = null!;
        public Course Course { get; set; } = null!;
        public double? Grade { get; set; }
    }
}
