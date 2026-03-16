using LeaveSphere.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using LeaveSphere.Web.Models.ViewModels;

namespace LeaveSphere.Web.Controllers
{
    [Route("Employee")]
    public class EmployeeController : Controller
    {
        private readonly ApiService _api;

        public EmployeeController(ApiService api)
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

            var response = await _api.GetAsync("employee", token);

            if (string.IsNullOrEmpty(response) || !response.Trim().StartsWith("["))
            {
                ViewBag.Message = response.Contains("Error") ? response : "Unexpected response from API";
                return View(new List<EmployeeViewModel>()); 
            }

            var employees = JsonConvert.DeserializeObject<List<EmployeeViewModel>>(response);

            return View(employees);
        }

        [HttpGet]
        [Route("DeptEmployees")]
        public async Task<IActionResult> DeptEmployees()
        {
            var token = HttpContext.Session.GetString("JWToken");
            if (token == null) return RedirectToAction("Login", "Account");

            var response = await _api.GetAsync("employee", token);
            if (string.IsNullOrEmpty(response) || !response.Trim().StartsWith("["))
            {
                ViewBag.Message = "No department employees found.";
                return View(new List<EmployeeViewModel>());
            }

            var employees = JsonConvert.DeserializeObject<List<EmployeeViewModel>>(response);
            return View(employees);
        }

        [HttpGet]
        [Route("Create")]
        public async Task<IActionResult> Create()
        {
            var model = new CreateEmployeeViewModel();
            await PopulateDepartmentsAsync(model);
            return View(model);
        }

        [HttpPost]
        [Route("Create")]
        public async Task<IActionResult> Create(CreateEmployeeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateDepartmentsAsync(model);
                return View(model);
            }

            var token = HttpContext.Session.GetString("JWToken");
            if (token == null)
                return RedirectToAction("Login", "Account");

            var result = await _api.PostAsync("auth/register", model, token);

            if (result.Contains("Error") || result.Contains("BadRequest") || result.Contains("already exists"))
            {
                ViewBag.Message = result.Contains("already exists") ? "Email already exists" : "Error creating employee: " + result;
                await PopulateDepartmentsAsync(model);
                return View(model);
            }

            TempData["Success"] = "Employee created successfully!";
            return RedirectToAction("Index");
        }
        [HttpGet]
        [Route("Edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            var token = HttpContext.Session.GetString("JWToken");
            if (token == null)
                return RedirectToAction("Login", "Account");

            var response = await _api.GetAsync($"employee/{id}", token);
            if (response.Contains("Error") || string.IsNullOrEmpty(response))
            {
                return RedirectToAction("Index");
            }

            var employee = JsonConvert.DeserializeObject<EditEmployeeViewModel>(response);
            if (employee != null)
            {
                await PopulateDepartmentsAsync(employee);
            }
            return View(employee);
        }

        [HttpPost]
        [Route("Edit/{id}")]
        public async Task<IActionResult> Edit(int id, EditEmployeeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateDepartmentsAsync(model);
                return View(model);
            }

            var token = HttpContext.Session.GetString("JWToken");
            if (token == null)
                return RedirectToAction("Login", "Account");

            var result = await _api.PutAsync($"employee/{id}", model, token);

            if (result.Contains("Error"))
            {
                ViewBag.Message = "Error updating employee: " + result;
                await PopulateDepartmentsAsync(model);
                return View(model);
            }

            TempData["Success"] = "Employee updated successfully!";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [Route("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var token = HttpContext.Session.GetString("JWToken");
            if (token == null)
                return RedirectToAction("Login", "Account");

            var result = await _api.DeleteAsync($"employee/{id}", token);

            if (result.Contains("Error"))
            {
                TempData["Error"] = "Error deleting employee: " + result;
            }

            if (!result.Contains("Error"))
            {
                TempData["Success"] = "Employee deleted successfully!";
            }
            return RedirectToAction("Index");
        }

        private async Task PopulateDepartmentsAsync(dynamic model)
        {
            var token = HttpContext.Session.GetString("JWToken");
            if (token == null) return;

            var response = await _api.GetAsync("Department", token);
            if (!string.IsNullOrEmpty(response) && response.Trim().StartsWith("["))
            {
                var departments = JsonConvert.DeserializeObject<List<DepartmentViewModel>>(response);
                if (departments != null)
                {
                    model.Departments = departments.Select(d => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                    {
                        Value = d.DepartmentId.ToString(),
                        Text = d.DepartmentName
                    }).ToList();
                }
            }
        }
    }
}