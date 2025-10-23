namespace ClassroomReservationSystem.Models;
public class Classroom
{
    public int ClassroomId { get; set; }
    public string Name { get; set; } 
    public int Capacity { get; set; }
    public int TermId { get; set; } 
    public Term Term { get; set; } 

    public ICollection<Reservation> Reservations { get; set; }
    public ICollection<Feedback> Feedbacks { get; set; }
}
