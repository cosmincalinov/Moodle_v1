using System.ComponentModel.DataAnnotations;

namespace Moodle_v1.Models
{
    public class Announcement
    {
        [Key]
        public int Id { get; set; }
        public int CourseId { get; set; }
        public int ProfessorId { get; set; } // Store the Professor's ID
        public string Title { get; set; }
        public string Body { get; set; }
        public DateTime PostedAt { get; set; }
        public Professor Professor { get; set; }
    }
}
