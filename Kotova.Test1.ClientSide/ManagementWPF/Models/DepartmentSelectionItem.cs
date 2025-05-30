// Updated models for department-based selection
// Add these to your existing Models files

using Kotova.CommonClasses;
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
}