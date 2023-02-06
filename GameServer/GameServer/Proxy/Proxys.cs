using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Proxy
{
    /// <summary>
    /// 存放所有的接口对象
    /// </summary>
    public class Proxys
    {
        private static AccountProxy accountProxy = new AccountProxy();

        public static AccountProxy AccountProxy { get => accountProxy; set => accountProxy = value; }
    }
}
