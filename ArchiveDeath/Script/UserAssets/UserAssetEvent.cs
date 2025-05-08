using AppBase.EventDeath;

namespace GameSDK.UserAssets
{
    public class EventUserAssetChange : IEventData
    {
        /// <summary>
        /// 资产类型
        /// </summary>
        public int assetId;
        
        /// <summary>
        /// 旧的资产数值
        /// </summary>
        public long oldAssetNum;
        
        /// <summary>
        /// 新的资产数值
        /// </summary>
        public long newAssetNum;
        
        public EventUserAssetChange(int assetId, long oldAssetNum, long newAssetNum)
        {
            this.assetId = assetId;
            this.oldAssetNum = oldAssetNum;
            this.newAssetNum = newAssetNum;
        }
    }
}