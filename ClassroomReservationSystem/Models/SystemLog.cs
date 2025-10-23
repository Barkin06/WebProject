using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ClassroomReservationSystem.Data;

public class SystemLog
{
    public int Id { get; set; }

    public string UserId { get; set; }

    [ForeignKey("UserId")]
    public ApplicationUser User { get; set; }

    public string Action { get; set; }
    public string Details { get; set; }

    public DateTime Timestamp { get; set; } = DateTime.Now;
}
