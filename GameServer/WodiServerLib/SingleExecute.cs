using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WodiServer
{
    /// <summary>
    /// 单线程池
    /// 强制单线程执行某个任务
    /// </summary>
    public class SingleExecute
    {
        #region 多线程单例

        private static volatile SingleExecute instance;
        private static readonly object locker = new();
        public static SingleExecute Instance
        {
            get
            {
                // 多线程独立访问判断是否为空
                if (instance == null)
                {
                    // 单一线程进锁
                    lock (locker)
                    {
                        // 再次判断是否为空
                        if (instance == null)
                            instance = new SingleExecute();
                    }
                }
                // 多线程返回同一单例
                return instance;
            }
        }
        #endregion

        /// <summary>
        /// 互斥锁
        /// </summary>
        public Mutex mutex;

        private SingleExecute()
        {
            mutex = new Mutex();
        }

        /// <summary>
        /// 单线程处理逻辑
        /// </summary>
        public void Execute(Action executeEvent)
        {
            // 锁同一应用程序中的不同线程
            lock (this)
            {
                // 锁不同应用程序中的不同线程
                mutex.WaitOne();
                executeEvent?.Invoke();
                mutex.ReleaseMutex();
            }
        }
    }
}
