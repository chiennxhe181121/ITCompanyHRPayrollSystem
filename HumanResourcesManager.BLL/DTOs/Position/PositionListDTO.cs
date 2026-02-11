using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HumanResourcesManager.BLL.DTOs.Position
{
    public class PositionListDTO
    {
        [Display(Name = "ID")]
        public int PositionId { get; set; }

        [Display(Name = "Tên chức vụ")]
        public string PositionName { get; set; } = null!;

        [Display(Name = "Mô tả")]
        public string? Description { get; set; }

        [Display(Name = "Lương cơ bản")]
        [DisplayFormat(DataFormatString = "{0:N0} ₫")]
        public decimal BaseSalary { get; set; }

        [Display(Name = "Trạng thái")]
        public int Status { get; set; }

        [Display(Name = "Số nhân viên")]
        public int EmployeeCount { get; set; }
    }
}
