using AppBase.ArchiveDeath;

namespace AppBase.PlayerInfo
{
    public class PlayerInfoArchiveData : BaseArchiveData
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        public string PlayerId;

        /// <summary>
        /// 用户名字
        /// </summary>
        public string PlayerName;
        
        /// <summary>
        /// 设备ID
        /// </summary>
        public string deviceId;
    }
}