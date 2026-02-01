using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HumanResourcesManager.BLL.Services
{
    public class OtpService
    {
        public string GenerateOtp()
        {
            return new Random().Next(100000, 999999).ToString();
        }
    }

}
