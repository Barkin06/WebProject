using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ClassroomReservationSystem.Data;
using ClassroomReservationSystem.Models;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace ClassroomReservationSystem.Pages.Public
{
    public class ContactModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public ContactModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Contact Contact { get; set; }

        public string SuccessMessage { get; set; }
        public string ErrorMessage { get; set; }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                ErrorMessage = "Please fix the validation errors.";
                return Page();
            }

            // reCAPTCHA client-side response
            string recaptchaResponse = Request.Form["g-recaptcha-response"];
            if (string.IsNullOrEmpty(recaptchaResponse))
            {
                ErrorMessage = "Please complete the reCAPTCHA.";
                return Page();
            }

            // Verify via Google API
            using var client = new HttpClient();
            var postData = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("secret", "6LeoCDwrAAAAADhx__0m3lzMlbtdLP3dDfuQok-O"),
                new KeyValuePair<string, string>("response", recaptchaResponse)
            });

            var response = await client.PostAsync("https://www.google.com/recaptcha/api/siteverify", postData);
            var result = await response.Content.ReadAsStringAsync();

            var captchaResult = JsonSerializer.Deserialize<RecaptchaResponse>(result);
            if (captchaResult == null || !captchaResult.success)
            {
                ErrorMessage = "reCAPTCHA verification failed.";
                return Page();
            }

            // Save message to DB
            _context.Contacts.Add(Contact);
            await _context.SaveChangesAsync();

            SuccessMessage = "Your message has been sent successfully!";
            ModelState.Clear();
            Contact = new Contact();

            return Page();
        }

        public class RecaptchaResponse
        {
            public bool success { get; set; }
            public string challenge_ts { get; set; }
            public string hostname { get; set; }
        }
    }
}
