using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Xml;
using System.Security.Cryptography;
using System.Runtime.Serialization.Formatters.Binary;
using Newtonsoft.Json;

namespace WodiServer
{
    /// <summary>
    /// 编码工具类
    /// </summary>
    public static class EncodeTool
    {
        #region 构造与解析消息数据包

        /// <summary>
        /// 构造消息体：消息头 + 消息尾 
        /// </summary>
        /// <returns></returns>
        public static byte[] EncodeMessage(byte[] data)
        {
            // 字节流对象 存储空间为内存
            using MemoryStream memoryStream = new MemoryStream();
            // 构造二进制写入对象
            using BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
            // 先写入长度
            binaryWriter.Write(data.Length);
            // 再写入数据
            binaryWriter.Write(data);
            // 构造返回数组
            byte[] byteArray = new byte[(int)memoryStream.Length];
            // 复制字节数组 类似Array.Copy 但比它性能更高
            Buffer.BlockCopy(memoryStream.GetBuffer(), 0, byteArray, 0, (int)memoryStream.Length);
            return byteArray;
        }

        /// <summary>
        /// 解析消息体 从缓存中取出完整数据包
        /// </summary>
        /// <param name="dataCache">数据缓存区</param>
        /// <returns></returns>
        public static byte[] DecodeMessage(ref List<byte> dataCache)
        {
            // 因为单个int长度为4 小于4不能构成一个完整的消息(连消息头都算不上)
            if (dataCache.Count < 4)
            {
                //Console.WriteLine("消息长度小于4字节，不能构成完整消息");
                return null;
            }
            // 将缓存区放入字节流对象
            using MemoryStream memoryStream = new MemoryStream(dataCache.ToArray());
            using BinaryReader binaryReader = new BinaryReader(memoryStream);
            // 读第一个int数据 获得消息头长度 此时Position索引加了4位
            int messageLenth = binaryReader.ReadInt32();
            // 如果目前缓冲区的数据包长度 小于消息头定义的长度 则表示没有传输完成
            int dataRemainLenth = (int)(memoryStream.Length - memoryStream.Position);
            if (messageLenth > dataRemainLenth)
            {
                //Console.WriteLine("数据长度不满足消息头所定义长度，不能构成完整消息");
                return null;
            }
            byte[] messageData = binaryReader.ReadBytes(messageLenth);
            // 更新缓存区 将剩余的数据重新放入缓存区
            dataCache.Clear();
            dataCache.AddRange(binaryReader.ReadBytes(dataRemainLenth));
            return messageData;
        }

        #endregion

        #region 构造与解析网络消息类

        /// <summary>
        /// 将SocketMessage对象转换成数据
        /// </summary>
        /// <param name="socketMessage"></param>
        /// <returns></returns>
        public static byte[] EncodeSocketMessage(SocketMessage socketMessage)
        {
            // 构造二进制写入对象
            using MemoryStream memoryStream = new MemoryStream();
            using BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
            // 写入操作码和子操作码
            binaryWriter.Write(socketMessage.OperationCode);
            binaryWriter.Write(socketMessage.SubOperationCode);
            // 存在数据 将object转换为字节数组
            if (socketMessage.Value != null)
            {
                // 将数据对象序列化
                byte[] valueBytes = EncodeObject(socketMessage.Value);
                binaryWriter.Write(valueBytes);
            }
            byte[] data = new byte[memoryStream.Length];
            Buffer.BlockCopy(memoryStream.GetBuffer(), 0, data, 0 ,(int)memoryStream.Length);
            return data;
        }

        /// <summary>
        /// 将收到的数据转换为SocketMessage对象
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static SocketMessage DecodeSocketMessage(byte[] data)
        {
            // 构造二进制读取对象
            using MemoryStream memoryStream = new MemoryStream(data);
            using BinaryReader binaryReader = new BinaryReader(memoryStream);
            SocketMessage socketMessage = new SocketMessage();
            // 操作码各读一个int
            socketMessage.OperationCode = binaryReader.ReadInt32();
            socketMessage.SubOperationCode = binaryReader.ReadInt32();
            // 如果还有剩余的数据没读取 代表value有值
            if (memoryStream.Length > memoryStream.Position)
            {
                byte[] valueBytes = binaryReader.ReadBytes((int)(memoryStream.Length - memoryStream.Position));
                // 将字节数组反序列化成对象
                object value = DecodeObject(valueBytes);
                socketMessage.Value = value;
            }
            return socketMessage;
        }

        #endregion

        #region 对象序列化与反序列化

        /// <summary>
        /// 序列化对象
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private static byte[] EncodeObject(object obj)
        {
            //string objJson = JsonConvert.SerializeObject(obj);
            //byte[] objJsonBytes = Encoding.UTF8.GetBytes(objJson);
            //string objBase64Str = Convert.ToBase64String(objJsonBytes);
            //return Encoding.UTF8.GetBytes(objBase64Str);

            // 声明一块内存容器
            using (MemoryStream memoryStream = new MemoryStream())
            {
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                binaryFormatter.Serialize(memoryStream, obj);
                byte[] data = memoryStream.GetBuffer();
                return data;
            }

        }

        /// <summary>
        /// 反序列化对象
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private static object DecodeObject(byte[] data)
        {
            // 声明一块内存容器
            using (MemoryStream memoryStream = new MemoryStream(data))
            {
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                object obj = binaryFormatter.Deserialize(memoryStream);
                return obj;
            }
        }

        #endregion
    }
}
