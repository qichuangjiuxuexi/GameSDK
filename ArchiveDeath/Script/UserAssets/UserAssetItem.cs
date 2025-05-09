using System;

namespace GameSDK.UserAssets
{
    /// <summary>
    /// 资产类
    /// </summary>
    [Serializable]
    public class UserAssetItem
    {
        /// <summary>
        /// 资产id
        /// </summary>
        public int assetId;
        
        /// <summary>
        /// 资产数量
        /// </summary>
        public long assetNum;

        public UserAssetItem()
        {
            
        }
        
        public UserAssetItem(int id, long num)
        {
            assetId = id;
            assetNum = num;
        }
    }
}