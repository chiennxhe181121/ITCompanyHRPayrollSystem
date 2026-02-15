using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HumanResourcesManager.BLL.DTOs.Allowance
{
    public class AllowanceDTO
    {
        public int AllowanceId { get; set; }

        [Required(ErrorMessage = "Tên phụ cấp không được để trống")]
        [Display(Name = "Tên phụ cấp")]
        public string AllowanceName { get; set; } = null!;

        [Required(ErrorMessage = "Số tiền không được để trống")]
        [Range(0, double.MaxValue, ErrorMessage = "Số tiền không hợp lệ")]
        [Display(Name = "Số tiền (VNĐ)")]
        public decimal Amount { get; set; }

        public int Status { get; set; } // 1: Active, 0: Inactive
    }
}
