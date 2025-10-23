using System.ComponentModel.DataAnnotations;
using ClassroomReservationSystem.Data;

namespace ClassroomReservationSystem.Models
{
    public class Feedback
    {
        public int Id { get; set; }

        [Range(1, 5)]
        public int Rating { get; set; }

        [Required]
        [Display(Name = "Course Name")]
        public string CourseName { get; set; }

        public string Comment { get; set; } 

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;


    }
}
