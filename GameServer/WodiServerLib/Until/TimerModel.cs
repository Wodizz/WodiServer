using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WodiServer.Until
{

    /// <summary>
    /// 定时器任务数据模型
    /// </summary>
    public class TimerModel
    {
        /// <summary>
        /// 任务ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 任务执行的时间
        /// </summary>
        public long Time { get; set; }

        /// <summary>
        /// 任务触发事件
        /// </summary>
        public Action TimeTriggerEvent { get; set; }

        public TimerModel(int id, long time, Action timeTriggerEvent)
        {
            this.Id = id;
            this.Time = time;
            this.TimeTriggerEvent = timeTriggerEvent;
        }

        /// <summary>
        /// 触发任务
        /// </summary>
        public void Trigger()
        {
            TimeTriggerEvent?.Invoke();
        }
    }
}
