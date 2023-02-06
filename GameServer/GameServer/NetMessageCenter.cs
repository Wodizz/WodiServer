using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WodiServer;
using GameServer.Logic;
using Protocol.Code;

namespace GameServer
{
    /// <summary>
    /// 网络消息中心
    /// 对网络消息进行分类 转发(路由器)
    /// </summary>
    public class NetMessageCenter : IApplication
    {

        // 帐号逻辑处理对象
        private IHandler accountHandler = new AccountHandler();

        public void OnConnect(ClientPeer client)
        {
            
        }

        public void OnDisconnect(ClientPeer client)
        {
            accountHandler.OnDisconnect(client);
        }

        /// <summary>
        /// 操作码分类
        /// 将SocketMessage的value提取
        /// 所有逻辑层 不会涉及到封装的SocketMessage
        /// </summary>
        public void OnRecive(ClientPeer client, SocketMessage message)
        {
            switch (message.OperationCode)
            {
                case OperationCode.ACCOUNT:
                    accountHandler.OnRecive(client, message.SubOperationCode, message.Value);
                    break;
                default:
                    break;
            }
        }
    }
}
