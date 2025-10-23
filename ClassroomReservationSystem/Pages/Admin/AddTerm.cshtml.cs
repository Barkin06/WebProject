//GPT, help me write term adding editing and deleting codes according to code that I shared: (Terms.cshtml.cs).

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using ClassroomReservationSystem.Data;
using ClassroomReservationSystem.Models;
using System.Diagnostics;

namespace ClassroomReservationSystem.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class AddTermModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public AddTermModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Term Term { get; set; } = new Term();

        public List<string> ErrorMessages { get; set; } = new();
        public IActionResult OnGet()
        {
            return Page();
        }

        public IActionResult OnPost()
        {
            if (Term.EndDate <= Term.StartDate)
            {
                ModelState.AddModelError(string.Empty, "End date must be after start date.");
            }

            if (!ModelState.IsValid)
            {
                ErrorMessages = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return Page();
            }

            Term.IsActive = false;

            try
            {
                _context.Terms.Add(Term);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "An error occurred while saving the term: " + ex.Message);
                return Page();
            }

            return RedirectToPage("Terms");
        }
    }
}
