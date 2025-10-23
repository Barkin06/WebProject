using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClassroomReservationSystem.Data;
using ClassroomReservationSystem.Models;
using Microsoft.EntityFrameworkCore;

//GPT, write a feedback view codes for instrucot, also it should show us the avarage ratings of instructor.

namespace ClassroomReservationSystem.Pages.Instructor
{
    [Authorize(Roles = "Instructor")]
    public class FeedbackSummaryModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public FeedbackSummaryModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<CourseFeedbackSummary> CourseSummaries { get; set; } = new();

        public double GeneralAverage { get; set; }

        public async Task OnGetAsync()
        {
            var instructorId = User.FindFirstValue(ClaimTypes.NameIdentifier);


            var instructorCourseNames = await _context.Reservations
                .Where(r => r.ApplicationUserId == instructorId)
                .Select(r => r.CourseName)
                .Distinct()
                .ToListAsync();


            CourseSummaries = await _context.Feedbacks
                .Where(f => instructorCourseNames.Contains(f.CourseName))
                .GroupBy(f => f.CourseName)
                .Select(g => new CourseFeedbackSummary
                {
                    CourseName = g.Key,
                    AverageRating = g.Average(f => f.Rating)
                })
                .ToListAsync();


            GeneralAverage = CourseSummaries.Any() ? CourseSummaries.Average(c => c.AverageRating) : 0;
        }

        public class CourseFeedbackSummary
        {
            public string CourseName { get; set; }
            public double AverageRating { get; set; }
        }
    }
}
