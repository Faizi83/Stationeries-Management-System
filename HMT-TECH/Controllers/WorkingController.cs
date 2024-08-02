using Microsoft.AspNetCore.Mvc;
using HMT_Tech.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using HMT_Tech.Database;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using HMTTech.Migrations;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace HMT_Tech.Controllers
{
    public class WorkingController : Controller
    {

        private readonly ApplicationDbContext dbContext;
        private readonly IWebHostEnvironment webHostEnvironment;


        public WorkingController(ApplicationDbContext dbContext, IWebHostEnvironment webHostEnvironment)
        {
            this.dbContext = dbContext;
            this.webHostEnvironment = webHostEnvironment;
        }

        [Route("")]
        [Route("index")]
        public async Task<IActionResult> Index()
        {

            var users = dbContext.Registers
                       .Where(u => u.Role != "admin")
                       .Take(5)
                       .ToList();

        

            var requests = dbContext.StationeryRequests.Count();
            var pendings = dbContext.StationeryRequests.Where(sr => sr.Status == "Pending").Count();
            var approves = dbContext.StationeryRequests.Where(sr => sr.Status == "Approved").Count();

            var adminCount = dbContext.Registers.Where(rl => rl.Role == "Admin").Count();
            var managerCount = dbContext.Registers.Where(rl => rl.Role == "Manager").Count();
            var employeCount = dbContext.Registers.Where(rl => rl.Role == "Employee").Count();
            var adminBalance = dbContext.Registers.Where(reg => reg.Role == "Admin").Select(reg => reg.Balance).FirstOrDefault();

         

            ViewBag.Requests = requests;
            ViewBag.Approve = approves;
            ViewBag.Pending = pendings;
            ViewBag.AdminBalance = adminBalance;

            ViewBag.Admins = adminCount;
            ViewBag.Managers= managerCount;
            ViewBag.Employes = employeCount;

            return View(users);
        }

     
            [HttpGet]
        [Route("login")]
        public IActionResult login()
        {
            return View();
        }

        [Route("login")]
        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            var user = await dbContext.Registers.FirstOrDefaultAsync(u => u.Email == email && u.Password == password);
            if (user != null)
            {
                // Create claims and identity
                var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()) // Add User ID
        };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true
                };

                // Sign in the user
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

                if (user.Role == "Admin")
                {
                    return RedirectToAction("Index"); // Admin dashboard
                }
                else
                {
                    return RedirectToAction("Index"); // User dashboard
                }
            }

            ViewBag.ErrorMessage = "Incorrect Email or Password.";
            return View();
        }


      
        [Route("logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("login");
        }

       
        [Route("unauthorized")]
        public IActionResult unauthorized()
        {
            return View();  
        }


        [HttpGet]
        [Route("addnewuser")]
        public IActionResult addnewuser()
        {

            //for fetching only Manager Roles
            var managers = dbContext.Registers.Where(r => r.Role == "Manager").Select(r => r.Name).Distinct().ToList();

            //for sending Manager as about viewbag
            ViewBag.Managers = managers;

            //for not showing admin identity data
            var users = dbContext.Registers.Where(r => r.Identity != "admin").ToList();
            return View(users);
        }


        [HttpPost]
        [Route("addnewuser")]
        public async Task<IActionResult> addnewuser(Register viewmodel, IFormFile profileimage)
        {
            string imageurl = null;

            if (profileimage != null)
            {
                string uploadsfolder = Path.Combine(webHostEnvironment.WebRootPath, "profileimages");
                Directory.CreateDirectory(uploadsfolder);

                string uniquefilename = Guid.NewGuid().ToString() + "_" + profileimage.FileName;
                string filepath = Path.Combine(uploadsfolder, uniquefilename);

                using (var filestream = new FileStream(filepath, FileMode.Create))
                {
                    await profileimage.CopyToAsync(filestream);
                    imageurl = "/profileimages/" + uniquefilename;
                }

                var reg = new Register
                {
                    Name = viewmodel.Name,
                    Identity = viewmodel.Identity,
                    Gender = viewmodel.Gender,
                    Role = viewmodel.Role,
                    Manager = viewmodel.Manager,
                    Balance = viewmodel.Balance,
                    Email = viewmodel.Email,
                    Password = viewmodel.Password,
                    Image = imageurl,
                };

                await dbContext.AddAsync(reg);
                await dbContext.SaveChangesAsync();

                return RedirectToAction("addnewuser"); // Assuming you have a view to return to after successful addition
            }
            return BadRequest("Profile image is required.");
        }

        //For Adding Balance to Accounts
        [HttpPost]
        [Route("AddBalance")]
        public async Task<IActionResult> AddBalance(string modalIdentity, int addBalanceAmount)
        {
            if (string.IsNullOrEmpty(modalIdentity) || addBalanceAmount <= 0)
            {
                // Handle invalid input (e.g., display error message)
                return BadRequest("Invalid identity or amount. Please enter a valid identity and a positive amount to add.");
            }

            try
            {


                // Find the user based on the provided identity
                var user = await dbContext.Registers.FirstOrDefaultAsync(u => u.Identity == modalIdentity);

                if (user == null)
                {
                    // Handle user not found (e.g., display error message)
                    return NotFound("User with the given identity not found.");
                }

                // Update the user's balance by adding the input amount
                user.Balance += addBalanceAmount;

                // Save the changes to the database
                await dbContext.SaveChangesAsync();

                // Success message (optional)
                TempData["SuccessMessage"] = "Balance updated successfully!";

                return RedirectToAction("addnewuser"); // Assuming you have an Index action to redirect to
            }
            catch (DbUpdateException ex)
            {
                // Handle database update errors (e.g., logging, user-friendly message)
                return StatusCode(500, "An error occurred while updating the balance. Please try again later.");
            }
        }





        [HttpGet]
        [Route("addstationery")]
        public async Task <IActionResult> AddStationery()
        {

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userId))
            {
                // Ensure that both operations are awaited properly
                var items = dbContext.Stationeries.ToList();
                var user = await dbContext.Registers.FirstOrDefaultAsync(u => u.Id.ToString() == userId);

                if (user != null)
                {
                    // Access the Identity column and store it in ViewBag
                    ViewBag.Role = user.Role;

                    return View(items);
                }
            }
           
            return View();
        }

        [HttpPost]
        [Route("addstationery")]
        public async Task<IActionResult> addstationery(AddStationery viewmodel)
        {
          

            var stationery = new AddStationery
            {
                Name = viewmodel.Name,
                Price = viewmodel.Price,
                Quantity = viewmodel.Quantity,
            };
          


            await dbContext.AddAsync(stationery);
            await dbContext.SaveChangesAsync();
            return RedirectToAction(nameof(addstationery));
        }



        [HttpGet]
        [Route("allrequestdata")]
        public async Task<IActionResult> allrequestdata()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await dbContext.Registers.FirstOrDefaultAsync(u => u.Id.ToString() == userId);

            if (user == null)
            {
                return NotFound(); // Handle the case where user is not found
            }

            var pendingRequests = await dbContext.StationeryRequests
                                                 .Where(request => request.Status == "Pending")
                                                 .ToListAsync();

            var allrequests = pendingRequests.Where(req => req.RequestId != user.Id).ToList(); // Ensure the property you're comparing is correct
            return View(allrequests);
        }

        //for fetching authenicated user data from data base

        [HttpGet]
        [Route("requestformpage")]
        public async Task<IActionResult> requestformpage(int itemid)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userId))
            {
                // Ensure that both operations are awaited properly
                var item = await dbContext.Stationeries.FindAsync(itemid); // Await this operation
                var user = await dbContext.Registers.FirstOrDefaultAsync(u => u.Id.ToString() == userId);

                if (user != null)
                {
                    // Access the Identity column and store it in ViewBag

                    ViewBag.ReqId = user.Id;
                    ViewBag.Name = user.Name;
                    ViewBag.Identity = user.Identity;
                    ViewBag.Manager = user.Manager;
                    ViewBag.Balance = user.Balance;

                    return View(item);
                }
            }

            ViewBag.Message = "User not authenticated or not found";
            return View("Error"); // Handle error case appropriately
        }

        [HttpPost]
        [Route("/Stationery/Add")]
        public async Task<IActionResult> StationeryAdd(int Identity, string Name, string ManagerName, string ItemName, int Quantity, string Date, int Price, string Status, int StationeryId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await dbContext.Registers.FirstOrDefaultAsync(u => u.Id.ToString() == userId);

            if (user != null)
            {
                var notification = new Notification
                {
                    Name = user.Name,
                    Image = user.Image,
                    Time = DateTime.Now.TimeOfDay,
                    Stationery = ItemName,
                    SenderId = user.Id
                };

                var stationeryRequest = new StationeryViewModel
                {
                    Name = Name,
                    Manager = ManagerName,
                    Stationery = ItemName,
                    Qty = Quantity,
                    Date = Date,
                    Price = Price,
                    Status = Status,
                    RequestId = Identity,
                    Stationery_id = StationeryId
                };

                await dbContext.Notifications.AddAsync(notification);
                await dbContext.StationeryRequests.AddAsync(stationeryRequest);
                await dbContext.SaveChangesAsync();

                TempData["ShowSweetAlert"] = true;

                return RedirectToAction(nameof(viewrequest));
            }

            // Optionally, handle the case where the user is not found.
            TempData["Error"] = "User not found.";
            return RedirectToAction(nameof(viewrequest));
        }






        [HttpGet]
        [Route("viewrequest")]
        public async Task<IActionResult> viewrequest()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await dbContext.Registers.FirstOrDefaultAsync(u => u.Id.ToString() == userId);


            if (user != null)
            {
                var requests = await dbContext.StationeryRequests
                    .Where(r => r.RequestId == user.Id)
                    .ToListAsync();
                return View(requests);
            }
            else
            {
                
                return BadRequest("sorry");
            }
        }




            [Route("help")]
        public IActionResult help()
        {
            return View();
        }

        [Route("contact")]
        public IActionResult contact()
        {
            return View();
        }

        [HttpPost]
        [Route("contact")]
        public async Task <IActionResult> contact(Contact viewmodel)
        {
            var newcontact = new Contact
            {
               FirstName = viewmodel.FirstName,
               LastName = viewmodel.LastName,
               Email = viewmodel.Email,
               Comment = viewmodel.Comment,

            };

            await dbContext.ContactsData.AddAsync(newcontact);
            await dbContext.SaveChangesAsync();


            return View();
        }


        [HttpGet]
        [Route("UserEdit")]
        public async Task<IActionResult> UserEdit()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userId))
            {
                var user = await dbContext.Registers.FirstOrDefaultAsync(u => u.Id.ToString() == userId);
                if (user != null)
                {
                    return View(user); // Pass the user object to the view
                }
            }

            ViewBag.Message = "User not authenticated or not found";
            return View("Error"); // Handle error case appropriately
        }

        //Changing after update a profile of user

        [HttpPost]
        [Route("UserEdit")]
        public async Task<IActionResult> UserEdit(int profileId, IFormFile profilePic, Register viewmodel)
        {
            var profile = await dbContext.Registers.FindAsync(profileId);
            if (profile == null)
            {
                return NotFound();
            }

            if (profilePic != null)
            {
                // Delete old image
                if (!string.IsNullOrEmpty(profile.Image))
                {
                    string oldImagePath = Path.Combine(webHostEnvironment.WebRootPath, profile.Image.TrimStart('/'));
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

                // Save new image
                string uploadsFolder = Path.Combine(webHostEnvironment.WebRootPath, "profileimages");
                Directory.CreateDirectory(uploadsFolder);

                string uniqueFileName = Guid.NewGuid().ToString() + "_" + profilePic.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await profilePic.CopyToAsync(fileStream);
                }

                profile.Image = "/profileimages/" + uniqueFileName;
            }

            profile.Name = viewmodel.Name;
            profile.Email = viewmodel.Email;
            profile.Gender = viewmodel.Gender; 

            dbContext.Registers.Update(profile);
            await dbContext.SaveChangesAsync();

            // Display SweetAlert
            TempData["ShowSweetAlert"] = true;

            return RedirectToAction(nameof(UserEdit));
        }




        [HttpPost]
        [Route("ChangePassword")]
        public async Task<IActionResult> ChangePassword(int passid, string newpass)
        {
            if (ModelState.IsValid)
            {
                var user = await dbContext.Registers.FirstOrDefaultAsync(u => u.Id == passid);
                if (user != null)
                {
                    user.Password = newpass;

                    // Save changes to database
                    await dbContext.SaveChangesAsync();

                    return Ok(); // Return a success response
                }
                return NotFound(); // Return a not found response if the user is not found
            }

            return BadRequest(); // Return a bad request response if the model state is invalid
        }


    

        [Route("UserProfile")]
        public IActionResult UserProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = dbContext.Registers.FirstOrDefault(u => u.Id.ToString() == userId);

            var allRequestsCount = dbContext.StationeryRequests
                             .Where(rp => rp.RequestId == user.Id)
                             .Count();
            var pendingRequestsCount = dbContext.StationeryRequests
                                    .Where(rp => rp.RequestId == user.Id && rp.Status == "Pending")
                                    .Count();

            var approvedRequestsCount = dbContext.StationeryRequests
                                    .Where(rp => rp.RequestId == user.Id && rp.Status == "Approved")
                                    .Count();


            var rejectedRequestsCount = dbContext.StationeryRequests
                                    .Where(rp => rp.RequestId == user.Id && rp.Status == "Rejected")
                                    .Count();


            if (user != null)
            {
                ViewBag.Name = user.Name;
                ViewBag.Image = user.Image;
                ViewBag.Role = user.Role;

                ViewBag.All = allRequestsCount;
                ViewBag.Pendings = pendingRequestsCount;
                ViewBag.Approved = approvedRequestsCount;
                ViewBag.Rejected = rejectedRequestsCount;


                if (user.Role == "Manager")
                {
                    //Manager points
                    ViewBag.pointOne = "Managers Can Send Requets";
                    ViewBag.pointTwo = "Managers can also Approve Requests";
                    ViewBag.pointThree = "Managers Can See Requested Data";
                    ViewBag.pointFour = "Managers Can see monthly incured expenses ";
                }

                if (user.Role == "Employee")
                {
                    //Employee points
                    ViewBag.pointOne = "Employes Can Send Requets";
                    ViewBag.pointTwo = "Emmployes Can See his requested data items";
                    ViewBag.pointThree = "Employes Can Edit his requests";
                    ViewBag.pointFour = "Employes Change thier Password ";

                }


            }


            return View();
        }


        //For Approve Requests

        [HttpPost]
        public JsonResult ApproveRequest(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var request = dbContext.StationeryRequests.Find(id);
            var user =  dbContext.Registers.FirstOrDefault(u => u.Id.ToString() == userId);

            if (user != null)
            {
                var notf = new NotificationUser
                {
                  Role = user.Role,
                  Image = user.Image,
                  Stationery = request.Stationery,
                  Status = "Approved",
                  Time = DateTime.Now.TimeOfDay,
                };

                dbContext.NotificationsUser.Add(notf);
            }

                if (request != null)
            {
                request.Status = "Approved";
                dbContext.SaveChanges();
                return Json(new { success = true });
            }

            return Json(new { success = false });
        }


        //For Reject Requests

        [HttpPost]
        public JsonResult RejectRequest(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var request = dbContext.StationeryRequests.Find(id);
            var user = dbContext.Registers.FirstOrDefault(u => u.Id.ToString() == userId);

            if (user != null)
            {
                var notf = new NotificationUser
                {
                    Role = user.Role,
                    Image = user.Image,
                    Stationery = request.Stationery,
                    Status = "Rejected",
                    Time = DateTime.Now.TimeOfDay,
                };

                dbContext.NotificationsUser.Add(notf);
            }

            if (request != null)
            {
                request.Status = "Rejected";
                dbContext.SaveChanges();
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }


        //For Updating Request Form
        [HttpGet]
        [Route("Edit")]
        public async Task<IActionResult> Edit(int id, int StationeryId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userId))
            {
                // Ensure that both operations are awaited properly
                var model = await dbContext.Stationeries.FindAsync(StationeryId);
                var request = await dbContext.StationeryRequests.FindAsync(id);
                var user = await dbContext.Registers.FirstOrDefaultAsync(u => u.Id.ToString() == userId);

                if (user != null)
                {
                    // Access the Identity column and store it in ViewBag
                    ViewBag.Name = user.Name;
                    ViewBag.Identity = user.Identity;
                    ViewBag.Balance = user.Balance;
                    ViewBag.ItemId = id;
                    //Existing data of table
                    ViewBag.Manager = request.Manager;
                    ViewBag.Date = request.Date;
                    ViewBag.Stationery = request.Stationery;
                    ViewBag.Qty = request.Qty;
                    ViewBag.Price = request.Price;

                    // Set ViewBag.QuantityItems with the provided quantityItems parameter

                    return View(model);
                }
            }

            return BadRequest("Error");
        }

        [HttpPost]
        [Route("/Edit")]
        public IActionResult Edit(int ItemId, int Quantity, int Price)
        {
                // Find the record by Identity
                var stationeryRequest = dbContext.StationeryRequests.FirstOrDefault(sr => sr.Id == ItemId);

                if (stationeryRequest != null)
                {
                    // Update the Quantity and Price
                    stationeryRequest.Qty = Quantity;
                    stationeryRequest.Price = Price;

                    // Save the changes to the database
                    dbContext.SaveChanges();

                    return RedirectToAction("viewrequest"); // or wherever you want to redirect after successful update
                }
            

            // If we got this far, something failed, redisplay form
            return BadRequest("Eroor");
        }

        [Route("Deleteitem")]
        public async Task<IActionResult> Deleteitem(int itemid)
        {
            var item = await dbContext.Stationeries.FirstOrDefaultAsync(x => x.Id == itemid);

            if(item != null)
            {
                dbContext.Stationeries.Remove(item);
                await dbContext.SaveChangesAsync();
            }

            return RedirectToAction("addstationery");
        }


        [Route("DelRequest")]
        public async Task<IActionResult> DelRequest(int reqid)
        {
            var req = await dbContext.StationeryRequests.FirstOrDefaultAsync(x => x.Id == reqid);

            if (req != null)
            {
                dbContext.StationeryRequests.Remove(req);
                await dbContext.SaveChangesAsync();
            }

            return RedirectToAction("viewrequest");
        }




        [Route("Report")]
        public IActionResult Report()
        {
            // Fetch all stationery items
            var stationeries = dbContext.Stationeries.ToList();

            // Fetch all stationery requests
            var stationeryRequests = dbContext.StationeryRequests.ToList();

            // Calculate total cost of all items
            var totalCostAllItems = stationeryRequests.Sum(sr => sr.Price * sr.Qty);

            // Initialize running cumulative cost
            decimal runningCumulativeCost = 0;

            // Calculate the report data
            var reportData = stationeries.Select(s =>
            {
                var totalQuantity = stationeryRequests.Where(sr => sr.Stationery_id == s.Id).Sum(sr => sr.Qty);
                var totalCost = stationeryRequests.Where(sr => sr.Stationery_id == s.Id).Sum(sr => sr.Price * sr.Qty);
                var costPercentage = totalCostAllItems != 0 ? (int)(((decimal)dbContext.StationeryRequests.Where(sr => sr.Stationery_id == s.Id).Sum(sr => sr.Price * sr.Qty) / totalCostAllItems) * 100) : 0;
                var headCount = stationeryRequests.Where(sr => sr.Stationery_id == s.Id).Select(sr => sr.Manager).Distinct().Count();

                // Add current item's total cost to running cumulative cost
                runningCumulativeCost += totalCost;

                return new StationeryReportViewModel
                {
                    StationeryName = s.Name,
                    TotalQuantity = totalQuantity,
                    TotalCost = totalCost,
                    CostPercentage = costPercentage,
                    HeadCount = headCount,
                    CumulativeCost = runningCumulativeCost
                };
            }).ToList();

            return View(reportData);
        }


        [Route("result")]
        public IActionResult result()
        {
            var reports = dbContext.StationeryRequests
                        .Where(r => r.Status != "Pending")
                        .ToList();
            return View(reports);
        }

    }


}
