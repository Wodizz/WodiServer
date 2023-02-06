using WodiServer;
using System;

namespace GameServer
{
    class Program
    {
        static void Main(string[] args)
        {
            // 创建服务端
            ServerPeer serverPeer = new ServerPeer();
            // 设置应用层
            serverPeer.SetApplication(new NetMessageCenter());
            // 开启服务端监听
            serverPeer.StartServer(9999, 10);
            // 阻塞主线程
            while (true)
            {
                ConsoleKeyInfo pressKey = Console.ReadKey();
                if (pressKey.Key == ConsoleKey.Escape)
                    break;
            }

        }
    }
}
