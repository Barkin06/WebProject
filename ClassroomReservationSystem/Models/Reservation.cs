using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ClassroomReservationSystem.Data;

namespace ClassroomReservationSystem.Models
{
    public class Reservation{
        public int Id { get; set; }

        [Required]
        public DateTime StartTime { get; set; }

        [Required]
        public DateTime EndTime { get; set; }

        [Required]
        [Display(Name = "Course Name")]
        public string CourseName { get; set; }
        public bool IsApproved { get; set; } = false;
        public bool IsRejected { get; set; } = false;
        public bool IsPending { get; set; } = true;

        public int ClassroomId { get; set; }
        public Classroom Classroom { get; set; }

        public string ApplicationUserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; }

        public int TermId { get; set; }
        public Term Term { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Guid? ReservationGroupId { get; set; }

    }

}
