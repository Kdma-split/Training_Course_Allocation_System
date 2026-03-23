namespace backend.Data
{
    using backend.Models;
    using Microsoft.EntityFrameworkCore;

    public static class DbInitializer
    {
        public static async Task InitializeAsync(TrainingCourseContext context)
        {
            if (await context.Roles.AnyAsync())
            {
                return;
            }

            var roles = new List<Role>
            {
                new Role { Name = "ChannelAdmin" },
                new Role { Name = "ChannelEditor" },
                new Role { Name = "ChannelViewer" },
                new Role { Name = "CourseAdmin" },
                new Role { Name = "CourseEditor" },
                new Role { Name = "CourseViewer" },
                new Role { Name = "AssignmentAdmin" },
                new Role { Name = "AssignmentEditor" },
                new Role { Name = "AssignmentViewer" }
            };

            context.Roles.AddRange(roles);
            await context.SaveChangesAsync();
        }
    }
}
