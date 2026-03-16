using LeaveSphere.Web.Models.ViewModels;
using LeaveSphere.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace LeaveSphere.Web.Controllers
{
    [Route("Account")]
    public class AccountController : Controller
    {
        private readonly ApiService _api;

        public AccountController(ApiService api)
        {
            _api = api;
        }

        [HttpGet]
        [Route("Login")]
        [Route("~/")]
        public IActionResult Login()
        {
            // ✅ If already logged in, redirect to appropriate page
            var token = HttpContext.Session.GetString("JWToken");
            if (!string.IsNullOrEmpty(token))
            {
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);
                var roleClaim = jwtToken.Claims
                    .FirstOrDefault(c => c.Type == ClaimTypes.Role || c.Type == "role");

                if (roleClaim != null)
                {
                    if (roleClaim.Value == "Admin")
                        return RedirectToAction("Admin", "Dashboard");
                    if (roleClaim.Value == "TeamLeader")
                        return RedirectToAction("TeamLeader", "Dashboard");

                    return Redirect("/Employee/Dashboard");
                }
                else
                {
                    return Redirect("/Employee/Dashboard");
                }
            }

            // 🚫 Prevent browser from caching the login page
            Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
            Response.Headers["Pragma"] = "no-cache";
            Response.Headers["Expires"] = "0";

            return View();
        }

        [HttpPost]
        [Route("Login")]
        [Route("~/")]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            var result = await _api.PostAsync("auth/login", model);

            if (string.IsNullOrEmpty(result) || !result.Trim().StartsWith("{"))
            {
                ViewBag.Error = "Invalid login";
                return View();
            }

            var json = JObject.Parse(result);

            if (json["token"] != null)
            {
                var tokenString = json["token"]?.ToString() ?? string.Empty;

                // ✅ Store Token
                HttpContext.Session.SetString("JWToken", tokenString);

                // ✅ Decode Token
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(tokenString);

                // ✅ Get Role
                var roleClaim = jwtToken.Claims
                    .FirstOrDefault(c => c.Type == ClaimTypes.Role || c.Type == "role");

                // ✅ Get EmployeeId from token
                var empIdClaim = jwtToken.Claims
                    .FirstOrDefault(c => c.Type == "EmployeeId");

                if (empIdClaim != null)
                {
                    HttpContext.Session.SetInt32("EmployeeId",
                        Convert.ToInt32(empIdClaim.Value));
                }

                // ✅ Get EmployeeName from token
                var empNameClaim = jwtToken.Claims
                    .FirstOrDefault(c => c.Type == "EmployeeName");
                if (empNameClaim != null)
                {
                    HttpContext.Session.SetString("EmployeeName", empNameClaim.Value);
                }

                // 🔥 Redirect based on role
                if (roleClaim != null)
                {
                    if (roleClaim.Value == "Admin")
                        return RedirectToAction("Admin", "Dashboard");
                    if (roleClaim.Value == "TeamLeader")
                        return RedirectToAction("TeamLeader", "Dashboard");

                    return Redirect("/Employee/Dashboard");
                }
                else
                {
                    return Redirect("/Employee/Dashboard");
                }
            }

            ViewBag.Error = "Invalid login";
            return View();
        }

        [HttpGet]
        [Route("Logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        [HttpGet]
        [Route("Profile")]
        public async Task<IActionResult> Profile()
        {
            var token = HttpContext.Session.GetString("JWToken");
            if (string.IsNullOrEmpty(token)) return RedirectToAction("Login");

            var response = await _api.GetAsync("auth/profile", token);
            Console.WriteLine($"[Web] Profile Response: {response}");
            if (string.IsNullOrEmpty(response) || response.Contains("Error"))
            {
                ViewBag.Error = "Could not fetch profile data.";
                return View();
            }

            var profile = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(response);
            return View(profile);
        }

        [HttpGet]
        [Route("Settings")]
        public IActionResult Settings()
        {
            var token = HttpContext.Session.GetString("JWToken");
            if (string.IsNullOrEmpty(token)) return RedirectToAction("Login");

            return View();
        }
    }
}