using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.Extensions.Options;

namespace ClassroomReservationSystem.Models
{
    public class Term
    {
        public int TermId { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Start date is required.")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "End date is required.")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        [ValidateNever]
        public ICollection<Classroom> Classrooms { get; set; }

        [ValidateNever]
        public ICollection<Reservation> Reservations { get; set; }

        public bool IsActive { get; set; } = false;
    }
}
