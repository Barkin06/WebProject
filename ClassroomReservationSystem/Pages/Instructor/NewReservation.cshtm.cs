using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ClassroomReservationSystem.Data;
using ClassroomReservationSystem.Models;
using System.Security.Claims;
using System.Text.Json;

namespace ClassroomReservationSystem.Pages.Instructor
{
    [Authorize(Roles = "Instructor")]
    public class NewReservationModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public NewReservationModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Reservation Reservation { get; set; }

        [BindProperty]
        public DateTime StartTime { get; set; }

        [BindProperty]
        public TimeSpan EndTime { get; set; } // Sadece saat alınacak

        [BindProperty]
        public bool ApplyToAllWeeks { get; set; } = false;

        public List<SelectListItem> ClassroomOptions { get; set; } = new();

        public void OnGet()
        {
            LoadDropdowns();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            LoadDropdowns();

            var activeTerm = _context.Terms.FirstOrDefault(t => t.IsActive);
            if (activeTerm == null)
            {
                TempData["WarningMessage"] = "No active term found.";
                return RedirectToPage();
            }

            var endDateTime = StartTime.Date + EndTime;

            if (StartTime < new DateTime(2025, 2, 17) || endDateTime > new DateTime(2025, 6, 15))
            {
                TempData["WarningMessage"] = "Reservations must be between 17.02.2025 and 15.06.2025.";
                return RedirectToPage();
            }

            if (StartTime.DayOfWeek == DayOfWeek.Saturday || StartTime.DayOfWeek == DayOfWeek.Sunday)
            {
                TempData["WarningMessage"] = "Weekend lectures are not allowed.";
                return RedirectToPage();
            }

            if (StartTime.Hour < 9 || endDateTime.Hour > 18 || endDateTime <= StartTime)
            {
                TempData["WarningMessage"] = "Reservations must be between 09:00 and 18:00, and end time must be after start time.";
                return RedirectToPage();
            }

            if (await IsHolidayAsync(StartTime))
            {
                TempData["WarningMessage"] = "This is a national holiday. Reservation not allowed.";
                return RedirectToPage();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            
            bool classroomTimeConflict = _context.Reservations.Any(r =>
                r.ClassroomId == Reservation.ClassroomId &&
                r.StartTime.Date == StartTime.Date &&
                r.StartTime < endDateTime &&
                r.EndTime > StartTime &&
                r.IsApproved);

            if (classroomTimeConflict)
            {
                TempData["WarningMessage"] = "Another reservation has already been approved for this classroom at the selected time.";
                return RedirectToPage();
            }

            
            bool courseTakenByOthers = _context.Reservations.Any(r =>
                r.CourseName == Reservation.CourseName &&
                (r.IsPending || r.IsApproved) &&
                r.ApplicationUserId != userId);

            if (courseTakenByOthers)
            {
                TempData["WarningMessage"] = $"The course '{Reservation.CourseName}' is already reserved by another instructor.";
                return RedirectToPage();
            }

            
            bool alreadyReserved = _context.Reservations
                .Any(r => r.ApplicationUserId == userId &&
                          r.StartTime.Date == StartTime.Date &&
                          r.StartTime.TimeOfDay == StartTime.TimeOfDay);

            if (alreadyReserved)
            {
                TempData["WarningMessage"] = "You already have a reservation at this time.";
                return RedirectToPage();
            }

            
            if (ApplyToAllWeeks)
            {
                TempData["InfoMessage"] = "Your reservation has been applied to all weeks of the term.";

                var reservations = new List<Reservation>();
                DateTime current = activeTerm.StartDate;
                var groupId = Guid.NewGuid();

                while (current <= activeTerm.EndDate)
                {
                    if (current.DayOfWeek == StartTime.DayOfWeek)
                    {
                        reservations.Add(new Reservation
                        {
                            ApplicationUserId = userId,
                            ClassroomId = Reservation.ClassroomId,
                            TermId = activeTerm.TermId,
                            StartTime = current.Date + StartTime.TimeOfDay,
                            EndTime = current.Date + EndTime,
                            CourseName = Reservation.CourseName,
                            IsApproved = false,
                            IsRejected = false,
                            IsPending = true,
                            ReservationGroupId = groupId
                        });
                    }
                    current = current.AddDays(1);
                }

                _context.Reservations.AddRange(reservations);
            }
            else
            {
                var singleReservation = new Reservation
                {
                    ApplicationUserId = userId,
                    ClassroomId = Reservation.ClassroomId,
                    TermId = activeTerm.TermId,
                    StartTime = StartTime,
                    EndTime = endDateTime,
                    CourseName = Reservation.CourseName,
                    IsApproved = false,
                    IsRejected = false,
                    IsPending = true
                };

                _context.Reservations.Add(singleReservation);
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Reservation submitted successfully!";
            return RedirectToPage();
        }

        private void LoadDropdowns()
        {
            ClassroomOptions = _context.Classrooms
                .Select(c => new SelectListItem
                {
                    Value = c.ClassroomId.ToString(),
                    Text = c.Name
                }).ToList();
        }

        private async Task<bool> IsHolidayAsync(DateTime date)
        {
            try
            {
                int year = date.Year;
                string apiUrl = $"https://date.nager.at/api/v3/PublicHolidays/{year}/TR";

                using var httpClient = new HttpClient();
                var response = await httpClient.GetAsync(apiUrl);
                if (!response.IsSuccessStatusCode)
                    return false;

                var json = await response.Content.ReadAsStringAsync();
                var holidays = JsonSerializer.Deserialize<List<PublicHoliday>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return holidays?.Any(h => h.Date.Date == date.Date) ?? false;
            }
            catch
            {
                return false;
            }
        }

        private class PublicHoliday
        {
            public DateTime Date { get; set; }
            public string LocalName { get; set; }
            public string Name { get; set; }
        }
    }
}
