using EventManagement.DbContext;
using EventManagement.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Net.Http;
using Salesforce.Common.Models.Json;
using Microsoft.CodeAnalysis;
using EventManagement.Helper;

namespace EventManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly HttpClient httpClient;
        private readonly IConfiguration configuration;

        public UserController(ApplicationDbContext context, HttpClient httpClient, IConfiguration configuration)
        {
            this.context = context;
            this.httpClient = httpClient;
            this.configuration = configuration;
        }

        [HttpGet("GetAllUsers")]
        public async Task<IActionResult> GetAllUsers()
        {
            int statusCode = 200;
            List<User> users = new();
            try
            {
                users = await context.Users.AsNoTracking().Where(x => x.IsActive == true).ToListAsync();
            }
            catch (Exception)
            {
                statusCode = 500;
            }
            return StatusCode(statusCode, users);
        }
        [HttpGet("GetUserById/{uid}")]
        public async Task<IActionResult> GetUserById(int uid)
        {
            int statusCode = 200;
            User user = null;
            try
            {
                 user = await context.Users.AsNoTracking().FirstOrDefaultAsync(x => x.UserId == uid);
            }
            catch (Exception)
            {
                statusCode = 500;
            }
            return StatusCode(statusCode, user);
        }

        [HttpGet("UpdateFromSF")]
        public async Task<IActionResult> UpdateFromSF()
        {
            try
            {
                SalesforceIntegration salesforce = new SalesforceIntegration(configuration);

                // Update Users from Salesforce
                var sFUsers = await salesforce.FetchAllMembersAsync();
                int updatedCount = 0;
                int addedCount = 0;
                var allUsers = await context.Users.AsNoTracking().ToListAsync();
                foreach (var sfuser in sFUsers.Members)
                {
                    var existingUser = allUsers.FirstOrDefault(x => x.MemberId == sfuser.MemberId);

                    if (existingUser != null)
                    {
                       
                        bool isChanged = false;

                        if (!string.IsNullOrEmpty(sfuser.Address) && existingUser.Address != sfuser.Address)
                        {
                            existingUser.Address = sfuser.Address;
                            isChanged = true;
                        }

                        if (!string.IsNullOrEmpty(sfuser.Phone) && existingUser.MobileNumber != sfuser.Phone)
                        {
                            existingUser.MobileNumber = sfuser.Phone;
                            isChanged = true;
                        }

                        if (!string.IsNullOrEmpty(sfuser.Email) && existingUser.UserEmail != sfuser.Email)
                        {
                            existingUser.UserEmail = sfuser.Email;
                            isChanged = true;
                        }

                        if (!string.IsNullOrEmpty(sfuser.LifeGroupName) && existingUser.LifeGroup != sfuser.LifeGroupName)
                        {
                            existingUser.LifeGroup = sfuser.LifeGroupName;
                            isChanged = true;
                        }


                        if (isChanged)
                        {
                            existingUser.UpdatedOn = DateTime.Now;
                            existingUser.FirstName = sfuser.MemberName;
                            existingUser.LastName = "";
                            context.Users.Update(existingUser);
                            updatedCount++;
                        }
                    }

                    else
                    {
                        User newUser = new User
                        {
                            Address = sfuser.Address,
                            LifeGroup = sfuser.LifeGroupName,
                            FirstName = sfuser.MemberName,
                            MobileNumber = sfuser.Phone,
                            UserEmail = sfuser.Email,
                            CreatedOn = DateTime.Now,
                            MemberId = sfuser.MemberId,
                            Role = "User",
                            Password = "12345678",
                            IsActive = true,
                            CreatedBy = 2,
                        };
                        await context.Users.AddAsync(newUser);
                        addedCount++;
                    }
                }
                await context.SaveChangesAsync();

                // Push Events to Salesforce
                var events = await context.Events.ToListAsync();
                if (events != null)
                {
                    EventData eventData = new EventData
                    {
                        DataList = new List<EventDataList>
                {
                    new EventDataList
                    {
                        Events = events.Select(e => new SFEvent { EventId = e.EventId.ToString(), EventName = e.EventName }).ToList()
                    }
                }
                    };
                    await salesforce.PushEventDataAsync(eventData);
                }

                // Push Event Attendance to Salesforce
                var attendances = await context.Attendances.ToListAsync();
                if (attendances != null)
                {
                    AttendanceData attendanceData = new AttendanceData
                    {
                        DataList = new List<AttendanceDataList>
                {
                    new AttendanceDataList
                    {   
                        Attendances = attendances.Select(a => new SFAttendance { EventId = a.EventId.ToString(), MemberId = context.Users.Find(a.UserId)!.MemberId }).ToList()
                    }
                }
                    };
                    await salesforce.PushAttendanceDataAsync(attendanceData);
                }

                return StatusCode(200, "Success");
            }
            catch (Exception e )
            {
                return StatusCode(500, "Internal Server Error");
            }
        }

        [HttpDelete("DeleteUser/{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            int statusCode = 200;
            User? user = new();
            try
            {
                user = await context.Users.AsNoTracking().FirstOrDefaultAsync(x => x.UserId == id);
                if (user != null)
                {
                    context.Users.Remove(user);
                    
                    var attendances = await context.Attendances.Where(x => x.UserId == user.UserId).ToListAsync();
                    context.Attendances.RemoveRange(attendances);
                    var mealAttendances = await context.EventMealAttendances.Where(x => x.UserId == user.UserId).ToListAsync();
                    context.EventMealAttendances.RemoveRange(mealAttendances);

                    await context.SaveChangesAsync();
                }
            }
            catch (Exception)
            {
                statusCode = 500;
            }
            return StatusCode(statusCode, user);
        }

        [HttpPost("AddUser")]
        public async Task<IActionResult> AddUser([FromBody] User user)
        {
            try
            {
                if (user == null)
                {
                    return BadRequest(new { Status = false, Message = "User data cannot be empty" });
                }
                user.CreatedOn = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));
                user.UpdatedOn = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));
                var validation = user.Validate();
                if (validation.Count > 0)
                {
                    return BadRequest(new { Status = false, Message = validation.Aggregate((i,j) => i+"\r\n"+j) });
                }

                var memberIdExists = await context.Users.AnyAsync(x => x.MemberId == user.MemberId);
                var rfIdExists = await context.Users.AnyAsync(x => x.RfId== user.RfId);
                //var mobileExists = await context.Users.AnyAsync(x => x.MobileNumber== user.MobileNumber);

                
                //if (mobileExists)
                //{
                //    return BadRequest(new Response(false, "Mobile number Already Exists", null));
                //}
                if (rfIdExists)
                {
                    return BadRequest(new Response(false, "Rfid Already Exists", null));
                }

                User newUser = new()
                {
                    Address = user.Address,
                    CreatedBy = user.CreatedBy,
                    CreatedOn = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time")),
                    FirstName = user.FirstName,
                    IsActive = user.IsActive,
                    LastName = user.LastName,
                    LifeGroup = user.LifeGroup,
                    MemberId = user.MemberId,
                    MobileNumber = user.MobileNumber,
                    Photo = user.Photo,
                    RfId = user.RfId,
                    Role = user.Role,
                    UpdatedBy = user.UpdatedBy,
                    UpdatedOn = user.UpdatedOn,
                    UserEmail = user.UserEmail,
                    Password = "12345678    ",
                };

                await context.Users.AddAsync(newUser);
                await context.SaveChangesAsync();

                return Ok(new { Status = true, Message = "Successfully Added User" });
            }
            catch (Exception e)
            {
                return StatusCode(500, new { Status = false, Message = e.Message });
            }
        }

        public async Task<string> GetUsersAsync(string accessToken)
        {
            var query = "SELECT Id, Username, Email, FirstName, LastName, MobilePhone, IsActive, CreatedDate, CreatedById, LastModifiedDate, LastModifiedById, Street, City, State, Country, PostalCode FROM User";
            var queryUrl = $"https://thekingstemplechurch.my.salesforce.com/services/data/v41.0/query?q={Uri.EscapeDataString(query)}";

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            var response = await httpClient.GetAsync(queryUrl);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            // Deserialize the response into a list of UserInfo objects
            var usersInfo = JsonConvert.DeserializeObject<SalesforceUserResponse>(content);

            // Loop through each user to update or insert into your database
            foreach (var userInfo in usersInfo.records)
            {
                // Check if the user's email already exists in your database
                var existingUser = await context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.UserEmail == userInfo.Email && u.MobileNumber == userInfo.MobilePhone);

                if (existingUser != null)
                {
                    // Compare each field and update if different
                    if (existingUser.FirstName != userInfo.FirstName)
                        existingUser.FirstName = userInfo.FirstName;

                    if (existingUser.LastName != userInfo.LastName)
                        existingUser.LastName = userInfo.LastName;

                    if (existingUser.MobileNumber != userInfo.MobilePhone)
                        existingUser.MobileNumber = userInfo.MobilePhone;

                    if (existingUser.IsActive != userInfo.IsActive)
                        existingUser.IsActive = userInfo.IsActive;


                    DateTime updatedOn = userInfo.LastModifiedDate;
                    if (existingUser.UpdatedOn != updatedOn)
                        existingUser.UpdatedOn = updatedOn;


                    string address = $"{userInfo.Street}, {userInfo.City}, {userInfo.State}, {userInfo.Country}, {userInfo.PostalCode}";
                    if (existingUser.Address != address)
                        existingUser.Address = address;

                    context.Users.Update(existingUser);
                }
                else
                {
                    // Insert a new user if the email does not exist
                    //var newUser = new User
                    //{
                    //    FirstName = userInfo.FirstName,
                    //    LastName = userInfo.LastName,
                    //    MobileNumber = userInfo.MobilePhone,
                    //    UserEmail = userInfo.Email,
                    //    IsActive = userInfo.IsActive,
                    //    Address = $"{userInfo.Street} ,  {userInfo.City}, {userInfo.State}, {userInfo.Country}, {userInfo.PostalCode}"
                    //};

                    //context.Users.Add(newUser);
                }
            }

            await context.SaveChangesAsync();

            return "Users synced successfully";
        }

        public async Task<string> AuthenticateAsync()
        {
            var tokenUrl = configuration["SF:TokenURL"];
            var clientId = configuration["SF:ClientId"];
            var clientSecret = configuration["SF:ClientSecret"];
            var username = configuration["SF:Username"];
            var password = configuration["SF:Password"];

            var requestBody = new FormUrlEncodedContent(new[]
            {
            new KeyValuePair<string, string>("grant_type", "password"),
            new KeyValuePair<string, string>("client_id", clientId),
            new KeyValuePair<string, string>("client_secret", clientSecret),
            new KeyValuePair<string, string>("username", username),
            new KeyValuePair<string, string>("password", password)
        });

            var response = await httpClient.PostAsync(tokenUrl, requestBody);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonConvert.DeserializeObject<Dictionary<string, string>>(content);

            return tokenResponse["access_token"];
        }

        [HttpPost("UpdateUser")]
        public async Task<IActionResult> UpdateUser([FromBody] User user)
        {
            try
            {
                var oldUser = await context.Users.FindAsync(user.UserId);

                if(oldUser == null)
                {
                    return Ok(new Response(false, "User not found", null));
                }
                if (user.RfId != null && oldUser.RfId != user.RfId) oldUser.RfId = user.RfId;
                if (user.Role != null && oldUser.Role != user.Role) oldUser.Role = user.Role;
                if (user.FirstName != null && oldUser.FirstName != user.FirstName) oldUser.FirstName = user.FirstName;
                if (user.LastName != null && oldUser.LastName != user.LastName) oldUser.LastName = user.LastName;
                if (user.MobileNumber != null && oldUser.MobileNumber != user.MobileNumber) oldUser.MobileNumber = user.MobileNumber;
                if (user.UserEmail != null && oldUser.UserEmail != user.UserEmail) oldUser.UserEmail = user.UserEmail;
                if (user.IsActive.HasValue && oldUser.IsActive != user.IsActive) oldUser.IsActive = user.IsActive;
                if (user.CreatedBy.HasValue && oldUser.CreatedBy != user.CreatedBy) oldUser.CreatedBy = user.CreatedBy;
                if (user.UpdatedBy.HasValue && oldUser.UpdatedBy != user.UpdatedBy) oldUser.UpdatedBy = user.UpdatedBy;
                if (user.Address != null && oldUser.Address != user.Address) oldUser.Address = user.Address;
                if (user.LifeGroup != null && oldUser.LifeGroup != user.LifeGroup) oldUser.LifeGroup = user.LifeGroup;
                if (user.Photo != null && oldUser.Photo != user.Photo) oldUser.Photo = user.Photo;
                if (user.MemberId != null && oldUser.MemberId != user.MemberId) oldUser.MemberId = user.MemberId;

                oldUser.UpdatedOn = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));

                context.Users.Update(oldUser);
                await context.SaveChangesAsync();

                return Ok(new Response(true, "Success", oldUser));

            }
            catch (Exception ex)
            {
                return BadRequest(new Response(false, ex.Message, null));
            }
        }

    }
}
