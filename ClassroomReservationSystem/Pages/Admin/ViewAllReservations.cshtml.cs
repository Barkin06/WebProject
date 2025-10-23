using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ClassroomReservationSystem.Data;
using ClassroomReservationSystem.Models;
using Microsoft.AspNetCore.Authorization;

//GPT, the code I sent you has a problem, it should not show the rejected lessons on calendar, but it shows, would u mind fixing it?
namespace ClassroomReservationSystem.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class ViewAllReservationsModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public ViewAllReservationsModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<Reservation> FilteredReservations { get; set; } = new List<Reservation>();


        public JsonResult OnGetEvents()
        {
            var startDate = new DateTime(2025, 2, 17, 9, 0, 0);
            var endDate = new DateTime(2025, 6, 15, 19, 0, 0);

            var events = _context.Reservations
                .Include(r => r.ApplicationUser)
                .Include(r => r.Classroom)
                .Where(r =>
                    r.StartTime >= startDate &&
                    r.EndTime <= endDate &&
                    r.StartTime.Hour >= 9 &&
                    r.EndTime.Hour <= 19 &&
                    !r.IsRejected)
                .Select(r => new
                {
                    id = r.Id,
                    title = $"{r.CourseName} - {r.Classroom.Name} - {r.ApplicationUser.FullName}" + (r.IsApproved ? "" : " (Pending)"),
                    start = r.StartTime.ToString("s"),
                    end = r.EndTime.ToString("s"),
                    color = r.IsApproved ? "#28a745"
                          : r.IsPending ? "#007bff"
                          : "#dc3545"
                })
                .ToList();

            return new JsonResult(events);
        }

        public void OnGet()
        {
            var startDate = new DateTime(2025, 2, 17, 9, 0, 0);
            var endDate = new DateTime(2025, 6, 15, 19, 0, 0);

            FilteredReservations = _context.Reservations
                .Include(r => r.ApplicationUser)
                .Include(r => r.Classroom)
                .Include(r => r.Term)
                .Where(r =>
                    r.StartTime >= startDate &&
                    r.EndTime <= endDate &&
                    r.StartTime.Hour >= 9 &&
                    r.EndTime.Hour <= 19)
                .OrderBy(r => r.StartTime)
                .ToList();
        }
    }
}
