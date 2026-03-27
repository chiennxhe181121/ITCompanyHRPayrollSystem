using System.Collections.Generic;
using HumanResourcesManager.BLL.DTOs.Manager;

namespace HumanResourcesManager.Models.Manager
{
    public class ManagerScheduleIndexViewModel
    {
        public List<OTScheduleDTO> Items { get; set; } = new();

        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
    }
}

