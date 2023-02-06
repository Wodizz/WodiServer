using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WodiServer
{
    /// <summary>
    /// 客户端连接池(和unity的对象池差不多)
    /// </summary>
    public class ClientPeerPool
    {
        // 连接池队列
        private Queue<ClientPeer> clientPeerQuene;

        public ClientPeerPool(int capacity)
        {
            clientPeerQuene = new Queue<ClientPeer>(capacity);
        }

        /// <summary>
        /// 回收客户端
        /// </summary>
        /// <param name="clientPeer"></param>
        public void RecycleClient(ClientPeer clientPeer)
        {
            clientPeerQuene.Enqueue(clientPeer);
        }


        /// <summary>
        /// 得到一个客户端
        /// </summary>
        /// <returns></returns>
        public ClientPeer GetClient()
        {
            return clientPeerQuene.Dequeue();
        }
    }
}
