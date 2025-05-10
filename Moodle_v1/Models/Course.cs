namespace Moodle_v1.Models
{
    public class Course
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public uint NoCredits { get; set; }
        public Professor Main { get; set; }
        public Professor Assistant { get; set; }
        public List<CourseStudent> CoursesStudents { get; set; } = [];
    }
}
