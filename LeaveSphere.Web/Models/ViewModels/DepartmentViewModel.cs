using System.ComponentModel.DataAnnotations;

namespace LeaveSphere.Web.Models.ViewModels
{
    public class DepartmentViewModel
    {
        public int DepartmentId { get; set; }

        [Required(ErrorMessage = "Department Name is required")]
        [Display(Name = "Department Name")]
        public string DepartmentName { get; set; } = string.Empty;
    }
}
