using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HumanResourcesManager.BLL.DTOs.Position
{
    public class PositionCreateUpdateDTO
    {
        public int? PositionId { get; set; }

        [Required(ErrorMessage = "Tên chức vụ không được để trống")]
        [StringLength(100, ErrorMessage = "Tên chức vụ không quá 100 ký tự")]
        [Display(Name = "Tên chức vụ")]
        public string PositionName { get; set; } = null!;

        [Display(Name = "Mô tả")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Lương cơ bản không được để trống")]
        //[Range(1000000, double.MaxValue, ErrorMessage = "Lương cơ bản phải từ 1.000.000 VNĐ trở lên")]
        // Giới hạn Min: 1 triệu, Max: 999 Nghìn Tỷ (15 số 9)
        [Range(1000000, 999999999999999, ErrorMessage = "Lương cơ bản phải từ 1 triệu đến dưới 1 triệu tỷ VNĐ")]
        [Display(Name = "Lương cơ bản")]
        public decimal BaseSalary { get; set; }

        [Display(Name = "Trạng thái")]
        public int Status { get; set; }
    }
}
