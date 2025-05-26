using Kotova.Test1.ClientSide.ManagementWPF.ViewModels;
using System.Collections.Generic;

namespace Kotova.Test1.ClientSide.ManagementWPF.Models
{
    public class DepartmentWithChiefsDto
    {
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public List<ChiefDto> Chiefs { get; set; } = new List<ChiefDto>();
    }

    public class ChiefDto
    {
        public int UserId { get; set; }
        public int PersonnelId { get; set; }
        public string Role { get; set; }
        public string FullName { get; set; }
        public string JobPosition { get; set; }
    }

    public class ChiefSelectionItem : BaseViewModel
    {
        private bool _isSelected;

        public int ChiefId { get; set; }
        public int DepartmentId { get; set; }
        public string ChiefName { get; set; }
        public string DepartmentName { get; set; }
        public string Role { get; set; }
        public string JobPosition { get; set; }

        public string DisplayText => $"{DepartmentName} - {ChiefName} ({Role})";

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