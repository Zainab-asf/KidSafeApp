using KidSafeApp.Backend.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace KidSafeApp.Backend.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<UserProgress> UserProgress { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<UserSettings> UserSettings { get; set; }
        public DbSet<Lesson> Lessons { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<CourseLesson> CourseLessons { get; set; }
        public DbSet<LessonProgress> LessonProgresses { get; set; }
        public DbSet<CourseAssignment> CourseAssignments { get; set; }
        public DbSet<ClassRoom> ClassRooms { get; set; }
        public DbSet<ClassRoomStudent> ClassRoomStudents { get; set; }
        public DbSet<ClassRoomCourseAssignment> ClassRoomCourseAssignments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(e =>
            {
                e.HasIndex(u => u.Username).IsUnique();
            });

            modelBuilder.Entity<Message>(e =>
            {
                e.HasOne(m => m.ToUser).WithMany().OnDelete(DeleteBehavior.NoAction);
                e.HasOne(m => m.FromUser).WithMany().OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<Course>(e =>
            {
                e.HasOne(c => c.Teacher)
                 .WithMany()
                 .HasForeignKey(c => c.TeacherId)
                 .OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<CourseLesson>(e =>
            {
                e.HasOne(cl => cl.Course)
                 .WithMany(c => c.CourseLessons)
                 .HasForeignKey(cl => cl.CourseId)
                 .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(cl => cl.Lesson)
                 .WithMany()
                 .HasForeignKey(cl => cl.LessonId)
                 .OnDelete(DeleteBehavior.NoAction);

                e.HasIndex(cl => new { cl.CourseId, cl.LessonId }).IsUnique();
            });

            modelBuilder.Entity<LessonProgress>(e =>
            {
                e.HasOne(lp => lp.CourseLesson)
                 .WithMany(cl => cl.LessonProgressItems)
                 .HasForeignKey(lp => lp.CourseLessonId)
                 .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(lp => lp.Child)
                 .WithMany()
                 .HasForeignKey(lp => lp.ChildId)
                 .OnDelete(DeleteBehavior.NoAction);

                e.HasIndex(lp => new { lp.CourseLessonId, lp.ChildId }).IsUnique();
            });

            modelBuilder.Entity<CourseAssignment>(e =>
            {
                e.HasOne(a => a.Course)
                 .WithMany()
                 .HasForeignKey(a => a.CourseId)
                 .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(a => a.Child)
                 .WithMany()
                 .HasForeignKey(a => a.ChildId)
                 .OnDelete(DeleteBehavior.NoAction);

                e.HasIndex(a => new { a.CourseId, a.ChildId }).IsUnique();
            });

            modelBuilder.Entity<ClassRoomStudent>(e =>
            {
                e.HasOne(cs => cs.ClassRoom)
                 .WithMany(c => c.Students)
                 .HasForeignKey(cs => cs.ClassRoomId)
                 .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(cs => cs.Student)
                 .WithMany()
                 .HasForeignKey(cs => cs.StudentId)
                 .OnDelete(DeleteBehavior.NoAction);

                e.HasIndex(cs => new { cs.ClassRoomId, cs.StudentId }).IsUnique();
            });

            modelBuilder.Entity<ClassRoom>(e =>
            {
                e.HasOne(c => c.Teacher)
                 .WithMany()
                 .HasForeignKey(c => c.TeacherId)
                 .OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<ClassRoomCourseAssignment>(e =>
            {
                e.HasOne(a => a.ClassRoom)
                 .WithMany(c => c.CourseAssignments)
                 .HasForeignKey(a => a.ClassRoomId)
                 .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(a => a.Course)
                 .WithMany()
                 .HasForeignKey(a => a.CourseId)
                 .OnDelete(DeleteBehavior.NoAction);

                e.HasIndex(a => new { a.ClassRoomId, a.CourseId }).IsUnique();
            });

            modelBuilder.Entity<Lesson>().HasData(Lesson.GetSeedData());
        }
    }
}
