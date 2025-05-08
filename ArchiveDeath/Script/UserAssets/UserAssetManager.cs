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
    public class UserAssetManager : ModuleBase
    {
        private UserAssetRecord assetRecord;

        public Dictionary<int, UserAssetItem> AssetItems => assetRecord.ArchiveData.assetItems;

        protected override void OnInit()
        {
            base.OnInit();
            assetRecord = AddModule<UserAssetRecord>();
        }

        public UserAssetItem AddAssetItem(int id, UserAssetItem addItem)
        {
            UserAssetItem item  = GetOrCreateAssetItem(id);
            long oldNum = item.assetNum;
            item.assetNum = Math.Max(0, item.assetNum+addItem.assetNum);
            Save();

            EventUserAssetChange evt = new EventUserAssetChange(id, oldNum, item.assetNum); 
            GameBase.Instance.GetModule<EventManager>().TriggerEvent<EventUserAssetChange>(evt);
            return item;
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
            return AssetItems.TryGetValue(id, out var value) ? value.assetNum : 0;
        }

        private void Save()
        {
            assetRecord.Save();
        }
    }
}