using Microsoft.AspNetCore.Mvc;

namespace Moodle_v1.Models
{
    public class Professor
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public DateTime HireDate { get; set; }
        public Rank Rank { get; set; }
    }
}
