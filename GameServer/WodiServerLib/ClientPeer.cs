using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using System.Net.Sockets;


namespace WodiServer
{
    /// <summary>
    /// 抽象的客户端
    /// </summary>
    public class ClientPeer
    {
        #region 字段

        private Socket clientSocket;

        // 接收数据缓冲区
        private byte[] reciveBuffer = new byte[1024];

        // 数据缓存区
        private List<byte> dataCache = new List<byte>();

        // 是否正在处理接收的数据
        private bool isSolveData = false;

        // 发送消息队列
        private Queue<byte[]> SendMessageQuene = new Queue<byte[]>();

        // 是否正在发送数据
        private bool isSendData = false;

        #endregion

        #region 属性

        // 客户端套接字
        public Socket ClientSocket 
        {
            get { return clientSocket; } 
            set { clientSocket = value; } 
        }

        // 客户端接收操作
        public SocketAsyncEventArgs ReciveArgs { get; set; }

        // 客户端发送操作
        public SocketAsyncEventArgs SendArgs { get; set; }

        // 客户端解析数据完成回调事件
        public Action<ClientPeer, SocketMessage> ClientDecodeDataCompleteEvent { get; set; }

        // 客户端失活事件
        public Action<ClientPeer, string> ClientNotActiveEvent { get; set; }

        #endregion


        public void Init()
        {
            ReciveArgs = new SocketAsyncEventArgs();
            SendArgs = new SocketAsyncEventArgs();
            // 设置接收数据缓冲区
            ReciveArgs.SetBuffer(reciveBuffer, 0, reciveBuffer.Length);
            // 将关联对象设置为自己
            ReciveArgs.UserToken = this;
            SendArgs.UserToken = this;
            // 添加事件监听
            SendArgs.Completed += OnSendCompletedEvent;
        }

        #region 接收数据 

        /// <summary>
        /// 处理接收的数据包
        /// </summary>
        /// <param name="reciveData"></param>
        public void ProcessReciveData()
        {
            // 创建一个传输大小的字节数组 将获取到的数据拷贝出来
            //Console.WriteLine("获取到的数据字节数:" + ReciveArgs.BytesTransferred);
            byte[] r_data = new byte[ReciveArgs.BytesTransferred];
            Buffer.BlockCopy(ReciveArgs.Buffer, ReciveArgs.Offset, r_data, 0, ReciveArgs.BytesTransferred);
            dataCache.AddRange(r_data);
            // 只要没在处理数据就调用
            if (!isSolveData)
                SolveDataCache();
        }

        /// <summary>
        /// 解决缓存区数据
        /// </summary>
        private void SolveDataCache()
        {
            isSolveData = true;
            byte[] messageData = EncodeTool.DecodeMessage(ref dataCache);
            // 如果为空代表解析失败
            if (messageData == null)
            {
                isSolveData = false;
                return;
            }
            // 将数据转换为封装的接口消息类
            SocketMessage socketMessage = EncodeTool.DecodeSocketMessage(messageData);
            // 单个数据解析成功 回调给上层
            ClientDecodeDataCompleteEvent?.Invoke(this, socketMessage);
            // 尾递归 继续解决数据
            SolveDataCache();
        }


        #endregion


        #region 断开连接

        /// <summary>
        /// 客户端断开连接
        /// </summary>
        public void ClientDisconnect()
        {
            // 清空缓存
            dataCache.Clear();
            isSolveData = false;
            // 通知断开连接

            // 先Shutdown再Close
            // 先禁用数据发送和接受 再断开Socket连接
            clientSocket.Shutdown(SocketShutdown.Both);
            clientSocket.Close();
            clientSocket = null;
        }

        #endregion

        #region 发送数据

        /// <summary>
        /// 给客户端发送一条消息
        /// </summary>
        /// <param name="opCode">操作码</param>
        /// <param name="subCode">子操作码</param>
        /// <param name="value">数据</param>
        public void SendMessageToClient(int opCode, int subCode, object value)
        {
            // 构造一个消息对象
            SocketMessage message = new SocketMessage(opCode, subCode, value);
            // 转换成消息字节数组
            byte[] messageBytes = EncodeTool.EncodeSocketMessage(message);
            // 转换成标准数据包(消息头+消息尾)
            byte[] messageData = EncodeTool.EncodeMessage(messageBytes);
            // 存入消息队列
            SendMessageQuene.Enqueue(messageData);
            // 类似处理接收数据 循环递归处理发送数据
            if (!isSendData)
                SolveSendData();
        }

        /// <summary>
        /// 处理发送数据
        /// </summary>
        private void SolveSendData()
        {
            isSendData = true;
            // 不存在消息数据
            if (SendMessageQuene.Count == 0)
            {
                isSendData = false;
                return;
            }
            // 取出一条数据
            byte[] sendData = SendMessageQuene.Dequeue();
            // 存入发送操作的缓存区
            SendArgs.SetBuffer(sendData, 0, sendData.Length);
            // 进行发送操作
            bool isAsync = clientSocket.SendAsync(SendArgs);
            if (!isAsync)
                SendCompleted();
        }

        /// <summary>
        /// 监听异步发送完成事件
        /// </summary>
        private void OnSendCompletedEvent(object sender, SocketAsyncEventArgs socketAsyncEventArgs)
        {
            SendCompleted();
        }

        /// <summary>
        /// 发送完成调用方法
        /// </summary>
        private void SendCompleted()
        {
            // 判断发送结果
            if (SendArgs.SocketError != SocketError.Success)
            {
                // 发送出错 调用客户端失活事件 与客户端断开连接
                ClientNotActiveEvent?.Invoke(this, SendArgs.SocketError.ToString());
                return;
            }
            // 发送成功了继续递归
            SolveSendData();
        }

        #endregion
    }
}
