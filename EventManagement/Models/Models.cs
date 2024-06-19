using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;

namespace EventManagement.Models
{
    public class Attendance
    {
        public int AttendanceId { get; set; }
        public string? UserId { get; set; }
        public int? EventId { get; set; }
        public DateTime? PunchTime { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? CreatedOn { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public int? UpdatedBy { get; set; }
    }
    public class EventMealAttendance
    {
        public int MealAttendanceId { get; set; }
        public int? EventId { get; set; }
        public string? UserId { get; set; }
        public int? MealId { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? CreatedOn { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public int? UpdatedBy { get; set; }
        public DateTime? PunchTime { get; set; }

    }
    public class EventMeal
    {
        public int MealId { get; set; }
        public int? EventId { get; set; }
        public string? MealType { get; set; }
        public string? MealName { get; set; }
        public DateTime? MealTimeFrom { get; set; }
        public DateTime? MealTimeTo { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? CreatedOn { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public int? UpdatedBy { get; set; }
    }
    public class Events
    {
        public int EventId { get; set; }
        public string EventName { get; set; }
        public string EventLocation { get; set; }
        public string? Photo { get; set; }
        public DateTime? EventStartTime { get; set; }
        public DateTime? EventEndTime { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? CreatedOn { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public int? UpdatedBy { get; set; }
    }
    public class EventsWithMealCount
    {
        public int EventId { get; set; }
        public string EventName { get; set; }
        public string EventLocation { get; set; }
        public DateTime? EventStartTime { get; set; }
        public DateTime? EventEndTime { get; set; }
        public bool? IsActive { get; set; }
        public int MealCount { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string? CreatedByName { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public int? UpdatedBy { get; set; }
    }
    public class EventsWithMeals
    {
        public int EventId { get; set; }
        public string EventName { get; set; }
        public string EventLocation { get; set; }
        public DateTime? EventStartTime { get; set; }
        public DateTime? EventEndTime { get; set; }
        public bool? IsActive { get; set; }
        public List<EventMeal> Meals { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string? CreatedByName { get; set; }
        public string? Photo { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public int? UpdatedBy { get; set; }
    }
    public class UserDTO
    {
        public string MemberId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MobileNumber { get; set; }
        public string UserEmail { get; set; }
        public string RfId { get; set; }
        public string LifeGroup { get; set; }
        public string Address { get; set; }
        public DateTime CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public string Photo { get; set; }
        public string UserId { get; set; } // Assuming UserId is also a field in your data
    }
    public class User:IdentityUser
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string UserId { get; set; }
        public string? RfId { get; set; }
        public string? Role { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? MobileNumber { get; set; }
        public string? UserEmail { get; set; }
        public string? Password { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? CreatedOn { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public int? UpdatedBy { get; set; }
        public string? Address { get; set; }
        public string? LifeGroup { get; set; }
        public string? Photo { get; set; }
        public string? MemberId { get; set; }
    }
    public static class UserValidator
    {
        public static List<string> Validate(this User user)
        {
            List<string> validationErrors = new List<string>();


            if (string.IsNullOrWhiteSpace(user.RfId))
                validationErrors.Add("RfId cannot be empty ");

            if (validationErrors.Count == 0 && !(user.RfId.Length == 8 || user.RfId.Length == 14))
                validationErrors.Add("RFID length should be equal to 14/8.");

            if (string.IsNullOrWhiteSpace(user.Role))
                validationErrors.Add("Role cannot be empty.");

            if (string.IsNullOrWhiteSpace(user.FirstName))
                validationErrors.Add("FirstName cannot be empty.");

            if (string.IsNullOrWhiteSpace(user.LastName))
                validationErrors.Add("LastName cannot be empty.");

            if (string.IsNullOrWhiteSpace(user.MobileNumber))
                validationErrors.Add("MobileNumber cannot be empty.");
            else if (!Regex.IsMatch(user.MobileNumber, @"^\d{10}$"))  // Example for 10-digit mobile number validation
                validationErrors.Add("MobileNumber must be a valid 10-digit number.");

            //if (string.IsNullOrWhiteSpace(user.UserEmail))
            //    validationErrors.Add("UserEmail cannot be empty.");

            else if (!Regex.IsMatch(user.UserEmail, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                validationErrors.Add("UserEmail must be a valid email address.");

            if (user.IsActive == null)
                validationErrors.Add("IsActive cannot be null.");

            if (user.CreatedOn == null)
                validationErrors.Add("CreatedOn cannot be null.");

            if (user.CreatedBy == null || user.CreatedBy <= 0)
                validationErrors.Add("CreatedBy must be greater than 0.");

            if (user.UpdatedOn != null && user.UpdatedBy == null)
                validationErrors.Add("UpdatedBy cannot be null if UpdatedOn is provided.");

            if (user.UpdatedBy != null && user.UpdatedBy <= 0)
                validationErrors.Add("UpdatedBy must be greater than 0.");

            if (string.IsNullOrWhiteSpace(user.Address))
                validationErrors.Add("Address cannot be empty.");

            //if (string.IsNullOrWhiteSpace(user.LifeGroup))
            //    validationErrors.Add("LifeGroup cannot be empty.");

            if (string.IsNullOrWhiteSpace(user.MemberId))
                validationErrors.Add("MemberId cannot be empty.");

            return validationErrors;
        }
    }
    public class SalesforceUserResponse
    {
        public int totalSize { get; set; }
        public bool done { get; set; }
        public List<UserRecord> records { get; set; }
    }
    public class UserRecord
    {
        public Attributes attributes { get; set; }
        public string Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MobilePhone { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedById { get; set; }
        public DateTime LastModifiedDate { get; set; }
        public string LastModifiedById { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string PostalCode { get; set; }
    }
    public class Attributes
    {
        public string type { get; set; }
        public string url { get; set; }
    }
    public class EventAttendance
    {
        public int EventId { get; set; }
        public DateTime? EventDate { get; set; }
        public List<UserDetail> UserDetails { get; set; }
    }
    public class UserDetail
    {
        public string UserName { get; set; }
        public DateTime? PunchIn { get; set; }
        public DateTime? PunchOut { get; set; }
    }
}
