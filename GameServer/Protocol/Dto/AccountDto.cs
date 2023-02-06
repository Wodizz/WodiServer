using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Protocol.Dto
{
    /// <summary>
    /// 帐号数据传输对象 用于model转换在网络层传输
    /// </summary>
    [Serializable]
    public class AccountDto
    {
        public string account;
        public string password;

        // 无参构造 方便传输时使用
        public AccountDto()
        {

        }

        public AccountDto(string acc, string pwd)
        {
            this.account = acc;
            this.password = pwd;
        }
    }
}
