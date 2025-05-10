namespace Moodle_v1.Models
{
    public class Student
    {
        public int NrMatricol { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public uint CurrentYear { get; set; }
        public string Email { get; set; }
        public Domain Domain { get; set; }
        public List<CourseStudent> CoursesStudents { get; set; } = [];
    }
}
