using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Model
{
    /// <summary>
    /// 帐号数据模型
    /// </summary>
    public class AccountModel
    {
        private int id;
        private string account;
        private string password;

        public AccountModel(int id, string account, string password)
        {
            this.id = id;
            this.account = account;
            this.password = password;
        }

        public int Id { get => id; set => id = value; }
        public string Account { get => account; set => account = value; }
        public string Password { get => password; set => password = value; }
    }
}
