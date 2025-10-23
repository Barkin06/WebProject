using Microsoft.AspNetCore.Identity;


namespace ClassroomReservationSystem.Data
{
    public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;

    public string FullName => $"{FirstName} {LastName}";

    public ICollection<SystemLog>? SystemLogs { get; set; }
}

}

