using LeaveSphere.Web.Models.ViewModels;
using LeaveSphere.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace LeaveSphere.Web.Controllers
{
    [Route("Dashboard")]
    public class DashboardController : Controller
    {
        private readonly ApiService _api;

        public DashboardController(ApiService api)
        {
            _api = api;
        }

        [HttpGet]
        [Route("")]
        [Route("Index")]
        public async Task<IActionResult> Index()
        {
            var token = HttpContext.Session.GetString("JWToken");
            if (token == null) return RedirectToAction("Login", "Account");

            var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            var roleClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role || c.Type == "role");

            if (roleClaim?.Value == "Admin") return RedirectToAction("Admin");
            if (roleClaim?.Value == "TeamLeader") return RedirectToAction("TeamLeader");

            if (roleClaim?.Value == "Employee") return RedirectToAction("Employee");

            return RedirectToAction("MyLeaves", "Leave");
        }

        [HttpGet]
        [Route("/Admin/Dashboard")]
        public async Task<IActionResult> Admin()
        {
            var token = HttpContext.Session.GetString("JWToken");
            if (token == null) return RedirectToAction("Login", "Account");

            var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            var roleClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role || c.Type == "role");

            if (roleClaim?.Value != "Admin") return RedirectToAction("Index");

            var response = await _api.GetAsync("dashboard/summary", token);
            if (string.IsNullOrEmpty(response) || response.StartsWith("<!DOCTYPE"))
            {
                 return View("Index", new DashboardViewModel());
            }

            var model = JsonConvert.DeserializeObject<DashboardViewModel>(response);
            return View("Index", model); // We use Index.cshtml as the Admin view
        }

        [HttpGet]
        [Route("/TeamLeader/Dashboard")]
        public async Task<IActionResult> TeamLeader()
        {
            var token = HttpContext.Session.GetString("JWToken");
            if (token == null) return RedirectToAction("Login", "Account");

            var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            var roleClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role || c.Type == "role");

            if (roleClaim?.Value != "TeamLeader") return RedirectToAction("Index");

            var response = await _api.GetAsync("dashboard/summary", token);
            if (string.IsNullOrEmpty(response) || response.StartsWith("<!DOCTYPE"))
            {
                 return View(new DashboardViewModel());
            }

            var model = JsonConvert.DeserializeObject<DashboardViewModel>(response);
            return View(model); // Will look for TeamLeader.cshtml
        }

        [HttpGet]
        [Route("/Employee/Dashboard")]
        public IActionResult Employee()
        {
            var token = HttpContext.Session.GetString("JWToken");
            if (token == null) return RedirectToAction("Login", "Account");

            var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            var roleClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role || c.Type == "role");

            if (roleClaim?.Value != "Employee") return RedirectToAction("Index");

            return View();
        }
    }
}