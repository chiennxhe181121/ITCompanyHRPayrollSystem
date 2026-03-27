using HumanResourcesManager.BLL.DTOs;

namespace HumanResourcesManager.Models.Manager
{
    public class ManagerTeamIndexViewModel
    {
        public List<EmployeeDTO> Items { get; set; } = new();
        public string? Keyword { get; set; }
        public int? Status { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
    }
}
