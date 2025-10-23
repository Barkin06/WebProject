using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ClassroomReservationSystem.Data;
using ClassroomReservationSystem.Models;
using ClassroomReservationSystem.Services;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
//GPT, I need you to create me a Reservations.cshtml.cs for reservation management

//Display a list of all reservations (with optional table clearing), Allow admins to approve or reject single reservations, 
//Support group approval or rejection using a shared a group ID so if I need to make a change on models or on database, tell me
//When approving a reservation, automatically detect and reject conflict in pending status (same classroom and overlapping time)
// Send email to users when their reservations are approved or rejected (I researched a little, SMTP protocol so, 
//I write the necessarry appsettings, i will share it with you if you need and my password-keys are ready as well)
//Log each action using SystemLog.cs (I will shaare with u)
//Use the other models as well, (I will share with you)


//GPT, In conflict, If I press approve one, other one is not rejecting automaticially, check the code, what should I do

//Good, last problem, mail system in group approves/rejects sends many mails. I just neeed one, I sent you the code.

namespace ClassroomReservationSystem.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class ReservationsModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly EmailService _emailService;
        private readonly LogService _logService;

        public ReservationsModel(ApplicationDbContext context,
                                 EmailService emailService,
                                 LogService logService)
        {
            _context = context;
            _emailService = emailService;
            _logService = logService;
        }

        public IList<Reservation> Reservations { get; set; } = new List<Reservation>();

        [TempData] public bool TableCleared { get; set; }

        // ─────────────────── GET ───────────────────
        public async Task OnGetAsync()
        {
            Reservations = TableCleared
                ? new List<Reservation>()
                : await ReloadReservationsAsync();
        }

        // ─────────────────── POST (approve / reject) ───────────────────
        public async Task<IActionResult> OnPostAsync(int reservationId, string action)
        {
            // ─────────────── GROUP ACTIONS ───────────────
            if ((action == "approveGroup" || action == "rejectGroup")
                && Request.Form.TryGetValue("groupId", out var gid)
                && Guid.TryParse(gid, out var groupId))
            {
                var groupReservations = await _context.Reservations
                    .Where(r => r.ReservationGroupId == groupId)
                    .Include(r => r.ApplicationUser)
                    .Include(r => r.Classroom)
                    .ToListAsync();

                if (!groupReservations.Any())
                    return NotFound();

                if (action == "approveGroup")
                {
                    var allPendingConflicts = new List<Reservation>();

                    foreach (var res in groupReservations)
                    {
                        // Onay çatışma kontrolü
                        var pending = await _context.Reservations
                            .Include(r => r.ApplicationUser)
                            .Include(r => r.Classroom)
                            .Where(r => r.Id != res.Id &&
                                        r.IsPending &&
                                        r.ClassroomId == res.ClassroomId &&
                                        r.StartTime.Date == res.StartTime.Date &&
                                        r.StartTime < res.EndTime &&
                                        r.EndTime > res.StartTime)
                            .ToListAsync();

                        allPendingConflicts.AddRange(pending);

                        res.IsApproved = true;
                        res.IsRejected = false;
                        res.IsPending = false;

                        _logService.Log(res.ApplicationUserId, "Group Approval",
                                        $"Reservation {res.Id} approved.");
                    }

                    // Hepsini tek seferde reddet → tek mail
                    await RejectConflictGroupsAsync(groupReservations.First().Id,
                        allPendingConflicts
                            .GroupBy(x => x.Id)
                            .Select(g => g.First())
                            .ToList());

                    await _context.SaveChangesAsync();
                    await SendGroupNotificationAsync(groupReservations, approved: true);
                }
                else // rejectGroup
                {
                    foreach (var res in groupReservations)
                    {
                        res.IsApproved = false;
                        res.IsRejected = true;
                        res.IsPending = false;

                        _logService.Log(res.ApplicationUserId, "Group Rejection",
                                        $"Reservation {res.Id} rejected.");
                    }

                    await _context.SaveChangesAsync();
                    await SendGroupNotificationAsync(groupReservations, approved: false);
                }

                Reservations = await ReloadReservationsAsync();
                return Page();
            }

            // ─────────────── SINGLE RESERVATION ───────────────
            var reservation = await _context.Reservations
                .Include(r => r.ApplicationUser)
                .Include(r => r.Classroom)
                .FirstOrDefaultAsync(r => r.Id == reservationId);

            if (reservation == null) return NotFound();

            var pendingConflicts = await _context.Reservations
                .Include(r => r.ApplicationUser)
                .Include(r => r.Classroom)
                .Where(r => r.Id != reservation.Id &&
                            r.IsPending &&
                            r.ClassroomId == reservation.ClassroomId &&
                            r.StartTime.Date == reservation.StartTime.Date &&
                            r.StartTime < reservation.EndTime &&
                            r.EndTime > reservation.StartTime)
                .ToListAsync();

            if (action == "approve")
            {
                reservation.IsApproved = true;
                reservation.IsRejected = false;
                reservation.IsPending = false;

                await RejectConflictGroupsAsync(reservation.Id, pendingConflicts);
                await NotifyAsync(reservation.ApplicationUser.Email,
                    "Reservation Approved",
                    $"Your reservation for {reservation.CourseName} in {reservation.Classroom.Name} on {reservation.StartTime:g} has been approved.");

                _logService.Log(reservation.ApplicationUserId, "Reservation Approved",
                    $"Reservation {reservation.Id} approved.");
            }
            else if (action == "reject")
            {
                reservation.IsApproved = false;
                reservation.IsRejected = true;
                reservation.IsPending = false;

                await NotifyAsync(reservation.ApplicationUser.Email,
                    "Reservation Rejected",
                    $"Your reservation for {reservation.CourseName} in {reservation.Classroom.Name} on {reservation.StartTime:g} has been rejected.");

                _logService.Log(reservation.ApplicationUserId, "Reservation Rejected",
                    $"Reservation {reservation.Id} rejected.");
            }
            else return BadRequest("Invalid action.");

            await _context.SaveChangesAsync();
            Reservations = await ReloadReservationsAsync();
            return Page();
        }

        // ─────────────────── POST helpers ───────────────────
        public async Task<IActionResult> OnPostClearAllAsync()
        {
            _context.Reservations.RemoveRange(_context.Reservations);
            _context.Feedbacks.RemoveRange(_context.Feedbacks);
            await _context.SaveChangesAsync();
            return RedirectToPage();
        }

        public IActionResult OnPostClearTable()
        {
            TableCleared = true;
            return RedirectToPage();
        }

        // ─────────────────── UTILITIES ───────────────────
        private async Task RejectConflictGroupsAsync(int approvedId, List<Reservation> conflicts)
        {
            var emailed = new HashSet<string>();

            foreach (var grp in conflicts
                     .GroupBy(c => c.ReservationGroupId ?? Guid.Empty))
            {
                foreach (var c in grp)
                {
                    c.IsRejected = true;
                    c.IsPending = false;
                    c.IsApproved = false;
                }

                var rep = grp.First();
                string day = rep.StartTime.ToString("dddd", CultureInfo.InvariantCulture);
                string time = $"{rep.StartTime:HH:mm} – {rep.EndTime:HH:mm}";
                string room = rep.Classroom?.Name ?? "the classroom";

                string body = grp.Count() > 1
                    ? $"Your group reservation for every {day} between ({time}) hours in {room} has been automatically rejected because another reservation was approved for the same time and classroom."
                    : $"Your reservation for {rep.CourseName} in {room} on {rep.StartTime:g} has been automatically rejected because another reservation was approved for the same time and classroom.";

                foreach (var c in grp.Where(c => emailed.Add(c.ApplicationUser.Email)))
                    await NotifyAsync(c.ApplicationUser.Email, "Reservation Rejected", body);

                foreach (var c in grp)
                    _logService.Log(c.ApplicationUserId, "Auto Rejection",
                        $"Reservation {c.Id} auto-rejected after approval of {approvedId}.");
            }
        }

        private async Task SendGroupNotificationAsync(IEnumerable<Reservation> reservations, bool approved)
        {
            var first = reservations.First();
            string day = first.StartTime.ToString("dddd", CultureInfo.InvariantCulture);
            string time = $"{first.StartTime:HH:mm} – {first.EndTime:HH:mm}";
            string room = first.Classroom?.Name ?? "the classroom";

            string subj = approved ? "Group Reservation Approved"
                                   : "Group Reservation Rejected";
            string body = $"Your group reservation for every {day} between ({time}) hours in {room} has been {(approved ? "approved" : "rejected")}.";

            var emailed = new HashSet<string>();
            foreach (var res in reservations.Where(r => emailed.Add(r.ApplicationUser.Email)))
                await NotifyAsync(res.ApplicationUser.Email, subj, body);
        }

        private async Task NotifyAsync(string to, string subject, string body)
        {
            try { await _emailService.SendEmailAsync(to, subject, body); }
            catch { /* İstenirse ModelState ile logla */ }
        }

        private async Task<IList<Reservation>> ReloadReservationsAsync() =>
            await _context.Reservations
                .Include(r => r.Classroom)
                .Include(r => r.ApplicationUser)
                .Include(r => r.Term)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
    }
}
