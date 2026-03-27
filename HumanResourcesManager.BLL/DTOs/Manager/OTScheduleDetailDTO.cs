using System.Collections.Generic;

namespace HumanResourcesManager.BLL.DTOs.Manager
{
    public class OTScheduleDetailDTO : OTScheduleDTO
    {
        public List<OTScheduleRegistrationDTO> Registrations { get; set; } = new List<OTScheduleRegistrationDTO>();
    }
}
