using EventManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace EventManagement.DbContext
{
    public class ApplicationDbContext: Microsoft.EntityFrameworkCore.DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
        {
        }

        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<EventMealAttendance> EventMealAttendances { get; set; }
        public DbSet<EventMeal> EventMeals { get; set; }
        public DbSet<Events> Events { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Attendance>()
                .HasKey(a => a.AttendanceId);

            modelBuilder.Entity<EventMealAttendance>()
                .HasKey(ema => ema.MealAttendanceId);

            modelBuilder.Entity<EventMeal>()
                .HasKey(em => em.MealId);

            modelBuilder.Entity<Events>()
                .HasKey(e => e.EventId);

            modelBuilder.Entity<User>()
                .HasKey(u => u.UserId);

            modelBuilder.Entity<Attendance>().ToTable("attendance");
            modelBuilder.Entity<EventMealAttendance>().ToTable("event_meal_attendance");
            modelBuilder.Entity<EventMeal>().ToTable("event_meals");
            modelBuilder.Entity<Events>().ToTable("events");
            modelBuilder.Entity<User>().ToTable("user");

            modelBuilder.Entity<User>()
                .Property(u => u.UserId).HasColumnName("user_id");

            modelBuilder.Entity<User>()
                .Property(u => u.RfId).HasColumnName("rf_id");

            modelBuilder.Entity<User>()
                .Property(u => u.Role).HasColumnName("role");

            modelBuilder.Entity<User>()
                .Property(u => u.FirstName).HasColumnName("first_name");

            modelBuilder.Entity<User>()
                .Property(u => u.LastName).HasColumnName("last_name");

            modelBuilder.Entity<User>()
                .Property(u => u.MobileNumber).HasColumnName("mobile_number");

            modelBuilder.Entity<User>()
                .Property(u => u.UserEmail).HasColumnName("user_email");

            modelBuilder.Entity<User>()
                .Property(u => u.Password).HasColumnName("password");

            modelBuilder.Entity<User>()
                .Property(u => u.IsActive).HasColumnName("is_active");

            modelBuilder.Entity<User>()
                .Property(u => u.CreatedOn).HasColumnName("created_on");

            modelBuilder.Entity<User>()
                .Property(u => u.CreatedBy).HasColumnName("created_by");

            modelBuilder.Entity<User>()
                .Property(u => u.UpdatedOn).HasColumnName("updated_on");

            modelBuilder.Entity<User>()
                .Property(u => u.UpdatedBy).HasColumnName("updated_by");

            modelBuilder.Entity<User>()
                .Property(u => u.Address).HasColumnName("address");

            modelBuilder.Entity<User>()
                .Property(u => u.LifeGroup).HasColumnName("life_group");

            modelBuilder.Entity<User>()
                .Property(u => u.Photo).HasColumnName("photo");

            modelBuilder.Entity<User>()
                .Property(u => u.MemberId).HasColumnName("member_id");

            modelBuilder.Entity<Attendance>()
                .Property(a => a.AttendanceId).HasColumnName("attendance_id");

            modelBuilder.Entity<Attendance>()
                .Property(a => a.UserId).HasColumnName("user_id");

            modelBuilder.Entity<Attendance>()
                .Property(a => a.EventId).HasColumnName("event_id");

            modelBuilder.Entity<Attendance>()
                .Property(a => a.PunchTime).HasColumnName("punch_time");

            modelBuilder.Entity<Attendance>()
                .Property(a => a.IsActive).HasColumnName("is_active");

            modelBuilder.Entity<Attendance>()
                .Property(a => a.CreatedOn).HasColumnName("created_on");

            modelBuilder.Entity<Attendance>()
                .Property(a => a.CreatedBy).HasColumnName("created_by");

            modelBuilder.Entity<Attendance>()
                .Property(a => a.UpdatedOn).HasColumnName("updated_on");

            modelBuilder.Entity<Attendance>()
                .Property(a => a.UpdatedBy).HasColumnName("updated_by");

            modelBuilder.Entity<EventMealAttendance>()
               .Property(e => e.MealAttendanceId).HasColumnName("meal_attendance_id");

            modelBuilder.Entity<EventMealAttendance>()
                .Property(e => e.EventId).HasColumnName("event_id");

            modelBuilder.Entity<EventMealAttendance>()
                .Property(e => e.UserId).HasColumnName("user_id");

            modelBuilder.Entity<EventMealAttendance>()
                .Property(e => e.MealId).HasColumnName("meal_id");

            modelBuilder.Entity<EventMealAttendance>()
                .Property(e => e.IsActive).HasColumnName("is_active");

            modelBuilder.Entity<EventMealAttendance>()
                .Property(e => e.CreatedOn).HasColumnName("created_on");

            modelBuilder.Entity<EventMealAttendance>()
                .Property(e => e.CreatedBy).HasColumnName("created_by");

            modelBuilder.Entity<EventMealAttendance>()
                .Property(e => e.UpdatedOn).HasColumnName("updated_on");

            modelBuilder.Entity<EventMealAttendance>()
                .Property(e => e.UpdatedBy).HasColumnName("updated_by");

            modelBuilder.Entity<EventMealAttendance>()
                .Property(e => e.PunchTime).HasColumnName("punch_time");

            modelBuilder.Entity<Events>()
                .Property(e => e.EventId).HasColumnName("event_id");

            modelBuilder.Entity<Events>()
                .Property(e => e.EventName).HasColumnName("event_name");

            modelBuilder.Entity<Events>()
                .Property(e => e.EventLocation).HasColumnName("event_location");

            modelBuilder.Entity<Events>()
                .Property(e => e.EventStartTime).HasColumnName("event_start_time");

            modelBuilder.Entity<Events>()
                .Property(e => e.EventEndTime).HasColumnName("event_end_time");

            modelBuilder.Entity<Events>()
                .Property(e => e.IsActive).HasColumnName("is_active");

            modelBuilder.Entity<Events>()
                .Property(e => e.CreatedOn).HasColumnName("created_on");

            modelBuilder.Entity<Events>()
                .Property(e => e.CreatedBy).HasColumnName("created_by");

            modelBuilder.Entity<Events>()
                .Property(e => e.UpdatedOn).HasColumnName("updated_on");

            modelBuilder.Entity<Events>()
                .Property(e => e.UpdatedBy).HasColumnName("updated_by");

            modelBuilder.Entity<Events>()
                .Property(e => e.Photo).HasColumnName("event_photo");

            modelBuilder.Entity<EventMeal>().Property(e => e.MealType).HasColumnName("meal_type");
            modelBuilder.Entity<EventMeal>().Property(e => e.EventId).HasColumnName("event_id");
            modelBuilder.Entity<EventMeal>().Property(e => e.UpdatedOn).HasColumnName("updated_on");
            modelBuilder.Entity<EventMeal>().Property(e => e.CreatedBy).HasColumnName("created_by");
            modelBuilder.Entity<EventMeal>().Property(e => e.CreatedOn).HasColumnName("created_on");
            modelBuilder.Entity<EventMeal>().Property(e => e.IsActive).HasColumnName("is_active");
            modelBuilder.Entity<EventMeal>().Property(e => e.MealId).HasColumnName("meal_id");
            modelBuilder.Entity<EventMeal>().Property(e => e.MealName).HasColumnName("meal_name");
            modelBuilder.Entity<EventMeal>().Property(e => e.MealTimeFrom).HasColumnName("meal_time_from");
            modelBuilder.Entity<EventMeal>().Property(e => e.MealTimeTo).HasColumnName("meal_time_to");
            modelBuilder.Entity<EventMeal>().Property(e => e.UpdatedBy).HasColumnName("updated_by");
        }
    }
}
