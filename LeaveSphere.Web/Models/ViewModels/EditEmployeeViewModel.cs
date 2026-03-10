using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace LeaveSphere.Web.Models.ViewModels
{
    public class EditEmployeeViewModel
    {
        public int EmployeeId { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [MaxLength(100)]
        [Display(Name = "Full Name")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [MaxLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Role is required")]
        public string Role { get; set; } = "Employee";

        [Required(ErrorMessage = "Department is required")]
        [Display(Name = "Department")]
        public int DepartmentId { get; set; }
        
        public IEnumerable<SelectListItem>? Departments { get; set; }
    }
}
