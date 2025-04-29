using System;

namespace AppBase.ArchiveDeath
{
    [Serializable]
    public class BaseArchiveData
    {
        /// <summary>
        /// 当前存档版本号
        /// </summary>
        public int version = 0;
        
        /// <summary>
        /// 当前存档最后修改时间
        /// </summary>
        public long lastSaveTime = 0;
    }
}
