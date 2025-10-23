using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using ClassroomReservationSystem.Data;
using ClassroomReservationSystem.Models;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;

namespace ClassroomReservationSystem.Pages.Public
{
    public class FeedbackModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public FeedbackModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public FeedbackInputModel Input { get; set; }

        public List<SelectListItem> CourseOptions { get; set; } = new();

        public string SuccessMessage { get; set; }
        public string ErrorMessage { get; set; }

        public class FeedbackInputModel
        {
            [Required(ErrorMessage = "Course name is required")]
            public string CourseName { get; set; }

            [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
            public int Rating { get; set; }

            public string Comment { get; set; }
        }

        public void OnGet()
        {
            CourseOptions = _context.Reservations
                .Select(r => r.CourseName)
                .Distinct()
                .OrderBy(c => c)
                .Select(c => new SelectListItem { Value = c, Text = c })
                .ToList();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Yeniden kurs listesini yükle (dropdown için)
            CourseOptions = _context.Reservations
                .Select(r => r.CourseName)
                .Distinct()
                .OrderBy(c => c)
                .Select(c => new SelectListItem { Value = c, Text = c })
                .ToList();

            if (!ModelState.IsValid)
            {
                ErrorMessage = "Please fill in all required fields correctly.";
                return Page();
            }

            // Gönderilen CourseName'in sistemde olup olmadığını kontrol et
            if (!CourseOptions.Any(c => c.Value == Input.CourseName))
            {
                ModelState.AddModelError("Input.CourseName", "Selected course is invalid.");
                return Page();
            }

            var feedback = new Feedback
            {
                CourseName = Input.CourseName.Trim(),
                Rating = Input.Rating,
                Comment = Input.Comment?.Trim()
            };

            _context.Feedbacks.Add(feedback);
            await _context.SaveChangesAsync();

            SuccessMessage = "Thank you for your feedback!";

            // Form temizleme
            ModelState.Clear();
            Input = new FeedbackInputModel();

            return Page();
        }
    }
}
