using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Model
{
    public class UserModel
    {
        #region 字段
        private int id;
        private string name;
        private int money;
        private int winCount;
        private int loseCount;
        private int runCount;
        private int lv;
        private int exp;
        #endregion


        #region 属性
        /// <summary>
        /// 外键
        /// </summary>
        public int AccountId { get => id; set => id = value; }
        /// <summary>
        /// 角色id
        /// </summary>
        public int Id { get => id; set => id = value; }
        /// <summary>
        /// 角色名字
        /// </summary>
        public string Name { get => name; set => name = value; }
        /// <summary>
        /// 金钱
        /// </summary>
        public int Money { get => money; set => money = value; }
        /// <summary>
        /// 胜场
        /// </summary>
        public int WinCount { get => winCount; set => winCount = value; }
        /// <summary>
        /// 输场
        /// </summary>
        public int LoseCount { get => loseCount; set => loseCount = value; }
        /// <summary>
        /// 逃跑场
        /// </summary>
        public int RunCount { get => runCount; set => runCount = value; }
        /// <summary>
        /// 等级
        /// </summary>
        public int Lv { get => lv; set => lv = value; }
        /// <summary>
        /// 经验
        /// </summary>
        public int Exp { get => exp; set => exp = value; }

        #endregion

    }
}
