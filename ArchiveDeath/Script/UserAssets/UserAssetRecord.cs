using System;
using System.Collections.Generic;
using AppBase.ArchiveDeath;

namespace GameSDK.UserAssets
{
    public class UserAssetRecord : BaseRecord<UserAssetArchive>
    {
        
    }

    [Serializable]
    public class UserAssetArchive : BaseArchiveData
    {
        /// <summary>
        /// 用户全部资产
        /// </summary>
        public Dictionary<int, UserAssetItem> assetItems = new();
        
        /// <summary>
        /// 用户全部临时资产
        /// </summary>
        public Dictionary<int, UserAssetItem> tempAssetItems = new();
    }
}