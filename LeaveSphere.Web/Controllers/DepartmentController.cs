using LeaveSphere.Web.Models.ViewModels;
using LeaveSphere.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LeaveSphere.Web.Controllers
{
    [Route("Department")]
    public class DepartmentController : Controller
    {
        private readonly ApiService _api;

        public DepartmentController(ApiService api)
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

            var response = await _api.GetAsync("Department", token);

            if (string.IsNullOrEmpty(response) || !response.Trim().StartsWith("["))
            {
                ViewBag.Message = response?.Contains("Error") == true ? response : "Unexpected response from API";
                return View(new List<DepartmentViewModel>());
            }

            var departments = JsonConvert.DeserializeObject<List<DepartmentViewModel>>(response);
            return View(departments);
        }

        [HttpGet]
        [Route("Create")]
        public IActionResult Create()
        {
            var token = HttpContext.Session.GetString("JWToken");
            if (token == null) return RedirectToAction("Login", "Account");

            return View(new DepartmentViewModel());
        }

        [HttpPost]
        [Route("Create")]
        public async Task<IActionResult> Create(DepartmentViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var token = HttpContext.Session.GetString("JWToken");
            if (token == null) return RedirectToAction("Login", "Account");

            var result = await _api.PostAsync("Department", model, token);

            if (result.Contains("Error") || result.Contains("BadRequest"))
            {
                ViewBag.Message = "Error creating department: " + result;
                return View(model);
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        [Route("Edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            var token = HttpContext.Session.GetString("JWToken");
            if (token == null) return RedirectToAction("Login", "Account");

            var response = await _api.GetAsync($"Department/{id}", token);
            if (response.Contains("Error") || string.IsNullOrEmpty(response))
            {
                return RedirectToAction("Index");
            }

            var department = JsonConvert.DeserializeObject<DepartmentViewModel>(response);
            return View(department);
        }

        [HttpPost]
        [Route("Edit/{id}")]
        public async Task<IActionResult> Edit(int id, DepartmentViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var token = HttpContext.Session.GetString("JWToken");
            if (token == null) return RedirectToAction("Login", "Account");

            var result = await _api.PutAsync($"Department/{id}", model, token);

            if (result.Contains("Error"))
            {
                ViewBag.Message = "Error updating department: " + result;
                return View(model);
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [Route("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete([FromForm] int id)
        {
            var token = HttpContext.Session.GetString("JWToken");
            if (token == null) return RedirectToAction("Login", "Account");

            var result = await _api.DeleteAsync($"Department/{id}", token);

            if (result.Contains("Error"))
            {
                var cleanError = result.Replace("Error: ", "").Replace("BadRequest -", "").Trim();
                TempData["Error"] = "Could not delete department: " + cleanError;
            }
            else
            {
                TempData["Success"] = "Department deleted successfully!";
            }

            return RedirectToAction("Index");
        }
    }
}
