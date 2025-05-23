using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

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
        [Range(1, 4)]
        public int? Year { get; set; }
        public List<Announcement> Announcements { get; set; } = new List<Announcement>();
        public List<CourseStudent>? CoursesStudents { get; set; } = [];
    }
}
