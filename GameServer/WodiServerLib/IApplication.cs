using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WodiServer
{
    /// <summary>
    /// 应用层接口
    /// </summary>
    public interface IApplication
    {
        /// <summary>
        /// 断开连接
        /// </summary>
        void OnDisconnect(ClientPeer client);

        /// <summary>
        /// 接收数据
        /// </summary>
        void OnRecive(ClientPeer client, SocketMessage message);

        /// <summary>
        /// 连接成功
        /// </summary>
        /// <param name="client"></param>
        void OnConnect(ClientPeer client);
    }
}
