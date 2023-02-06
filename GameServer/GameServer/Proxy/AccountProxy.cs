using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameServer.Model;
using WodiServer;
using WodiServer.Concurrent;

namespace GameServer.Proxy
{
    /// <summary>
    /// 帐号信息代理类
    /// 用于逻辑与数据的中间交互
    /// </summary>
    public class AccountProxy
    {
        // 帐号与对应的数据模型字典
        private ConcurrentDictionary<string, AccountModel> accModelDic = new ConcurrentDictionary<string, AccountModel>();

        // 帐号与帐号对应客户端字典 用于判断是否在线
        private ConcurrentDictionary<string, ClientPeer> accClientDic = new ConcurrentDictionary<string, ClientPeer>();
        private ConcurrentDictionary<ClientPeer, string> clientAccDic = new ConcurrentDictionary<ClientPeer, string>();

        // 全局唯一的id
        private ConcurrentInt accId = new ConcurrentInt(-1);

        /// <summary>
        /// 是否存在帐号
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        public bool IsExistAccount(string account)
        {
            return accModelDic.ContainsKey(account);
        }

        /// <summary>
        /// 创建帐号
        /// </summary>
        public void CreateAccount(string account, string password)
        {
            // 每次进入id自增
            accId.Add();
            AccountModel accountModel = new AccountModel(accId.Get(), account, password);
            accModelDic.TryAdd(account, accountModel);
        }

        /// <summary>
        /// 得到帐号对应的数据模型
        /// </summary>
        public AccountModel GetModel(string account)
        {
            AccountModel accountModel;
            accModelDic.TryGetValue(account, out accountModel);
            return accountModel;
        }

        /// <summary>
        /// 帐号密码是否匹配
        /// </summary>
        public bool IsMatch(string acc, string pwd)
        {
            AccountModel accountModel;
            accModelDic.TryGetValue(acc, out accountModel);
            if (accountModel.Password == pwd)
                return true;
            return false;
        }
        
        /// <summary>
        /// 是否在线
        /// </summary>
        public bool IsOnline(string acc)
        {
            return accClientDic.ContainsKey(acc);
        }

        /// <summary>
        /// 是否在线
        /// </summary>
        public bool IsOnline(ClientPeer client)
        {
            return clientAccDic.ContainsKey(client);
        }

        /// <summary>
        /// 上线
        /// </summary>
        public void Online(ClientPeer client, string acc)
        {
            accClientDic.TryAdd(acc, client);
            clientAccDic.TryAdd(client, acc);
        }

        /// <summary>
        /// 下线
        /// </summary>
        public void Offline(ClientPeer client)
        {
            if (clientAccDic.TryRemove(client, out string acc))
                accClientDic.TryRemove(acc, out client);
        }

        /// <summary>
        /// 下线
        /// </summary>
        public void Offline(string acc)
        {
            if (accClientDic.TryRemove(acc, out ClientPeer client))
                clientAccDic.TryRemove(client, out acc);
        }

        /// <summary>
        /// 获取在线玩家的id
        /// </summary>
        public int GetAccountId(ClientPeer client)
        {
            clientAccDic.TryGetValue(client, out string acc);
            return accModelDic[acc].Id;
        }
    }
}
