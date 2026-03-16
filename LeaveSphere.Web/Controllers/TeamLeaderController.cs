using LeaveSphere.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using LeaveSphere.Web.Models.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LeaveSphere.Web.Controllers
{
    [Route("TeamLeader")]
    public class TeamLeaderController : Controller
    {
        private readonly ApiService _api;

        public TeamLeaderController(ApiService api)
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

            var response = await _api.GetAsync("TeamLeader", token);
            var leaders = new List<TeamLeaderViewModel>();
            
            if (!string.IsNullOrEmpty(response) && response.Trim().StartsWith("["))
            {
                leaders = JsonConvert.DeserializeObject<List<TeamLeaderViewModel>>(response);
            }

            return View(leaders ?? new List<TeamLeaderViewModel>());
        }

        [HttpGet]
        [Route("Create")]
        public async Task<IActionResult> Create()
        {
            var token = HttpContext.Session.GetString("JWToken");
            if (token == null) return RedirectToAction("Login", "Account");

            var model = new TeamLeaderViewModel();
            await PopulateDepartmentsAsync(model);
            return View(model);
        }

        [HttpPost]
        [Route("Create")]
        public async Task<IActionResult> Create(TeamLeaderViewModel model)
        {
            var token = HttpContext.Session.GetString("JWToken");
            if (token == null) return RedirectToAction("Login", "Account");

            // Password is required for creation
            if (string.IsNullOrEmpty(model.Password))
            {
                ModelState.AddModelError("Password", "Password is required for new Team Leader");
            }

            if (!ModelState.IsValid)
            {
                await PopulateDepartmentsAsync(model);
                return View(model);
            }

            model.PasswordHash = model.Password; // API will hash it
            var result = await _api.PostAsync("TeamLeader", model, token);

            if (result.Contains("Error") || result.Contains("BadRequest"))
            {
                ViewBag.Error = result;
                await PopulateDepartmentsAsync(model);
                return View(model);
            }

            TempData["Success"] = "Team Leader created successfully!";
            return RedirectToAction("Index");
        }

        [HttpGet]
        [Route("Edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            var token = HttpContext.Session.GetString("JWToken");
            if (token == null) return RedirectToAction("Login", "Account");

            var response = await _api.GetAsync($"TeamLeader/{id}", token);
            if (string.IsNullOrEmpty(response) || response.Contains("Error")) return RedirectToAction("Index");

            var leader = JsonConvert.DeserializeObject<TeamLeaderViewModel>(response);
            await PopulateDepartmentsAsync(leader);
            return View(leader);
        }

        [HttpPost]
        [Route("Edit/{id}")]
        public async Task<IActionResult> Edit(int id, TeamLeaderViewModel model)
        {
            var token = HttpContext.Session.GetString("JWToken");
            if (token == null) return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid)
            {
                await PopulateDepartmentsAsync(model);
                return View(model);
            }

            if (!string.IsNullOrEmpty(model.Password)) model.PasswordHash = model.Password;

            var result = await _api.PutAsync($"TeamLeader/{id}", model, token);

            if (result.Contains("Error"))
            {
                ViewBag.Error = result;
                await PopulateDepartmentsAsync(model);
                return View(model);
            }

            TempData["Success"] = "Team Leader updated successfully!";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [Route("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var token = HttpContext.Session.GetString("JWToken");
            if (token == null) return RedirectToAction("Login", "Account");

            var result = await _api.DeleteAsync($"TeamLeader/{id}", token);

            if (result.Contains("Error"))
            {
                TempData["Error"] = result;
            }
            else
            {
                TempData["Success"] = "Team Leader deleted successfully!";
            }

            return RedirectToAction("Index");
        }

        private async Task PopulateDepartmentsAsync(TeamLeaderViewModel model)
        {
            var token = HttpContext.Session.GetString("JWToken");
            var response = await _api.GetAsync("Department", token);
            
            if (!string.IsNullOrEmpty(response) && response.Trim().StartsWith("["))
            {
                var departments = JsonConvert.DeserializeObject<List<DepartmentViewModel>>(response);
                ViewBag.Departments = departments?.Select(d => new SelectListItem
                {
                    Value = d.DepartmentId.ToString(),
                    Text = d.DepartmentName
                }).ToList();
            }
        }
    }
}
