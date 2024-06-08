using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Text;
using Microsoft.Extensions.Configuration;
using EventManagement.Models;
using EventManagement.DbContext;

namespace EventManagement.Helper
{
    public class SalesforceIntegration
    {
        private IConfiguration Configuration { get; }

        public SalesforceIntegration(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        private async Task<string?> GetAccessTokenAsync()
        {
            try
            {
                var client = new HttpClient();
                var tokenUrl = Configuration["SF:TokenURL"];
                var clientId = Configuration["SF:ClientId"];
                var clientSecret = Configuration["SF:ClientSecret"];
                var username = Configuration["SF:Username"];
                var password = Configuration["SF:Password"];

                var parameters = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("grant_type", "password"),
                    new KeyValuePair<string, string>("client_id", clientId),
                    new KeyValuePair<string, string>("client_secret", clientSecret),
                    new KeyValuePair<string, string>("username", username),
                    new KeyValuePair<string, string>("password", password)
                });

                var request = new HttpRequestMessage(HttpMethod.Post, tokenUrl)
                {
                    Content = parameters
                };

                var response = await client.SendAsync(request);
                var content = await response.Content.ReadAsStringAsync();
                var tokenResponse = JsonConvert.DeserializeObject<Dictionary<string, string>>(content);

                return tokenResponse["access_token"];
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting access token: {ex.Message}");
                return null;
            }
        }

        private async Task<string?> GetInstanceUrlAsync()
        {
            try
            {
                var client = new HttpClient();
                var tokenUrl = Configuration["SF:TokenURL"];
                var clientId = Configuration["SF:ClientId"];
                var clientSecret = Configuration["SF:ClientSecret"];
                var username = Configuration["SF:Username"];
                var password = Configuration["SF:Password"];

                var parameters = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("grant_type", "password"),
                    new KeyValuePair<string, string>("client_id", clientId),
                    new KeyValuePair<string, string>("client_secret", clientSecret),
                    new KeyValuePair<string, string>("username", username),
                    new KeyValuePair<string, string>("password", password)
                });

                var request = new HttpRequestMessage(HttpMethod.Post, tokenUrl)
                {
                    Content = parameters
                };

                var response = await client.SendAsync(request);
                var content = await response.Content.ReadAsStringAsync();
                var tokenResponse = JObject.Parse(content);

                return (string?)tokenResponse["instance_url"];
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting instance URL: {ex.Message}");
                return null;
            }
        }

        public async Task PushMemberDataAsync(MemberResponse member)
        {
            try
            {
                string? accessToken = await GetAccessTokenAsync();
                string? instanceUrl = await GetInstanceUrlAsync();
                string url = $"{instanceUrl}/services/apexrest/api/member";

                var client = new HttpClient();
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken);

                var json = JsonConvert.SerializeObject(member);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(url, content);
                var responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine(responseContent);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error pushing member data: {ex.Message}");
            }
        }

        public async Task PushEventDataAsync(EventData eventData)
        {
            try
            {
                string? accessToken = await GetAccessTokenAsync();
                string? instanceUrl = await GetInstanceUrlAsync();
                string url = $"{instanceUrl}/services/apexrest/api/event";

                var client = new HttpClient();
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken);

                var json = JsonConvert.SerializeObject(eventData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(url, content);
                var responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine(responseContent);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error pushing event data: {ex.Message}");
            }
        }

        public async Task PushAttendanceDataAsync(AttendanceData attendanceData)
        {
            try
            {
                string? accessToken = await GetAccessTokenAsync();
                string? instanceUrl = await GetInstanceUrlAsync();
                string url = $"{instanceUrl}/services/apexrest/api/visits";

                var client = new HttpClient();
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken);

                var json = JsonConvert.SerializeObject(attendanceData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(url, content);
                var responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine(responseContent);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error pushing attendance data: {ex.Message}");
            }
        }
        public async Task<MemberResponse> FetchAllMembersAsync()
        {
            MemberResponse member = new MemberResponse();
            try
            {
                string? accessToken = await GetAccessTokenAsync();
                string? instanceUrl = await GetInstanceUrlAsync();
                string url = $"{instanceUrl}/services/apexrest/api/member";

                var client = new HttpClient();
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken);

                var response = await client.GetAsync(url);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    member = JsonConvert.DeserializeObject<MemberResponse>(responseContent);
                    Console.WriteLine("Fetched Member Data: " + JsonConvert.SerializeObject(member, Formatting.Indented));
                }
                else
                {
                    Console.WriteLine("Error fetching member data: " + responseContent);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching member data: {ex.Message}");
            }
            return member;
        }
    }
    public class MemberResponse
    {
        [JsonProperty("member")]
        public List<Member> Members { get; set; }
    }

    public class Member
    {
        [JsonProperty("memberId")]
        public string MemberId { get; set; }

        [JsonProperty("memberName")]
        public string MemberName { get; set; }

        [JsonProperty("phone")]
        public string Phone { get; set; }

        [JsonProperty("lifeGroupName")]
        public string LifeGroupName { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("area")]
        public string Area { get; set; }

        [JsonProperty("address")]
        public string Address { get; set; }
    }
    public class AttendanceData
    {
        [JsonProperty("dataList")]
        public List<AttendanceDataList> DataList { get; set; }
    }

    public class AttendanceDataList
    {
        [JsonProperty("attendance")]
        public List<SFAttendance> Attendances { get; set; }
    }

    public class SFAttendance
    {
        [JsonProperty("eventid")]
        public string EventId { get; set; }

        [JsonProperty("memberId")]
        public string MemberId { get; set; }
    }
    public class EventData
    {
        [JsonProperty("dataList")]
        public List<EventDataList> DataList { get; set; }
    }

    public class EventDataList
    {
        [JsonProperty("evnt")]
        public List<SFEvent> Events { get; set; }
    }

    public class SFEvent
    {
        [JsonProperty("eventid")]
        public string EventId { get; set; }

        [JsonProperty("eventName")]
        public string EventName { get; set; }
    }
}
