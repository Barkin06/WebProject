using ClassroomReservationSystem.Data;
using ClassroomReservationSystem.Models;

namespace ClassroomReservationSystem.Services
{
    public class LogService
    {
        private readonly ApplicationDbContext _context;

        public LogService(ApplicationDbContext context)
        {
            _context = context;
        }

        public void Log(string? userId, string action, string details)
        {
            var log = new SystemLog
            {
                UserId = userId,
                Action = action,
                Details = details
            };

            _context.SystemLogs.Add(log);
            _context.SaveChanges();
        }
    }
}
