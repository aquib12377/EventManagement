using System;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EventManagement.Helper
{
    public static class CookieHelper
    {

        private static IRequestCookieCollection Cookies;
        private static string CKey => Encode("IsLoggedIn");
        private static string CVak => Encode("Yes");
        private static string Encode(string value) 
            => Convert.ToBase64String(Encoding.UTF8.GetBytes(value))
                .Replace('+', '-')
                .Replace('/', '_')
                .TrimEnd('=');
        public static bool IsLoggedIn(this HttpContext context) 
            => context.Request.Cookies.ContainsKey(CKey) && context.Request.Cookies[CKey] == CVak;
        public static void LogInUser(this HttpContext context,int userId)
        {
            if (context.Request.Cookies.ContainsKey(CKey))
            {
                context.Response.Cookies.Append(CKey, CVak, new CookieOptions
                {
                    Expires = DateTimeOffset.UtcNow.AddDays(7)
                });
                //context.Response.Cookies.Append("LU", userId.ToString(), new CookieOptions
                //{
                //    Expires = DateTimeOffset.UtcNow.AddDays(7)
                //}); 
            }
            else
            {
                context.Response.Cookies.Append(CKey, CVak, new CookieOptions
                {
                    Expires = DateTimeOffset.UtcNow.AddDays(7)
                });
                //context.Response.Cookies.Append("LU", userId.ToString(), new CookieOptions
                //{
                //    Expires = DateTimeOffset.UtcNow.AddDays(7)
                //});
            }
            Cookies = context.Request.Cookies;
        }

        public static int GetUID()
        {
            return Convert.ToInt32(Cookies["LU"]);
        }

        public static void LogOutUser(this HttpContext context) => context.Response.Cookies.Delete(CKey);
        public static void RemoveUID(this HttpContext context)
        {
            if (context.Request.Cookies.ContainsKey("LU"))
            {
                context.Response.Cookies.Delete("LU");
                //UserHelper.User = null;
            }
        }
    }
}
