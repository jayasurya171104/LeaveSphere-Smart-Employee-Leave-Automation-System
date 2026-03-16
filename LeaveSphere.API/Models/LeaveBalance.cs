using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System;
using System.Collections.Generic;

namespace LeaveSphere.API.Models
{
    public class LeaveBalance
    {
        [Key]
        public int BalanceId { get; set; }

        public int? EmployeeId { get; set; }

        [ForeignKey("EmployeeId")]
        [System.Text.Json.Serialization.JsonIgnore]
        public Employee? Employee { get; set; }

        public int? TeamLeaderId { get; set; }

        [ForeignKey("TeamLeaderId")]
        [System.Text.Json.Serialization.JsonIgnore]
        public TeamLeader? TeamLeader { get; set; }

        public int TotalLeaves { get; set; } = 20;

        public int UsedLeaves { get; set; } = 0;

        public int RemainingLeaves { get; set; } = 20;
    }
}