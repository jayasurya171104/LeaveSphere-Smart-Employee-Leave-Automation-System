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

            if (token == null)
                return RedirectToAction("Login", "Account");

            var response = await _api.GetAsync("dashboard/summary", token);

            if (string.IsNullOrEmpty(response) || response.StartsWith("<!DOCTYPE"))
            {
                 // Handle error (maybe redirect or show error)
                 return View(new DashboardViewModel());
            }

            var model = JsonConvert.DeserializeObject<DashboardViewModel>(response);

            return View(model);
        }
    }
}