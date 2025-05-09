using System;
using System.Collections.Generic;
using AppBase;
using AppBase.EventDeath;
using AppBase.Module;

namespace GameSDK.UserAssets
{
    /// <summary>
    /// 资产控制类
    /// </summary>
    public partial class UserAssetManager : ModuleBase
    {
        private UserAssetRecord assetRecord;

        public Dictionary<int, UserAssetItem> AssetItems => assetRecord.ArchiveData.assetItems;

        protected override void OnInit()
        {
            base.OnInit();
            assetRecord = AddModule<UserAssetRecord>();
        }

        protected override void OnAfterInit()
        {
            base.OnAfterInit();
            ConsumeAllAssets(); //初始化结算所有临时资产
        }

        /// <summary>
        /// 通过资产添加资产数量
        /// </summary>
        /// <param name="id"></param>
        /// <param name="addItem"></param>
        /// <returns></returns>
        public UserAssetItem AddAssetItem(UserAssetItem addItem)
        {
            UserAssetItem item  = GetOrCreateAssetItem(addItem.assetId);
            long oldNum = item.assetNum;
            item.assetNum = Math.Max(0, item.assetNum+addItem.assetNum);
            Save();

            EventUserAssetChange evt = new EventUserAssetChange(addItem.assetId, oldNum, item.assetNum); 
            GameBase.Instance.GetModule<EventManager>().TriggerEvent<EventUserAssetChange>(evt);
            return item;
        }
        
        /// <summary>
        /// 直接增加资产数量
        /// </summary>
        /// <param name="id"></param>
        /// <param name="addNum"></param>
        /// <returns></returns>
        public long AddAssetNum(int id, long addNum)
        {
            UserAssetItem item  = GetOrCreateAssetItem(id);
            long oldNum = item.assetNum;
            item.assetNum = Math.Max(0, oldNum + addNum);
            Save();

            EventUserAssetChange evt = new EventUserAssetChange(id, oldNum, item.assetNum); 
            GameBase.Instance.GetModule<EventManager>().TriggerEvent<EventUserAssetChange>(evt);
            return oldNum + addNum;
        }
        
        /// <summary>
        /// 直接减少资产数量
        /// </summary>
        /// <param name="assetId">资产类型</param>
        /// <param name="assetNum">资产数量</param>
        /// <param name="tag">来源标签，打点使用</param>
        /// <returns>新的资产数量</returns>
        public long SubAssetNum(int assetId, long assetNum)
        {
            return AddAssetNum(assetId, -assetNum);
        }
        
        /// <summary>
        /// 直接设置资产数量
        /// </summary>
        /// <param name="id"></param>
        /// <param name="num"></param>
        /// <returns></returns>
        public long SetAssetNum(int id, long num)
        {
            UserAssetItem item  = GetOrCreateAssetItem(id);
            long oldNum = item.assetNum; 
            item.assetNum = Math.Max(0, num);
            Save();
            
            EventUserAssetChange evt = new EventUserAssetChange(id, oldNum, item.assetNum); 
            GameBase.Instance.GetModule<EventManager>().TriggerEvent<EventUserAssetChange>(evt);
            return item.assetNum;
        }

        private UserAssetItem GetOrCreateAssetItem(int id)
        {
            UserAssetItem item  = GetAssetItem(id);
            if (item == null)
            {
                item = new UserAssetItem(id, 0);
                AssetItems[id] = item;
            }

            return item;
        }
        
        /// <summary>
        /// 获取资产
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public UserAssetItem GetAssetItem(int id)
        {
            return AssetItems.GetValueOrDefault(id);
        }

        /// <summary>
        /// 直接获取数量
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public long GetAssetNum(int id)
        {
            return GetAssetItem(id).assetNum;
        }

        private void Save()
        {
            assetRecord.Save();
        }
    }
}