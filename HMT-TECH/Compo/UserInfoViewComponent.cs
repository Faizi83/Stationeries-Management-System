using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using HMT_Tech.Database; // Adjust namespace accordingly
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HMT_Tech.Compo // Adjust namespace to match your project structure
{
    public class UserInfoViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _dbContext;

        public UserInfoViewComponent(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var claimsPrincipal = User as ClaimsPrincipal; // Cast User to ClaimsPrincipal
            var userId = claimsPrincipal?.FindFirstValue(ClaimTypes.NameIdentifier); // Get the current user's ID
            if (userId != null)
            {
                var user = await _dbContext.Registers.FirstOrDefaultAsync(u => u.Id == int.Parse(userId)); // Fetch user details
                if (user != null)
                {
                    return View(user); // Pass user details to the view
                }
            }
            // Debugging: Return a view with a message indicating no user found or not authenticated
            ViewBag.Message = "User not authenticated or not found";
            return View("Default");
        }
    }
}
