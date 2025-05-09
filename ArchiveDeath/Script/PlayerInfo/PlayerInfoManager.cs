using AppBase.Module;

namespace AppBase.PlayerInfo
{
    public class PlayerInfoManager: ModuleBase
    {
        /// <summary>
        /// 用户信息
        /// </summary>
        public PlayerInfoRecord PlayerRecord { get; protected set; }
        
        protected override void OnInit()
        {
            base.OnInit();
            
            PlayerRecord = AddModule<PlayerInfoRecord>();
            if (PlayerRecord.IsNewRecord)
            {
                OnNewRecord();
            }
        }

        /// <summary>
        /// 创建新存档
        /// </summary>
        protected virtual void OnNewRecord()
        {
        }
    }
}