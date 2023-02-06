using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using WodiServer;
using GameServer.Proxy;
using Protocol.Code;
using Protocol.Dto;

namespace GameServer.Logic
{
    public class AccountHandler : IHandler
    {
        private AccountDto accountDto;

        public void OnDisconnect(ClientPeer client)
        {
            // 判断是在线的 不然ping会出问题
            if (Proxys.AccountProxy.IsOnline(client))
            {
                Console.WriteLine("ID:" + Proxys.AccountProxy.GetAccountId(client) + "断开连接");
                // 调用下线方法
                Proxys.AccountProxy.Offline(client);
            }
            
        }

        public void OnRecive(ClientPeer client, int subCode, object value)
        {
            Console.WriteLine(string.Format("帐号操作请求, 子操作码:{0}", subCode));
            accountDto = value as AccountDto;
            switch (subCode)
            {
                case AccountCode.LOGIN_CREQ:
                    Login(client, accountDto.account, accountDto.password);
                    break;
                case AccountCode.LOGIN_SRES:
                    break;
                case AccountCode.REGIST_CREQ:
                    Regist(client, accountDto.account, accountDto.password);
                    break;
                case AccountCode.REGIST_SRES:
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 注册
        /// </summary>
        /// <param name="client"></param>
        /// <param name="acc"></param>
        /// <param name="pwd"></param>
        private void Regist(ClientPeer client, string acc, string pwd)
        {
            // 已存在帐号
            if (Proxys.AccountProxy.IsExistAccount(acc))
            {
                client.SendMessageToClient(OperationCode.ACCOUNT, AccountCode.REGIST_SRES, -1);
                return;
            }
            // 合法校验
            if (string.IsNullOrEmpty(acc) || string.IsNullOrEmpty(pwd))
            {
                client.SendMessageToClient(OperationCode.ACCOUNT, AccountCode.REGIST_SRES, -2);
                return;
            }
            // 注册完成
            Proxys.AccountProxy.CreateAccount(acc, pwd);
            client.SendMessageToClient(OperationCode.ACCOUNT, AccountCode.REGIST_SRES, 0);
        }

        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="client"></param>
        /// <param name="acc"></param>
        /// <param name="pwd"></param>
        private void Login(ClientPeer client, string acc, string pwd)
        {
            // 不存在帐号
            if (!Proxys.AccountProxy.IsExistAccount(acc))
            {
                client.SendMessageToClient(OperationCode.ACCOUNT, AccountCode.LOGIN_SRES, -1);
                return;
            }
            // 帐号密码不匹配
            if (!Proxys.AccountProxy.IsMatch(acc, pwd))
            {
                client.SendMessageToClient(OperationCode.ACCOUNT, AccountCode.LOGIN_SRES, -2);
                return;
            }
            // 已在线
            if (Proxys.AccountProxy.IsOnline(acc))
            {
                client.SendMessageToClient(OperationCode.ACCOUNT, AccountCode.LOGIN_SRES, -3);
                return;
            }
            Proxys.AccountProxy.Online(client, acc);
            client.SendMessageToClient(OperationCode.ACCOUNT, AccountCode.LOGIN_SRES, 0);
        }
    }
}
