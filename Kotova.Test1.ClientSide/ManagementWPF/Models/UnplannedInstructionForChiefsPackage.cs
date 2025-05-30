using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Kotova.Test1.ClientSide.ManagementWPF.Models
{

    public class ChiefStatusDto
    {
        public string ChiefName { get; set; }
        public string DepartmentName { get; set; }
        public string JobPosition { get; set; }
        public bool IsPassed { get; set; }
        public DateTime? DatePassed { get; set; }
        public DateTime? DateAssigned { get; set; }
    }
}