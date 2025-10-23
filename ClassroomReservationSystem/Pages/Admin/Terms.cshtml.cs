using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using ClassroomReservationSystem.Data;
using ClassroomReservationSystem.Models;

namespace ClassroomReservationSystem.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class TermsModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public TermsModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<Term> Terms { get; set; }

        public void OnGet()
        {
            Terms = _context.Terms
                .OrderByDescending(t => t.StartDate)
                .ToList();
        }
    }
}
