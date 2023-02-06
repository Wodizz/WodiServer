using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WodiServer;

namespace GameServer.Logic
{
    /// <summary>
    /// 逻辑层处理数据接口
    /// 所有逻辑层类必须继承该接口
    /// </summary>
    public interface IHandler
    {
        /// <summary>
        /// 逻辑层接收数据方法 只需要关心子操作码与数据
        /// </summary>
        void OnRecive(ClientPeer client, int subCode, object value);

        /// <summary>
        /// 逻辑层断开连接方法
        /// </summary>
        void OnDisconnect(ClientPeer client);
    }
}
