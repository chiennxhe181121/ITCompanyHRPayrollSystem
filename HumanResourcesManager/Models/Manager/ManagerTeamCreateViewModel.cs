using HumanResourcesManager.BLL.DTOs;

namespace HumanResourcesManager.Models.Manager
{
    public class ManagerTeamCreateViewModel
    {
        public int? SelectedEmployeeId { get; set; }
        public List<EmployeeDTO> AvailableEmployees { get; set; } = new();
    }
}
