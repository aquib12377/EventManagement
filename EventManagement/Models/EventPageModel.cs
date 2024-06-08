using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using EventManagement.Helper;

namespace EventManagement.Models
{
    public class EventPageModel : PageModel
    {
        private readonly HttpClient _httpClient;

        public EventPageModel(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        [BindProperty]
        public EventsWithMeals Event { get; set; }

        public async Task OnGetAsync(int? id)
        {
            if (id.HasValue && id.Value != 0)
            {
                var response = await _httpClient.GetAsync($"/api/GetEvenWithMeals/{id.Value}");
                if (response.IsSuccessStatusCode)
                {
                    var responseData = await response.Content.ReadFromJsonAsync<Response>();
                    if (responseData.Status)
                    {
                        var e = responseData.Payload as EventsWithMeals;
                        Event = new EventsWithMeals
                        {
                            EventId = e.EventId,
                            EventName = e.EventName,
                            EventLocation = e.EventLocation,
                            EventStartTime = e.EventStartTime,
                            EventEndTime = e.EventEndTime,
                            IsActive = e.IsActive,
                            Meals = e.Meals,
                            CreatedOn = e.CreatedOn,
                            CreatedByName = e.CreatedByName,
                            UpdatedOn = e.UpdatedOn,
                            UpdatedBy = e.UpdatedBy
                        };
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, responseData.Message);
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Error fetching event details.");
                }
            }
            else
            {
                Event = new EventsWithMeals();
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                if (Event.EventId == 0)
                {
                    // Save the new event
                    await SaveEvent(Event);
                }
                else
                {
                    // Update the existing event
                    await UpdateEvent(Event);
                }
                return RedirectToPage("EventList"); // Redirect to a list page or another relevant page
            }

            return Page();
        }

        private async Task SaveEvent(EventsWithMeals eventModel)
        {
            // Call your API to save the new event
            var response = await _httpClient.PostAsJsonAsync("/api/SaveEvent", eventModel);
            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(string.Empty, "Error saving event.");
            }
        }

        private async Task UpdateEvent(EventsWithMeals eventModel)
        {
            // Call your API to update the event
            var response = await _httpClient.PutAsJsonAsync("/api/UpdateEvent", eventModel);
            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(string.Empty, "Error updating event.");
            }
        }
    }
}
