using System;
using System.ComponentModel.DataAnnotations;

namespace HumanResourcesManager.BLL.DTOs.Manager
{
    public class OTScheduleCreateDTO
    {
        [Required(ErrorMessage = "Vui lòng nhập tên Lịch OT")]
        [MaxLength(200, ErrorMessage = "Tên Lịch OT không quá 200 ký tự")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500, ErrorMessage = "Mô tả không quá 500 ký tự")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập số lượng")]
        [Range(1, 100, ErrorMessage = "Số lượng phải từ 1 đến 100")]
        public int Quantity { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ngày bắt đầu")]
        public DateTime StartDate { get; set; }
 
        [Required(ErrorMessage = "Vui lòng chọn ngày kết thúc")]
        public DateTime EndDate { get; set; }
 
        [Required(ErrorMessage = "Vui lòng Chọn giờ bắt đầu")]
        public TimeSpan StartTime { get; set; }

        [Range(1, 8, ErrorMessage = "Số giờ OT phải từ 1 đến 8 tiếng")]
        public int DurationHours { get; set; } = 1;

        [Required(ErrorMessage = "Vui lòng chọn giờ kết thúc")]
        public TimeSpan EndTime { get; set; }

        public int Status { get; set; }
    }
}
