using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using ClassroomReservationSystem.Data;
using ClassroomReservationSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace ClassroomReservationSystem.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class EditTermModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public EditTermModel(ApplicationDbContext context)
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
            if (!ModelState.IsValid)
                return Page();

            _context.Attach(Term).State = EntityState.Modified;
            _context.SaveChanges();
            return RedirectToPage("Terms");
        }
    }
}
