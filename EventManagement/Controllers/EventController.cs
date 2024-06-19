using EventManagement.DbContext;
using EventManagement.Helper;
using EventManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EventManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class EventController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly UserManager<User> userManager;

        public EventController(ApplicationDbContext context,UserManager<User> userManager)
        {
            this.context = context;
            this.userManager = userManager;
        }

        [HttpGet("GetAllEventsWithMealCount")]
        public async Task<IActionResult> GetAllEventsWithMealCount()
        {
            try
            {
                var eventsWithMealCounts = await context.Events
                    .AsNoTracking()
                    .Where(x => x.IsActive == true)
                    .Select(e => new EventsWithMealCount
                    {
                        EventId = e.EventId,
                        EventName = e.EventName,
                        EventLocation = e.EventLocation,
                        EventStartTime = e.EventStartTime,
                        EventEndTime = e.EventEndTime,
                        IsActive = e.IsActive,
                        MealCount = context.EventMeals.Count(em => em.EventId == e.EventId),
                        CreatedOn = e.CreatedOn,
                        CreatedByName = HttpContext.Session.Get<User>("UserData").FirstName,
                        UpdatedOn = e.UpdatedOn,
                        UpdatedBy = e.UpdatedBy
                    })
                    .ToListAsync();

                return Ok(eventsWithMealCounts);
            }
            catch (Exception)
            {
                return StatusCode(500, new List<EventsWithMealCount>());
            }
        }

        [HttpGet("GetEvenWithMeals/{id}")]
        public async Task<IActionResult> GetEvenWithMeals(int id)
        {
            try
            {
                var e = await context.Events.FindAsync(id);
                if (e == null)
                {
                    return BadRequest(new Response(false, "Event Not Found"));
                }

                var eventMeals = await context.EventMeals.Where(x => x.EventId == id).ToListAsync();

                var eventsWithMeals = new EventsWithMeals
                {
                    EventId = e.EventId,
                    EventName = e.EventName,
                    EventLocation = e.EventLocation,
                    EventStartTime = e.EventStartTime,
                    EventEndTime = e.EventEndTime,
                    IsActive = e.IsActive,
                    Meals = eventMeals,
                    CreatedOn = e.CreatedOn,
                    CreatedByName = context.Users.Find(e.CreatedBy.ToString())?.FirstName,
                    UpdatedOn = e.UpdatedOn,
                    UpdatedBy = e.UpdatedBy,
                    Photo = e.Photo,
                };

                return Ok(new Response(true, "Success", eventsWithMeals));
            }
            catch (Exception ex)
            {
                return BadRequest(new Response(false, ex.Message));
            }
        }

        [HttpPost("AddEvent")]
        public async Task<IActionResult> AddEvent([FromBody] EventsWithMeals e)
        {
            try
            {
                if (e == null)
                {
                    return BadRequest(new Response(false, "Event data cannot be empty", null));
                }

                var eventEntity = new Events
                {   
                    CreatedBy = Convert.ToInt32(e.CreatedByName),
                    CreatedOn = DateTime.Now,
                    EventEndTime = e.EventEndTime,
                    EventStartTime = e.EventStartTime,
                    EventLocation = e.EventLocation,
                    IsActive = true,
                    EventName = e.EventName,
                    UpdatedBy = Convert.ToInt32(e.CreatedByName),
                    UpdatedOn = DateTime.Now,
                    Photo = e.Photo
                };

                await context.Events.AddAsync(eventEntity);
                await context.SaveChangesAsync();

                foreach (var meal in e.Meals)
                {

                    meal.EventId = eventEntity.EventId;
                    meal.CreatedBy = eventEntity.CreatedBy;
                    meal.UpdatedBy = eventEntity.UpdatedBy;
                    meal.CreatedOn = eventEntity.CreatedOn;
                    meal.UpdatedOn = eventEntity.UpdatedOn;

                    if(meal.MealName == null)
                    {
                        continue;
                    }

                    EventMeal m = new EventMeal
                    {
                        CreatedBy = eventEntity.CreatedBy,
                        CreatedOn = eventEntity.CreatedOn,
                        EventId = eventEntity.EventId,
                        MealName = meal.MealName,
                        IsActive = true,
                        MealTimeFrom = meal.MealTimeFrom,
                        MealTimeTo = meal.MealTimeTo,
                        MealType = meal.MealType,
                    };
                    await context.EventMeals.AddAsync(m);
                }

                await context.SaveChangesAsync();

                return Ok(new Response(true, "Success", eventEntity));
            }
            catch (Exception ex)
            {
                return BadRequest(new Response(false, ex.Message, null));
            }
        }

        [HttpGet("GetEventAndMealsAttendanceByUserId/{emid}/{uid}")]
        public async Task<IActionResult> GetEventAndMealsAttendanceByUserId(int emid, string uid)
        {
            try
            {
                var em = await context.EventMeals.FindAsync(emid);
                var u = await context.Users.FindAsync(uid);

                if (em == null || u == null)
                {
                    return BadRequest(new Response(false, "User / Meal Does not exist."));
                }

                var e = await context.Events.FindAsync(em.EventId);

                if (DateTime.Now < e.EventStartTime || DateTime.Now > e.EventEndTime)
                {
                    return Ok(new Response(false, "Meal cannot be attended outside of the event's time.", null));
                }

                var eventMealAttendance = await context.EventMealAttendances
                    .FirstOrDefaultAsync(x => x.EventId == e.EventId && x.MealId == emid && x.UserId == uid);

                if (eventMealAttendance == null)
                {
                    var mealAttendance = new EventMealAttendance
                    {
                        CreatedBy = Convert.ToInt32(uid),
                        CreatedOn = DateTime.Now,
                        EventId = e.EventId,
                        IsActive = true,
                        MealId = em.MealId,
                        PunchTime = DateTime.Now,
                        UpdatedBy = Convert.ToInt32(uid),
                        UpdatedOn = DateTime.Now,
                        UserId = uid
                    };

                    await context.EventMealAttendances.AddAsync(mealAttendance);
                    await context.SaveChangesAsync();

                    return Ok(new Response(true, "Success", null));
                }
                return Ok(new Response(false, "You have already attended this meal.", null));
            }
            catch (Exception ex)
            {
                return BadRequest(new Response(false, ex.Message, null));
            }
        }

        [HttpGet("GetEventAttendanceByUserId/{eid}/{uid}")]
        public async Task<IActionResult> GetEventAttendanceByUserId(int eid, string uid)
        {
            try
            {
                var e = await context.Events.FindAsync(eid);
                var u = await context.Users.FindAsync(uid);

                if (e == null || u == null)
                {
                    return BadRequest(new Response(false, "User / Event Does not exist."));
                }

                if (DateTime.Now < e.EventStartTime || DateTime.Now > e.EventEndTime)
                {
                    return Ok(new Response(false, "Event cannot be attended outside of its time.", null));
                }

                var eventAttendance = await context.Attendances
                    .FirstOrDefaultAsync(x => x.EventId == e.EventId && x.UserId == uid);

                if (eventAttendance == null)
                {
                    var attendance = new Attendance
                    {
                        UserId = uid,
                        CreatedBy = Convert.ToInt32(uid),
                        CreatedOn = DateTime.Now,
                        EventId = e.EventId,
                        IsActive = true,
                        PunchTime = DateTime.Now,
                        UpdatedBy = Convert.ToInt32(uid),
                        UpdatedOn = DateTime.Now
                    };

                    await context.Attendances.AddAsync(attendance);
                    await context.SaveChangesAsync();
                    return Ok(new Response(true, "Success", null));
                }

                return Ok(new Response(false, "You have already attended this event.", null));
            }
            catch (Exception ex)
            {
                return BadRequest(new Response(false, ex.Message, null));
            }
        }

        [HttpGet("GetAllAttendanceByEventId/{eid}")]
        public async Task<IActionResult> GetAllAttendanceByEventId(int eid)
        {
            try
            {
                var eventDetails = await context.Events
                    .AsNoTracking()
                    .Where(x => x.EventId == eid)
                    .Select(e => new
                    {
                        e.EventId,
                        e.EventStartTime,
                        e.EventEndTime
                    })
                    .FirstOrDefaultAsync();

                if (eventDetails == null)
                {
                    return Ok(new Response(false, "Event not found", null));
                }

                // Retrieve attendances and corresponding user details
                var attendances = await context.Attendances
                    .AsNoTracking()
                    .Where(a => a.EventId == eid)
                    .Join(context.Users,
                        a => a.UserId,
                        u => u.UserId,
                        (a, u) => new
                        {
                            a.PunchTime,
                            UserName = u.FirstName + " " + u.LastName
                        })
                    .ToListAsync();

                if (!attendances.Any())
                {
                    return Ok(new Response(false, "No attendance for this event", null));
                }

                var eventAttendance = new EventAttendance
                {
                    EventId = eventDetails.EventId,
                    EventDate = eventDetails.EventStartTime,
                    UserDetails = attendances.Select(a => new UserDetail
                    {
                        PunchIn = a.PunchTime,
                        PunchOut = a.PunchTime,
                        UserName = a.UserName
                    }).ToList()
                };

                return Ok(new Response(true, "Success", new List<EventAttendance> { eventAttendance }));
            }
            catch (Exception ex)
            {
                return BadRequest(new Response(false, ex.Message, null));
            }
        }

        [HttpGet("GetAllMealAttendanceByEventId/{eid}")]
        public async Task<IActionResult> GetAllMealAttendanceByEventId(int eid)
        {
            try
            {
                var eventDetails = await context.Events
                    .AsNoTracking()
                    .Where(x => x.EventId == eid)
                    .Select(e => new
                    {
                        e.EventId,
                        e.EventStartTime,
                        e.EventEndTime
                    })
                    .FirstOrDefaultAsync();

                if (eventDetails == null)
                {
                    return Ok(new Response(false, "Event not found", null));
                }

                // Retrieve attendances and corresponding user details
                var attendances = await context.EventMealAttendances
                    .AsNoTracking()
                    .Where(a => a.EventId == eid && a.PunchTime >= eventDetails.EventStartTime && a.PunchTime <= eventDetails.EventEndTime)
                    .Join(context.Users,
                        a => a.UserId,
                        u => u.UserId,
                        (a, u) => new
                        {
                            a.PunchTime,
                            UserName =u.FirstName + " " + u.LastName 
                        })
                    .ToListAsync();

                if (!attendances.Any())
                {
                    return Ok(new Response(false, "No meal attendance for this event", null));
                }

                var eventAttendance = new EventAttendance
                {
                    EventId = eventDetails.EventId,
                    EventDate = eventDetails.EventStartTime,
                    UserDetails = attendances.Select(a => new UserDetail
                    {
                        PunchIn = a.PunchTime,
                        PunchOut = a.PunchTime,
                        UserName = a.UserName
                    }).ToList()
                };

                return Ok(new Response(true, "Success", new List<EventAttendance> { eventAttendance }));
            }
            catch (Exception ex)
            {
                return BadRequest(new Response(false, ex.Message, null));
            }
        }
        [HttpGet("GetAllMealAttendanceByEventId/{eid}/{mid}")]
        public async Task<IActionResult> GetAllMealAttendanceByEventId(int eid, int mid)
        {
            try
            {
                var eventDetails = await context.Events
                    .AsNoTracking()
                    .Where(x => x.EventId == eid)
                    .Select(e => new
                    {
                        e.EventId,
                        e.EventStartTime,
                        e.EventEndTime
                    })
                    .FirstOrDefaultAsync();

                if (eventDetails == null)
                {
                    return Ok(new Response(false, "Event not found", null));
                }

                // Retrieve attendances and corresponding user details
                var attendances = await context.EventMealAttendances
                    .AsNoTracking()
                    .Where(a => a.MealId == mid && a.EventId == eid)
                    .Join(context.Users,
                        a => a.UserId,
                        u => u.UserId,
                        (a, u) => new
                        {
                            a.PunchTime,
                            UserName = u.FirstName + " " + u.LastName
                        })
                    .ToListAsync();

                if (!attendances.Any())
                {
                    return Ok(new Response(false, "No meal attendance for this event", null));
                }

                var eventAttendance = new EventAttendance
                {
                    EventId = eventDetails.EventId,
                    EventDate = eventDetails.EventStartTime,
                    UserDetails = attendances.Select(a => new UserDetail
                    {
                        PunchIn = a.PunchTime,
                        PunchOut = a.PunchTime,
                        UserName = a.UserName
                    }).ToList()
                };

                return Ok(new Response(true, "Success", new List<EventAttendance> { eventAttendance }));
            }
            catch (Exception ex)
            {
                return BadRequest(new Response(false, ex.Message, null));
            }
        }
        [HttpPut("EditEvent")]
        public async Task<IActionResult> EditEvent([FromBody] EventsWithMeals updatedEvent)
        {
            try
            {
                var oldEvent = await context.Events.FindAsync(updatedEvent.EventId);

                if (oldEvent == null)
                {
                    return Ok(new Response(false, "Event not found", null));
                }

                if (updatedEvent.EventName != null && oldEvent.EventName != updatedEvent.EventName) oldEvent.EventName = updatedEvent.EventName;
                if (updatedEvent.EventLocation != null && oldEvent.EventLocation != updatedEvent.EventLocation) oldEvent.EventLocation = updatedEvent.EventLocation;
                if (updatedEvent.EventStartTime.HasValue && oldEvent.EventStartTime != updatedEvent.EventStartTime) oldEvent.EventStartTime = updatedEvent.EventStartTime;
                if (updatedEvent.EventEndTime.HasValue && oldEvent.EventEndTime != updatedEvent.EventEndTime) oldEvent.EventEndTime = updatedEvent.EventEndTime.Value;
                if (updatedEvent.IsActive.HasValue && oldEvent.IsActive != updatedEvent.IsActive) oldEvent.IsActive = updatedEvent.IsActive;

                oldEvent.Photo = updatedEvent.Photo;
                oldEvent.UpdatedOn = DateTime.Now;

                context.Events.Update(oldEvent);

                foreach (var updatedMeal in updatedEvent.Meals)
                {
                    var oldMeal = await context.EventMeals.FindAsync(updatedMeal.MealId);
                    if (oldMeal != null)
                    {
                        if (updatedMeal.MealType != null && oldMeal.MealType != updatedMeal.MealType) oldMeal.MealType = updatedMeal.MealType;
                        if (updatedMeal.MealName != null && oldMeal.MealName != updatedMeal.MealName) oldMeal.MealName = updatedMeal.MealName;
                        if (updatedMeal.MealTimeFrom.HasValue && oldMeal.MealTimeFrom != updatedMeal.MealTimeFrom) oldMeal.MealTimeFrom = updatedMeal.MealTimeFrom;
                        if (updatedMeal.MealTimeTo.HasValue && oldMeal.MealTimeTo != updatedMeal.MealTimeTo) oldMeal.MealTimeTo = updatedMeal.MealTimeTo;
                        if (updatedMeal.IsActive.HasValue && oldMeal.IsActive != updatedMeal.IsActive) oldMeal.IsActive = updatedMeal.IsActive;
                        oldMeal.UpdatedOn = DateTime.Now;
                        if (updatedMeal.UpdatedBy.HasValue && oldMeal.UpdatedBy != updatedMeal.UpdatedBy) oldMeal.UpdatedBy = updatedMeal.UpdatedBy;

                        context.EventMeals.Update(oldMeal);
                    }
                    else
                    {
                        EventMeal meal = new()
                        {
                            CreatedBy = Convert.ToInt32(updatedEvent.CreatedByName),
                            CreatedOn = updatedEvent.CreatedOn,
                            EventId = updatedEvent.EventId,
                            MealName = updatedMeal.MealName,
                            IsActive = true,
                            MealTimeFrom = updatedMeal.MealTimeFrom,
                            MealTimeTo = updatedMeal.MealTimeTo,
                            MealType = updatedMeal.MealType,
                        };
                        await context.EventMeals.AddAsync(meal);
                    }
                }
                await context.SaveChangesAsync();

                return Ok(new Response(true, "Event updated successfully", oldEvent));
            }
            catch (Exception ex)
            {
                return BadRequest(new Response(false, ex.Message, null));
            }
        }

        [HttpPut("MarkEventAttendance/{rfid}/{eid}")]
        public async Task<IActionResult> MarkEventAttendance(string rfid, int eid)
        {
            try
            {
                if (await context.Users.AnyAsync(x => x.RfId == rfid))
                {
                    var u = await context.Users.FirstOrDefaultAsync(x => x.RfId == rfid);
                    var e = await context.Events.FindAsync(eid);
                    if (e == null)
                    {
                        return Ok(new Response(false, "Invaid Event.", null));
                    }
                    var attendanceExist = await context.Attendances.AnyAsync(x => x.EventId == eid && x.UserId == u.UserId);
                    if (attendanceExist)
                    {
                        return Ok(new Response(false, "Attendance already captured.", null));
                    }

                    Attendance eventAttendance = new Attendance
                    {
                        CreatedOn = DateTime.UtcNow,
                        EventId = eid,
                        IsActive = true,
                        PunchTime = DateTime.UtcNow,
                        UserId = u.UserId,
                        UpdatedOn = DateTime.UtcNow,
                    };

                    await context.Attendances.AddAsync(eventAttendance);
                    await context.SaveChangesAsync();
                    return Ok(new Response(true, "Attendance Marked Successfully", u));
                }
                return Ok(new Response(false, "Please Scan again.", null));
            }
            catch (Exception ex)
            {
                return BadRequest(new Response(false, ex.Message, null));
            }
        }

        [HttpPut("MarkEventAttendance/{rfid}/{eid}/{mid}")]
        public async Task<IActionResult> MarkEventAttendance(string rfid, int eid, int mid)
        {
            try
            {
                if (await context.Users.AnyAsync(x => x.RfId == rfid))
                {
                    var u = await context.Users.FirstOrDefaultAsync(x => x.RfId == rfid);
                    var e = await context.Events.FindAsync(eid);
                    if (e == null)
                    {
                        return Ok(new Response(false, "Please scan again.", null));
                    }
                    var attendanceExist = await context.EventMealAttendances.AnyAsync(x => x.EventId == eid && x.UserId == u.UserId && x.MealId == mid);
                    if (attendanceExist)
                    {
                        return Ok(new Response(false, "Meal already served.", null));
                    }

                    EventMealAttendance eventMealAttendance = new EventMealAttendance
                    {
                        CreatedBy = 2,
                        CreatedOn = DateTime.UtcNow,
                        EventId = eid,
                        IsActive = true,
                        PunchTime = DateTime.UtcNow,
                        UserId = u.UserId,
                        UpdatedOn = DateTime.UtcNow,
                        UpdatedBy = 2,
                        MealId = mid,
                    };

                    await context.EventMealAttendances.AddAsync(eventMealAttendance);
                    await context.SaveChangesAsync();
                    return Ok(new Response(true, "Enjoy your meal.", u));
                }
                return Ok(new Response(false, "Invaid User.", null));
            }
            catch (Exception ex)
            {
                return BadRequest(new Response(false, ex.Message, null));
            }
        }


        [HttpGet("GetEventAndMealsbyIds/{eid}/{mid}")]
        public async Task<IActionResult> GetEvenWithMeals(int eid, int mid)
        {
            try
            {
                var e = await context.Events.FindAsync(eid);
                if (e == null)
                {
                    return BadRequest(new Response(false, "Event Not Found"));
                }

                var eventMeals = await context.EventMeals.FirstOrDefaultAsync(x => x.MealId == mid);

                if (eventMeals == null)
                {
                    return BadRequest(new Response(false, "Meal Not Found"));
                }

                var eventsWithMeals = new EventsWithMeals
                {
                    EventId = e.EventId,
                    EventName = e.EventName,
                    EventLocation = e.EventLocation,
                    EventStartTime = e.EventStartTime,
                    EventEndTime = e.EventEndTime,
                    IsActive = e.IsActive,
                    Meals = new List<EventMeal> { eventMeals },
                    CreatedOn = e.CreatedOn,
                    CreatedByName = HttpContext.Session.Get<User>("UserData").FirstName,
                    UpdatedOn = e.UpdatedOn,
                    UpdatedBy = e.UpdatedBy,
                    Photo = e.Photo,
                };

                return Ok(new Response(true, "Success", eventsWithMeals));
            }
            catch (Exception ex)
            {
                return BadRequest(new Response(false, ex.Message));
            }
        }
        [HttpDelete("DeleteEvent/{eid}")]
        public async Task<IActionResult> DeleteEvent(int eid)
        {
            try
            {
                var events = await context.Events.FindAsync(eid);
                if (events != null)
                {
                    var meals = await context.EventMeals.Where(x => x.EventId == eid).ToListAsync();
                    var eventAttendance = await context.Attendances.Where(x => x.EventId == eid).ToListAsync();

                    if (meals != null)
                    {
                        var mealAttendance = await context.EventMealAttendances.Where(x => x.EventId == eid).ToListAsync();
                        if (mealAttendance != null)
                        {
                            context.EventMealAttendances.RemoveRange(mealAttendance);
                        }
                        context.EventMeals.RemoveRange(meals);
                        if (eventAttendance != null)
                        {
                            context.Attendances.RemoveRange(eventAttendance);
                        }
                    }
                    context.Events.RemoveRange(events);
                    await context.SaveChangesAsync();
                }
                return Ok(new Response(false, "Event Does not exists.", null));
            }
            catch (Exception ex)
            {
                return BadRequest(new Response(false, ex.Message));
            }
        }
    }
}
