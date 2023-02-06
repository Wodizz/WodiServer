using System;
using System.Threading;
using System.Text;
using System.Net;
using System.Net.Sockets;

/// <summary>
/// 通过类库的方式 可以在任何项目中引用该服务器
/// </summary>
namespace WodiServer
{
    /// <summary>
    /// 服务器端
    /// </summary>
    public class ServerPeer
    {
        private Socket serverSocket;

        // 限制客户端连接数量的信号量(即最大多少连接数的线程池)
        private Semaphore acceptSemaphore;

        // 客户端对象池(连接池)
        private ClientPeerPool clientPeerPool;

        // 应用层对象
        private IApplication application;

        public ServerPeer()
        {
            serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        /// <summary>
        /// 设置应用层
        /// </summary>
        public void SetApplication(IApplication application)
        {
            this.application = application;
        }

        /// <summary>
        /// 开启服务器
        /// </summary>
        /// <param name="port">端口号</param>
        /// <param name="maxCount">最大连接数</param>
        public void StartServer(int port, int maxCount)
        {
            // 必须要准备异常捕获
            try
            {
                // 创建信号池
                acceptSemaphore = new Semaphore(maxCount, maxCount);
                // 创建对象池
                clientPeerPool = new ClientPeerPool(maxCount);
                ClientPeer tempClient;
                for (int i = 0; i < maxCount; i++)
                {
                    tempClient = new ClientPeer();
                    tempClient.Init();
                    // 添加接收消息完成事件
                    tempClient.ReciveArgs.Completed += OnReceiveCompleted;
                    // 添加客户端消息解析完成事件
                    tempClient.ClientDecodeDataCompleteEvent += OnClientDecodeDataCompleteEvent;
                    // 添加客户端失活时断开连接事件
                    tempClient.ClientNotActiveEvent += DisconnectTargetClient;
                    // 存入连接池
                    clientPeerPool.RecycleClient(tempClient);
                }
                serverSocket.Bind(new IPEndPoint(IPAddress.Any, port));
                serverSocket.Listen(maxCount);
                Console.WriteLine("服务端启动成功...");
                Console.WriteLine("开始等待客户端连接...");
                // 声明一个连接操作
                SocketAsyncEventArgs acceptArgs = new SocketAsyncEventArgs();
                acceptArgs.Completed += OnAcceptCompleted;
                StartAcceptClient(acceptArgs);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
        }

        #region 等待客户端连接

        /// <summary>
        /// 开始等待客户端连接
        /// </summary>
        private void StartAcceptClient(SocketAsyncEventArgs socketAsyncEvent)
        {
            // AcceptAsync返回值为是否正在执行异步连接请求
            bool isAsync= serverSocket.AcceptAsync(socketAsyncEvent);
            // false同步执行完成 如果同步完成是不会触发Completed事件需要自己手动调用处理
            if (!isAsync)
            {
                // 直接处理客户端连接请求
                ProcessClientAccept(socketAsyncEvent);
            }
        }

        /// <summary>
        /// 监听客户端连接完成事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="socketAsyncEvent"></param>
        private void OnAcceptCompleted(object sender , SocketAsyncEventArgs socketAsyncEvent)
        {
            ProcessClientAccept(socketAsyncEvent);
        }


        /// <summary>
        /// 处理客户端连接请求
        /// </summary>
        private void ProcessClientAccept(SocketAsyncEventArgs socketAsyncEvent)
        {
            // 信号量计数 进入一个连接线程
            acceptSemaphore.WaitOne();
            // 得到所连接的客户端Socket
            Socket clientSocket = socketAsyncEvent.AcceptSocket;
            // 保存客户端信息
            ClientPeer client = clientPeerPool.GetClient();
            client.ClientSocket = clientSocket;
            // 通知应用层连接成功
            application.OnConnect(client);
            // 获取ip地址
            Console.WriteLine((clientSocket.RemoteEndPoint as IPEndPoint).Address + "客户端连接");
            // 开始接收客户端数据
            StartRecive(client);
            // 完成一次客户端连接请求后开始下一次
            // 将socketAsyncEvent的连接Socket置空 继续调用等待连接方法
            socketAsyncEvent.AcceptSocket = null;
            StartAcceptClient(socketAsyncEvent);
        }

        #endregion

        #region 接收数据

        /// <summary>
        /// 等待接收数据
        /// </summary>
        /// <param name="client"></param>
        private void StartRecive(ClientPeer client)
        {
            try
            {
                // 将客户端自己的接收操作对象传入
                bool isAsync = client.ClientSocket.ReceiveAsync(client.ReciveArgs);
                if (!isAsync)
                    ProcessRecive(client.ReciveArgs);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
        }

        /// <summary>
        /// 用于监听接收完成事件
        /// </summary>
        /// <param name="socketAsyncEventArgs"></param>
        private void OnReceiveCompleted(object sender, SocketAsyncEventArgs socketAsyncEventArgs)
        {
            ProcessRecive(socketAsyncEventArgs);
        }


        /// <summary>
        /// 处理接收操作
        /// </summary>
        /// <param name="socketAsyncEventArgs"></param>
        private void ProcessRecive(SocketAsyncEventArgs socketAsyncEventArgs)
        {
            // UserToken 获取或设置与此异步套接字操作关联的用户或对象。
            ClientPeer client = socketAsyncEventArgs.UserToken as ClientPeer;
            // 判断消息接收结果为Success 并且传输的字节数大于0
            if (client.ReciveArgs.SocketError == SocketError.Success && client.ReciveArgs.BytesTransferred > 0)
            {
                // 让对应客户端对象处理自己的数据包
                client.ProcessReciveData();
                // 尾递归 继续接收客户端发送的数据
                StartRecive(client);
            }
            // Socket.Receive()方法在[连接的另一方]断开时，它返回结果告知只读了0个字节
            else if (client.ReciveArgs.BytesTransferred == 0)
            {
                // 如果消息接收结果是Success 但是没有传输数据 代表客户端主动断开连接
                if (client.ReciveArgs.SocketError == SocketError.Success)
                {
                    // 断开与目标客户端的连接
                    DisconnectTargetClient(client, "客户端主动断开连接");
                }
                // 网络异常
                else
                {
                    // 被动断开与目标客户端的连接
                    DisconnectTargetClient(client, client.ReciveArgs.SocketError.ToString());
                }
            }
        }

        /// <summary>
        /// 监听客户端数据解析完成事件
        /// </summary>
        private void OnClientDecodeDataCompleteEvent(ClientPeer client, SocketMessage message)
        {
            // 将数据转换给应用层使用
            application.OnRecive(client, message);
        }

        #endregion

        #region 断开连接

        /// <summary>
        /// 断开与指定客户端的连接
        /// </summary>
        /// <param name="client">断开的连接对象</param>
        /// <param name="reason">断开原因</param>
        public void DisconnectTargetClient(ClientPeer client, string reason)
        {
            try
            {
                if (client == null)
                    throw new Exception("当前指定的客户端连接对象为空，无法断开连接");
                // 通知应用层 该客户端断开连接
                application.OnDisconnect(client);
                client.ClientDisconnect();
                // 客户端对象池回收 信号池回收
                clientPeerPool.RecycleClient(client);
                acceptSemaphore.Release();
                Console.WriteLine(reason);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
        }



        #endregion
    }
}
