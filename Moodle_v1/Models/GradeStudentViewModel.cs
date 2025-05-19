namespace Moodle_v1.Models
{
    public class GradeStudentViewModel
    {
        public int CourseId { get; set; }
        public string CourseTitle { get; set; }
        public List<StudentGradeEntry> Students { get; set; }
    }

    public class StudentGradeEntry
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; }
        public double? Grade { get; set; }
    }
}
