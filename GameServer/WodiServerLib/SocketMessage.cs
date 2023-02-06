using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WodiServer
{
    /// <summary>
    /// 网络消息
    /// 发送时都要走这个类
    /// </summary>
    [Serializable]
    public class SocketMessage
    {
        #region 属性

        /// <summary>
        /// 操作码
        /// </summary>
        public int OperationCode { get; set; }

        /// <summary>
        /// 子操作码
        /// </summary>
        public int SubOperationCode { get; set; }

        /// <summary>
        /// 参数
        /// </summary>
        public object Value { get; set; }

        #endregion

        public SocketMessage()
        {

        }

        public SocketMessage(int operationCode, int subOperationCode, object value)
        {
            this.OperationCode = operationCode;
            this.SubOperationCode = subOperationCode;
            this.Value = value;
        }

    }
}
