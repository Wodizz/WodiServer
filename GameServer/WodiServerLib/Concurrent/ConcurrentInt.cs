using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WodiServer.Concurrent
{
    /// <summary>
    /// 线程安全的Int类型
    /// </summary>
    public class ConcurrentInt
    {
        private int value;

        public ConcurrentInt(int value)
        {
            this.value = value;
        }

        public int Add()
        {
            lock (this)
            {
                value++;
                return value;
            }
        }

        public int Reduce()
        {
            lock (this)
            {
                value--;
                return value;
            }
        }

        public int Get()
        {
            return value;
        }
    }
}
