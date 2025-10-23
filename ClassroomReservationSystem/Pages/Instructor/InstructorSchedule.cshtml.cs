using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using ClassroomReservationSystem.Data;
using ClassroomReservationSystem.Models;
using System.Security.Claims;

namespace ClassroomReservationSystem.Pages.Instructor
{
    [Authorize(Roles = "Instructor")]
    public class InstructorScheduleModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public InstructorScheduleModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public void OnGet() { }

        public JsonResult OnGetEvents()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var reservations = _context.Reservations
                .Include(r => r.Classroom)
                .Where(r => r.ApplicationUserId == userId && !r.IsRejected)
                .ToList();

            var events = reservations.Select(r => new
            {
                title = $"{r.CourseName} - {r.Classroom.Name}",
                start = r.StartTime,
                end = r.EndTime,
                className = r.IsApproved ? "event-approved" :
                            r.IsPending ? "event-pending" : "event-rejected"
            });

            return new JsonResult(events);
        }

    }
}
