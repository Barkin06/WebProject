using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using ClassroomReservationSystem.Data;
using ClassroomReservationSystem.Models;

namespace ClassroomReservationSystem.Pages.Instructor
{
    [Authorize(Roles = "Instructor")]
    public class DashboardModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DashboardModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public string FullName { get; set; }

        public void OnGet()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            FullName = user != null ? $"{user.FirstName} {user.LastName}" : "Instructor";
        }
    }
}
