using System.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;

namespace LeaveSphere.API.Models
{
    public class Department
    {
        [Key]
        public int DepartmentId { get; set; }



        [Required]
        [MaxLength(100)]
        public string DepartmentName { get; set; }

        [System.Text.Json.Serialization.JsonIgnore]
        public ICollection<Employee>? Employees { get; set; }
    }
}