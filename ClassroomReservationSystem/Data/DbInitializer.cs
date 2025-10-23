//I asked GPT:
//please embbed the spring term between 17.02.2025 - 15.06.2025 and L111, NA01, MA05 into the database so i use them for everything
//without changing them. (like initial database)
using ClassroomReservationSystem.Models;

namespace ClassroomReservationSystem.Data
{
    public static class DbInitializer
    {
        public static void Seed(ApplicationDbContext context)
        {
            if (!context.Terms.Any())
            {
                context.Terms.Add(new Term
                {
                    Name = "Spring 2025",
                    StartDate = new DateTime(2025, 2, 17),
                    EndDate = new DateTime(2025, 6, 15),
                    IsActive = true
                });
                context.SaveChanges();
            }

            var springTerm = context.Terms.First(t => t.Name.Contains("Spring"));

            if (!context.Classrooms.Any())
            {
                var classrooms = new List<Classroom>
                {
                    new Classroom { Name = "L111", Capacity = 50, TermId = springTerm.TermId },
                    new Classroom { Name = "NA01", Capacity = 80, TermId = springTerm.TermId },
                    new Classroom { Name = "MA05", Capacity = 40, TermId = springTerm.TermId }
                };

                context.Classrooms.AddRange(classrooms);
                context.SaveChanges();
            }
        }
    }
}
