using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


// @HoangDH 
namespace HumanResourcesManager.BLL.DTOs.Department
{
    public class DepartmentCreateUpdateDTO
    {
        public int DepartmentId { get; set; }

        [Required(ErrorMessage = "Tên phòng ban không được để trống")]
        [MaxLength(100, ErrorMessage = "Tên phòng ban tối đa 100 ký tự")]
        public string DepartmentName { get; set; } = null!;

        [MaxLength(255, ErrorMessage = "Mô tả tối đa 255 ký tự")]
        public string? Description { get; set; }
    }
}
