using BlogApp.Web.Models.Entities;

namespace BlogApp.Web.Helpers
{
    public static class SessionHelper
    {
        public static void SetUserSession(AppUser user, HttpContext httpContext)
        {
            httpContext.Session.SetString("user_id", user.Id);
            httpContext.Session.SetString("username", user.UserName);
        }

        public static void SetUserSession(string user_id, string user_name, HttpContext httpContext)
        {
            httpContext.Session.SetString("user_id", user_id);
            httpContext.Session.SetString("username", user_name);
        }

        public static void ClearUserSession(HttpContext httpContext)
        {
            httpContext.Session.Clear();
        }

    }
}
