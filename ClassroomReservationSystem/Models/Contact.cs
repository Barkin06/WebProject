
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ClassroomReservationSystem.Data;


public class Contact
{
    public int Id { get; set; }

    [Required]
    [Display(Name = "Full Name")]
    public string FullName { get; set; }

    [Required, EmailAddress]
    [Display(Name = "Email Address")]
    public string Email { get; set; }

    [Required]
    [Display(Name = "Subject")]
    public string Subject { get; set; }

    [Required]
    [Display(Name = "Message")]
    public string Message { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // FK ilişkisi ekle
    public string? UserId { get; set; }

    [ForeignKey("UserId")]
    public ApplicationUser? User { get; set; }
}

