using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Exiled.API.Interfaces;

namespace DAONPowerOutage
{
    public class Config : IConfig
    {
        public bool IsEnabled { get; set; } = true;

        public bool Debug { get; set; } = false;

        [Description("이 옵션을 활성화하려면 GGUtils 플러그인이 필요합니다.")]
        public bool IsLovelyArloEnabled { get; set; } = false;
    }
}
