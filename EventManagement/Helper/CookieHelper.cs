using System;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace EventManagement.Helper
{
    public static class CookieHelper
    {
        private static string CKey => Encode("IsLoggedIn");
        private static string CVak => Encode("Yes");
        private static string Encode(string value) 
            => Convert.ToBase64String(Encoding.UTF8.GetBytes(value))
                .Replace('+', '-')
                .Replace('/', '_')
                .TrimEnd('=');
        public static bool IsLoggedIn(this HttpContext context) 
            => context.Request.Cookies.ContainsKey(CKey) && context.Request.Cookies[CKey] == CVak;
        public static void LogInUser(this HttpContext context)
        {
            if (context.Request.Cookies.ContainsKey(CKey))
            {
                context.Response.Cookies.Append(CKey, CVak, new CookieOptions
                {
                    Expires = DateTimeOffset.UtcNow.AddDays(7)
                });
            }
            else
            {
                context.Response.Cookies.Append(CKey, CVak, new CookieOptions
                {
                    Expires = DateTimeOffset.UtcNow.AddDays(7)
                });
            }
        }

        public static void LogOutUser(this HttpContext context) => context.Response.Cookies.Delete(CKey);
    }
}
