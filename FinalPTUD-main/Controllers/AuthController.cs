
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using BusTicketBooking.Models;

namespace BusTicketBooking.Controllers
{
    public class AuthController : Controller
    {
        private readonly BusDbContext _db;
        public AuthController(BusDbContext db) => _db = db;

        public IActionResult Login(string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password, string? returnUrl = null)
        {
            string Hash(string s)
            {
                using var sha = System.Security.Cryptography.SHA256.Create();
                var bytes = System.Text.Encoding.UTF8.GetBytes(s);
                var hash = sha.ComputeHash(bytes);
                return BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
            var hashedPassword = Hash(password);

            var user = _db.Users
                .FirstOrDefault(u => u.Email == email && u.PasswordHash == hashedPassword);

            if (user == null)
            {
                TempData["Error"] = "Sai email hoặc mật khẩu";
                return View();
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)) return Redirect(returnUrl);
            return RedirectToAction("Index", "Home");
        }

        public IActionResult Register() => View();

        [HttpPost]
        public IActionResult Register(string name, string email, string password, string phone)
        {
            if (_db.Users.Any(u => u.Email == email))
            {
                TempData["Error"] = "Email đã tồn tại";
                return View();
            }

            string Hash(string s)
            {
                using var sha = System.Security.Cryptography.SHA256.Create();
                var bytes = System.Text.Encoding.UTF8.GetBytes(s);
                var hash = sha.ComputeHash(bytes);
                return BitConverter.ToString(hash).Replace("-", "").ToLower();
            }

            var user = new User
            {
                Name = name,
                Email = email,
                PasswordHash = Hash(password),
                Role = "User",
                Phone = phone
            };
            _db.Users.Add(user);
            _db.SaveChanges();
            TempData["Message"] = "Đăng ký thành công. Hãy đăng nhập.";
            return RedirectToAction(nameof(Login));
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        public IActionResult AccessDenied() => Content("Bạn không có quyền truy cập.");
    }
}
