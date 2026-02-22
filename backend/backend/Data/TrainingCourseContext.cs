namespace backend.Data
{
    using backend.Models;
    using Microsoft.EntityFrameworkCore;
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
        public DbSet<ChannelApproval> ChannelApprovals { get; set; }
        public DbSet<CourseApproval> CourseApprovals { get; set; }
        public DbSet<AssignmentApproval> AssignmentApprovals { get; set; }
        public DbSet<Domain> Domains { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Channel>()
                .HasOne(c => c.CreatedBy)
                .WithMany(u => u.CreatedChannels)
                .HasForeignKey(c => c.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Channel>()
                .HasOne(c => c.Admin)
                .WithMany(u => u.AdminChannels)
                .HasForeignKey(c => c.AdminId)
                .OnDelete(DeleteBehavior.Restrict);

            //modelBuilder.Entity<ChannelUser>()
            //    .HasKey(cu => new { cu.ChannelId, cu.UserId });

            //modelBuilder.Entity<ChannelUser>()
            //    .HasOne(cu => cu.Channel)
            //    .WithMany(c => c.ChannelUsers)
            //    .HasForeignKey(cu => cu.ChannelId)
            //    .OnDelete(DeleteBehavior.Restrict);

            //modelBuilder.Entity<ChannelUser>()
            //    .HasOne(cu => cu.User)
            //    .WithMany(u => u.ChannelUsers)
            //    .HasForeignKey(cu => cu.UserId)
            //    .OnDelete(DeleteBehavior.Restrict);
        }
    }
}