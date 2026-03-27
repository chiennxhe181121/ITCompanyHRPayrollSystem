using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace HumanResourcesManager.BLL.DTOs.Employee
{
    public class OTCheckInDTO
    {
        [Required]
        public int OverTimeRequestId { get; set; }

        [Required]
        public TimeSpan CheckInTime { get; set; }

        public IFormFile? CheckInImage { get; set; }
    }
}

