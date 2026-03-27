using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace HumanResourcesManager.BLL.DTOs.Employee
{
    public class OTCheckOutDTO
    {
        [Required]
        public int OverTimeRequestId { get; set; }

        [Required]
        public TimeSpan CheckOutTime { get; set; }

        public IFormFile? CheckOutImage { get; set; }
    }
}

