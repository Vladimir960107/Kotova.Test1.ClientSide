using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Kotova.Test1.ClientSide.ManagementWPF.Models
{
    public class UnplannedInstructionForChiefsPackage
    {
        [Required]
        public InstructionCreateDto Instruction { get; set; }

        [Required]
        [MinLength(1, ErrorMessage = "At least one chief must be selected")]
        public List<int> SelectedChiefIds { get; set; }

        public List<string> FilePaths { get; set; }

        public List<int> NormativeInstructionIds { get; set; }
    }

    public class InstructionCreateDto
    {
        [Required]
        [StringLength(500)]
        public string CauseOfInstruction { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        [Range(0, 5)]
        public byte TypeOfInstruction { get; set; }
    }

    public class UnplannedInstructionStatusDto
    {
        public int InstructionId { get; set; }
        public string CauseOfInstruction { get; set; }
        public DateTime BeginDate { get; set; }
        public DateTime EndDate { get; set; }
        public string TypeName { get; set; }
        public int TotalAssigned { get; set; }
        public int TotalPassed { get; set; }
        public List<ChiefStatusDto> ChiefStatuses { get; set; } = new List<ChiefStatusDto>();
    }

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