using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Protocol.Code
{
    /// <summary>
    /// 帐号子操作码
    /// 数据参数：AccountDto
    /// </summary>
    public class AccountCode
    {
        /// <summary>
        /// regist client request 客户端注册请求
        /// </summary>
        public const int REGIST_CREQ = 0;
        /// <summary>
        /// regist server response 服务端注册响应
        /// </summary>
        public const int REGIST_SRES = 1;
        /// <summary>
        /// login client request 客户端登录请求
        /// </summary>
        public const int LOGIN_CREQ = 2;
        /// <summary>
        /// login server request 服务端登录响应
        /// </summary>
        public const int LOGIN_SRES = 3;
    }
}
