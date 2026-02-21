namespace backend.Data
{
    using Microsoft.EntityFrameworkCore;
    using backend.Models;
    public class TrainingCourseContext : DbContext
    {
        public TrainingCourseContext(DbContextOptions<TrainingCourseContext> options) : base(options)
        {
        }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Channel> Channels { get; set; }
        public DbSet<Assignment> Assignments { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<ChannelUser> ChannelUsers { get; set; }
        public DbSet<CourseChannelUser> CourseChannelUsers { get; set; }
        public DbSet<ChannelCourse> ChannelCourses { get; set; }
        public DbSet<AssignmentChannelUser> AssignmentChannelUsers { get; set; }
        public DbSet<ChannelAssignment> ChannelAssignments { get; set; }
        public DbSet<Domain> Domains { get; set; }
    }
}