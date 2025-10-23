using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using ClassroomReservationSystem.Data;
using ClassroomReservationSystem.Models;

namespace ClassroomReservationSystem.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class DeleteTermModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DeleteTermModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Term Term { get; set; }

        public IActionResult OnGet(int id)
        {
            Term = _context.Terms.FirstOrDefault(t => t.TermId == id);
            if (Term == null)
                return NotFound();

            return Page();
        }

        public IActionResult OnPost()
        {
            var termToDelete = _context.Terms.Find(Term.TermId);
            if (termToDelete != null)
            {
                _context.Terms.Remove(termToDelete);
                _context.SaveChanges();
            }

            return RedirectToPage("Terms");
        }
    }
}
