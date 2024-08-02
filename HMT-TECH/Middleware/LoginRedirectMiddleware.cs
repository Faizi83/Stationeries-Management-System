using System.Security.Claims;

namespace HMT_Tech.Middleware
{
    public class LoginRedirectMiddleware
    {
        private readonly RequestDelegate _next;

        public LoginRedirectMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Check if the user is authenticated
            if (!context.User.Identity.IsAuthenticated)
            {
                // Check if the current path is not login or register
                var path = context.Request.Path.Value.ToLower();
                if (!path.Contains("login"))
                {
                    // Redirect to login page
                    context.Response.Redirect("/login");
                    return;
                }
            }
            else
            {
                // Check if the user is not an Admin and is trying to access restricted pages
                var role = context.User.FindFirst(ClaimTypes.Role)?.Value;
                var path = context.Request.Path.Value.ToLower();
                var restrictedPaths = new[] { "addnewuser", "allrequestdata", "viewrequest", "requestformpage","report","result" };

                // Check if the user is trying to access "addnewuser" page
                if (role != "Admin" && path.Contains("addnewuser"))
                {
                    // Redirect to unauthorized page or home page
                    context.Response.Redirect("/unauthorized");
                    return;
                }

                // Check if the user is trying to access "allrequestdata" page
                if (path.Contains("allrequestdata"))
                {
                    // Allow access for both Admin and Manager roles
                    if (role != "Admin" && role != "Manager")
                    {
                        // Redirect to unauthorized page or home page
                        context.Response.Redirect("/unauthorized");
                        return;
                    }
                }

                if (path.Contains("viewrequest"))
                {
                    // Allow access for both Admin and Manager roles
                    if (role != "Employee" && role != "Manager")
                    {
                        // Redirect to unauthorized page or home page
                        context.Response.Redirect("/unauthorized");
                        return;
                    }
                }

                if (path.Contains("requestformpage"))
                {
                    // Allow access for both Admin and Manager roles
                    if (role != "Employee" && role != "Manager")
                    {
                        // Redirect to unauthorized page or home page
                        context.Response.Redirect("/unauthorized");
                        return;
                    }
                }

                if (path.Contains("report"))
                {
                    // Allow access for both Admin and Manager roles
                    if (role != "Admin" && role != "Manager")
                    {
                        // Redirect to unauthorized page or home page
                        context.Response.Redirect("/unauthorized");
                        return;
                    }
                }

                if (path.Contains("result"))
                {
                    // Allow access for both Admin and Manager roles
                    if (role != "Admin")
                    {
                        // Redirect to unauthorized page or home page
                        context.Response.Redirect("/unauthorized");
                        return;
                    }
                }
            }


            // Call the next middleware in the pipeline
            await _next(context);
        }
    }
}