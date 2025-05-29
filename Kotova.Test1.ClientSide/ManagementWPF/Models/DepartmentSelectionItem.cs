// Updated models for department-based selection
// Add these to your existing Models files

using Kotova.Test1.ClientSide.ManagementWPF.ViewModels;
using System.ComponentModel.DataAnnotations;

namespace Kotova.Test1.ClientSide.ManagementWPF.Models
{
    // New model for department selection
    public class DepartmentSelectionItem : BaseViewModel
    {
        private bool _isSelected;

        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public int ChiefsCount { get; set; }
        public List<ChiefInfo> Chiefs { get; set; } = new List<ChiefInfo>();

        public string DisplayText => $"{DepartmentName} ({ChiefsCount} руководителей)";

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                OnPropertyChanged();
            }
        }
    }

    public class ChiefInfo
    {
        public int UserId { get; set; }
        public string FullName { get; set; }
        public string Role { get; set; }
        public string JobPosition { get; set; }
    }

    // Updated package for sending to backend
    public class UnplannedInstructionForDepartmentsPackage
    {
        [Required]
        public InstructionCreateDto Instruction { get; set; }

        [Required]
        [MinLength(1, ErrorMessage = "At least one department must be selected")]
        public List<int> SelectedDepartmentIds { get; set; }

        public List<string> FilePaths { get; set; }

        public List<int> NormativeInstructionIds { get; set; }
    }
}