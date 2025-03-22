using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using Npgsql;
using WeirdAdminPanel.Models;

namespace WeirdAdminPanel.Controllers
{
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;
        public UserController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            if (await IsAuthenticatedUser() == false) return await Logout();
            List<User> user = await _context.User.ToListAsync();
            List<User> sortedUser = user.OrderByDescending(u => u.LastLogin).ToList();
            return View(sortedUser);
        }

        public ActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public async Task<ActionResult> Register([Bind("Name, Email, Password")] User user)
        {
            // For Postgresql
            // try
            // {
            //     _context.User.Add(user);
            //     await _context.SaveChangesAsync();
            //     return RedirectToAction("Login");
            // }
            // catch (DbUpdateException e)
            // {
            //     if (e.InnerException is PostgresException pgE && pgE.SqlState == "23505")
            //     {
            //         ModelState.AddModelError("NotUniqueUser", "Email already in use!");
            //         return View(user);
            //     }

            //     Console.WriteLine(e);
            // }

           // For Sql Server 
            try
            {
                _context.User.Add(user);
                await _context.SaveChangesAsync();
                return RedirectToAction("Login");
            }
            catch (DbUpdateException e)
            {
                if (e.InnerException is SqlException sqlE && (sqlE.Number == 2601 || sqlE.Number == 2627))
                {
                    ModelState.AddModelError("NotUniqueUser", "Email already in use!");
                    return View(user);
                }

                Console.WriteLine(e);
            }

            return View(user);


        }

        public async Task<bool> IsValidUser(string email, string password)
        {
            User? user = await _context.User.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null) return false;
            if (user.Password == password && user.IsBlocked == false) return true;
            return false;
        }

        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login([Bind("Email, Password")] User user)
        {
            if(user.Password == null) return View(user);

            if (await IsValidUser(user.Email, user.Password) == false)
            {
                ModelState.AddModelError("User404", "Incorrect email or password");
                return View(user);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Email),
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authenticationProperties = new AuthenticationProperties
            {
                AllowRefresh = true,
                ExpiresUtc = DateTime.UtcNow.AddHours(1),
                IsPersistent = true,

            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authenticationProperties
            );

            var currentUserMail = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
            var currentUser = await _context.User.FirstOrDefaultAsync(u => u.Email == currentUserMail);

            if(currentUser == null) return View(user);

            currentUser.LastLogin = DateTime.UtcNow;
            _context.Update(currentUser);
            await _context.SaveChangesAsync();

            ViewBag.CurrentUserName = currentUser.Name;
            
            return RedirectToAction("Index");
        }


        public async Task<bool> IsAuthenticatedUser()
        {

            var currentUserMail = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
            var currentUser = await _context.User.FirstOrDefaultAsync(u => u.Email == currentUserMail);
            if (currentUser == null || currentUser.IsBlocked == true) return false;
            return true;
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }


        [HttpPost]
        public async Task<IActionResult> Delete(List<int> selectedId)
        {
            if (await IsAuthenticatedUser() == false) return await Logout();
            if (selectedId.Count != 0)
            {
                var users = _context.User.Where(u => selectedId.Contains((u.Id))).ToList();
                _context.User.RemoveRange(users);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Block(List<int> selectedId)
        {
            if (await IsAuthenticatedUser() == false) return await Logout();
            if (selectedId.Count != 0)
            {
                _context.User.Where(u => selectedId.Contains(u.Id))
                                .ToList()
                                .ForEach(u => u.IsBlocked = true);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Unblock(List<int> selectedId)
        {
            if (await IsAuthenticatedUser() == false) return await Logout();
            if (selectedId.Count != 0)
            {
                _context.User.Where(u => selectedId.Contains(u.Id))
                                .ToList()
                                .ForEach(u => u.IsBlocked = false);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }
    }
}
