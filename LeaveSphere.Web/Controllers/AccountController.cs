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

                if (roleClaim != null && roleClaim.Value == "Admin")
                    return RedirectToAction("Index", "Dashboard");
                else
                    return RedirectToAction("MyLeaves", "Leave");
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
                if (roleClaim != null && roleClaim.Value == "Admin")
                {
                    return RedirectToAction("Index", "Dashboard");
                }
                else
                {
                    return RedirectToAction("MyLeaves", "Leave");
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
    }
}