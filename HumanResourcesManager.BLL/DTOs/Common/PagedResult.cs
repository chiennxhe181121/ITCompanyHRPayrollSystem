using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


// @HoangDH 
namespace HumanResourcesManager.BLL.DTOs.Common
{
    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new();
        public int TotalItems { get; set; }
    }
}

