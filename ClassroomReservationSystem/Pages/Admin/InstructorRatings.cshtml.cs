using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClassroomReservationSystem.Data;
using ClassroomReservationSystem.Models;
using Microsoft.EntityFrameworkCore;

//GPT, How can I connect database's Feedback section and create an Instructor Ratings page with them?

//Okay gpt, I shared the code with you but there is an error _context does not exists, why?

//Can you also use some symbols (star and schedule symbol) on the cshtml and create one for me?
namespace ClassroomReservationSystem.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class InstructorRatingsModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public InstructorRatingsModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<InstructorCourseRating> InstructorCourseRatings { get; set; } = new();
        public List<InstructorRatingSummary> InstructorSummaries { get; set; } = new();

        public async Task OnGetAsync()
        {
            var validCoursesWithFeedback = await _context.Feedbacks
                .Select(f => f.CourseName)
                .Distinct()
                .ToListAsync();

            InstructorCourseRatings = await _context.Reservations
                .Include(r => r.ApplicationUser)
                .Where(r => validCoursesWithFeedback.Contains(r.CourseName))
                .GroupBy(r => new { r.ApplicationUserId, r.ApplicationUser.FirstName, r.ApplicationUser.LastName, r.CourseName })
                .Select(g => new InstructorCourseRating
                {
                    InstructorId = g.Key.ApplicationUserId,
                    InstructorFullName = g.Key.FirstName + " " + g.Key.LastName,
                    CourseName = g.Key.CourseName,
                    AverageRating = _context.Feedbacks
                        .Where(f => f.CourseName == g.Key.CourseName && f.Rating > 0)
                        .Average(f => (double?)f.Rating) ?? 0
                })
                .Where(x => x.AverageRating > 0)
                .ToListAsync();

            InstructorSummaries = InstructorCourseRatings
                .GroupBy(i => new { i.InstructorId, i.InstructorFullName })
                .Select(g => new InstructorRatingSummary
                {
                    InstructorId = g.Key.InstructorId,
                    InstructorFullName = g.Key.InstructorFullName,
                    OverallAverageRating = g.Average(x => x.AverageRating)
                })
                .ToList();
        }

        public class InstructorCourseRating
        {
            public string InstructorId { get; set; }
            public string InstructorFullName { get; set; }
            public string CourseName { get; set; }
            public double AverageRating { get; set; }
        }

        public class InstructorRatingSummary
        {
            public string InstructorId { get; set; }
            public string InstructorFullName { get; set; }
            public double OverallAverageRating { get; set; }
        }
    }
}
