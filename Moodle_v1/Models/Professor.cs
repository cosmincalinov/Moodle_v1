using Microsoft.AspNetCore.Mvc;
using Moodle_v1.Areas.Identity.Data;
using System.ComponentModel.DataAnnotations;

namespace Moodle_v1.Models
{
    public class Professor
    {
        [Key]
        public int Id { get; set; }
        public string? ApplicationUserId { get; set; }
        public ApplicationUser? ApplicationUser { get; set; }
        public DateTime HireDate { get; set; }
    }
}
