using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ClassroomReservationSystem.Data;
using ClassroomReservationSystem.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace ClassroomReservationSystem.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class ViewContactMessagesModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public ViewContactMessagesModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<Contact> Messages { get; set; } = new();

        public async Task OnGetAsync()
        {
            Messages = await _context.Contacts
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }
    }
}
