using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using WodiServer.Concurrent;

namespace WodiServer.Until
{
    /// <summary>
    /// 定时任务管理类
    /// </summary>
    public class TimeManager
    {
        #region 多线程单例(双检锁)

        private static volatile TimeManager instance;
        private static readonly object locker = new object();
        public static TimeManager Instance
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
                            instance = new TimeManager();
                    }
                }
                // 多线程返回同一单例
                return instance;
            }
        }
        #endregion

        /// <summary>
        /// 触发任务定时器
        /// </summary>
        private Timer timer;

        /// <summary>
        /// 定时任务id
        /// </summary>
        private ConcurrentInt timeTaskId = new ConcurrentInt(-1);

        /// <summary>
        /// 线程安全的字典 可以多线程同时访问
        /// </summary>
        private ConcurrentDictionary<int, TimerModel> idModelDic = new ConcurrentDictionary<int, TimerModel>();

        /// <summary>
        /// 需要移除的任务id列表
        /// </summary>
        private List<int> removeList = new List<int>();

        public TimeManager()
        {
            timer = new Timer(10);
            timer.Elapsed += Timer_Elapsed;
        }

        /// <summary>
        /// 达到时间间隔时触发
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            // 每次触发时 进行任务移除
            lock (removeList)
            {
                TimerModel tempModel;
                for (int i = 0; i < removeList.Count; i++)
                {
                    idModelDic.TryRemove(removeList[i], out tempModel);
                }
                removeList.Clear();
            }

            foreach (var item in idModelDic.Values)
            {
                // 如果当前时间大于等于定时时间 就触发
                if (DateTime.Now.Ticks >= item.Time )
                    item.Trigger();
            }
        }

        /// <summary>
        /// 添加指定触发时间的任务
        /// </summary>
        /// <param name="dateTime">触发时间</param>
        /// <param name="timeTriggerEvent">触发事件</param>
        public void AddTimeTask(DateTime dateTime, Action timeTriggerEvent)
        {
            long delayTime = dateTime.Ticks - DateTime.Now.Ticks;
            if (delayTime > 0)
                AddTimeTask(delayTime, timeTriggerEvent);
        }

        /// <summary>
        /// 添加延迟多少时间的任务
        /// </summary>
        /// <param name="delayTime">延迟时间(毫秒)</param>
        /// <param name="timeTriggerEvent">触发事件</param>
        public void AddTimeTask(long delayTime, Action timeTriggerEvent)
        {
            TimerModel timerModel = new TimerModel(timeTaskId.Add(), DateTime.Now.Ticks + delayTime, timeTriggerEvent);
            idModelDic.TryAdd(timerModel.Id, timerModel);
        }
    }
}
