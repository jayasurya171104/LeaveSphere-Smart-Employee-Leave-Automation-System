using LeaveSphere.Web.Models.ViewModels;
using LeaveSphere.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace LeaveSphere.Web.Controllers
{
    [Route("Leave")]
    public class LeaveController : Controller
    {
        private readonly ApiService _api;
        private readonly IConfiguration _config;

        public LeaveController(ApiService api, IConfiguration config)
        {
            _api = api;
            _config = config;
        }

        [HttpGet]
        [Route("Calendar")]
        public IActionResult Calendar()
        {
            var token = HttpContext.Session.GetString("JWToken");
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Account");

            ViewBag.Token = token;
            ViewBag.ApiBaseUrl = _config["ApiBaseUrl"] ?? "http://localhost:5100/api/";
            return View();
        }

        [HttpGet]
        [Route("MyLeaves")]
        [Route("")]
        [Route("Index")]
        public async Task<IActionResult> MyLeaves()
        {
            var token = HttpContext.Session.GetString("JWToken");

            if (token == null)
                return RedirectToAction("Login", "Account");

            var response = await _api.GetAsync("leave/my", token);

            if (string.IsNullOrEmpty(response) || !response.Trim().StartsWith("["))
            {
                ViewBag.Message = response.Contains("Error") ? response : "Unexpected response from API";
                return View(new List<LeaveViewModel>());
            }

            var leaves = JsonConvert.DeserializeObject<List<LeaveViewModel>>(response);

            // Fetch Profile for Leave Balance
            var profileResponse = await _api.GetAsync("auth/profile", token);
            if (!string.IsNullOrEmpty(profileResponse) && profileResponse.Trim().StartsWith("{"))
            {
                var profile = JsonConvert.DeserializeObject<EmployeeViewModel>(profileResponse);
                ViewBag.Profile = profile;
            }

            return View(leaves);
        }

        [HttpGet]
        [Route("Apply")]
        public IActionResult Apply()
        {
            return View();
        }

        [HttpPost]
[ValidateAntiForgeryToken]
[Route("Apply")]
public async Task<IActionResult> Apply(LeaveViewModel model)
{
    var token = HttpContext.Session.GetString("JWToken");
    if (string.IsNullOrEmpty(token))
        return RedirectToAction("Login", "Account");

    if (!ModelState.IsValid)
        return View(model);

    if (model.StartDate.Date < DateTime.Today || model.EndDate.Date < DateTime.Today)
    {
        ViewBag.Message = "You cannot apply leave for past dates.";
        return View(model);
    }

    if (model.EndDate.Date < model.StartDate.Date)
    {
        ViewBag.Message = "End date cannot be earlier than start date.";
        return View(model);
    }

    var leave = new
    {
        LeaveType = model.LeaveType,
        StartDate = model.StartDate,
        EndDate = model.EndDate,
        Reason = model.Reason
    };

    var result = await _api.PostAsync("leave/apply", leave, token);

    if (result.Contains("Error") || result.Contains("BadRequest"))
    {
        ViewBag.Message = "Error applying leave: " + result;
        return View(model);
    }

    return RedirectToAction("MyLeaves");
}

        [HttpGet]
        [Route("Approve")]
        public async Task<IActionResult> Approve(string? status)
        {
            var token = HttpContext.Session.GetString("JWToken");
            if (token == null) return RedirectToAction("Login", "Account");
            
            var response = await _api.GetAsync("leave", token);

            if (string.IsNullOrEmpty(response) || !response.Trim().StartsWith("["))
            {
                ViewBag.Message = response.Contains("Error") ? response : "Unexpected response from API";
                return View(new List<LeaveViewModel>());
            }

            var leaves = JsonConvert.DeserializeObject<List<LeaveViewModel>>(response);

            if (!string.IsNullOrEmpty(status))
            {
                leaves = leaves.Where(l => l.Status.Equals(status, StringComparison.OrdinalIgnoreCase)).ToList();
                ViewBag.Filter = status;
            }

            return View(leaves);
        }

        [HttpPost]
        [Route("ApproveRequest/{id}")]
        public async Task<IActionResult> ApproveRequest(int id)
        {
            var token = HttpContext.Session.GetString("JWToken");
            if (token == null) return RedirectToAction("Login", "Account");
            
            await _api.PutAsync($"leave/approve/{id}", null, token);
            return RedirectToAction("Approve");
        }

        [HttpPost]
        [Route("RejectRequest/{id}")]
        public async Task<IActionResult> RejectRequest(int id)
        {
            var token = HttpContext.Session.GetString("JWToken");
            if (token == null) return RedirectToAction("Login", "Account");
            
            await _api.PutAsync($"leave/reject/{id}", null, token);
            return RedirectToAction("Approve");
        }

        [HttpGet]
        [Route("CheckConflict")]
        public async Task<IActionResult> CheckConflict(DateTime startDate, DateTime endDate)
        {
            var token = HttpContext.Session.GetString("JWToken");
            if (string.IsNullOrEmpty(token)) return Json(new { conflict = false });

            // Format dates for query
            string qs = $"?startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}";
            var response = await _api.GetAsync($"leave/check-conflict{qs}", token);

            if (string.IsNullOrEmpty(response) || response.Contains("Error"))
            {
                return Json(new { conflict = false });
            }

            var result = JsonConvert.DeserializeObject<dynamic>(response);
            return Json(new
            {
                conflict = (bool)result?.conflict,
                message = (string)result?.message
            });
        }
        [HttpGet]
        [Route("Report")]
        public async Task<IActionResult> Report(DateTime? date)
        {
            var token = HttpContext.Session.GetString("JWToken");
            if (token == null) return RedirectToAction("Login", "Account");

            var selectedDate = date ?? DateTime.Today;
            ViewBag.SelectedDate = selectedDate.ToString("yyyy-MM-dd");

            var response = await _api.GetAsync($"leave/report?date={selectedDate:yyyy-MM-dd}", token);

            if (string.IsNullOrEmpty(response) || !response.Trim().StartsWith("["))
            {
                ViewBag.Message = response.Contains("Error") ? response : "No leave records found for this date.";
                return View(new List<LeaveReportViewModel>());
            }

            var reportData = JsonConvert.DeserializeObject<List<LeaveReportViewModel>>(response);
            return View(reportData);
        }
    }
}