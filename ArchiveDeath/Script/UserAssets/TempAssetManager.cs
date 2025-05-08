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
        
        public Dictionary<int, UserAssetItem> TempAsset => assetRecord.ArchiveData.tempAssetItems;

        /// <summary>
        /// 添加临时资产
        /// </summary>
        /// <param name="id"></param>
        /// <param name="addItem"></param>
        /// <returns></returns>
        public UserAssetItem AddTempAsset(UserAssetItem addItem)
        {
            UserAssetItem item  = GetOrCreateTempAsset(addItem.assetId);
            item.assetNum = Math.Max(0, item.assetNum+addItem.assetNum);
            Save();
            
            return item;
        }

        /// <summary>
        /// 添加临时资产数量
        /// </summary>
        /// <param name="id"></param>
        /// <param name="addItem"></param>
        /// <returns></returns>
        public UserAssetItem AddTempAssetNum(int id, long addNum)
        {
            UserAssetItem item  = GetOrCreateTempAsset(id);
            item.assetNum = Math.Max(0, item.assetNum + addNum);
            Save();
            
            return item;
        }
        
        /// <summary>
        /// 获取或者创建临时资产
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private UserAssetItem GetOrCreateTempAsset(int id)
        {
            UserAssetItem item  = GetAssetItem(id);
            if (item == null)
            {
                item = new UserAssetItem(id, 0);
                TempAsset[id] = item;
            }

            return item;
        }
        
        /// <summary>
        /// 获取临时资产
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public UserAssetItem GetTempAsset(int id)
        {
            return TempAsset.GetValueOrDefault(id);
        }

        /// <summary>
        /// 直接获取临时资产数量
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public long GetTempAssetNum(int id)
        {
            return TempAsset.TryGetValue(id, out var value) ? value.assetNum : 0;
        }
        
        /// <summary>
        /// 将某个临时资产转换为正式资产
        /// </summary>
        /// <param name="assetId">资产类型</param>
        /// <returns>转换的临时资产数量</returns>
        public long ConsumeAsset(int assetId)
        {
            var tempAsset = GetTempAsset(assetId);
            if (tempAsset == null) return 0;
            
            var assetItem = GetOrCreateAssetItem(assetId);
            long oldNum = assetItem.assetNum;
            if (tempAsset.assetNum != 0)
            {
                assetItem.assetNum += tempAsset.assetNum;
            }
            TempAsset.Remove(assetId);
            GameBase.Instance.GetModule<EventManager>().TriggerEvent(new EventUserAssetChange(assetId, oldNum, assetItem.assetNum));
            
            Save();
            
            return tempAsset.assetNum;
        }
        
        /// <summary>
        /// 将所有临时资产转换为正式资产
        /// </summary>
        /// <param name="assetId">资产类型</param>
        /// <returns>转换的临时资产数量</returns>
        public void ConsumeAllAssets()
        {
            if (TempAsset.Count == 0) return;
            foreach (var temp in TempAsset)
            {
                var tempAsset = GetTempAsset(temp.Key);
                if (tempAsset == null) return;
            
                var assetItem = GetOrCreateAssetItem(tempAsset.assetId);
                long oldNum = assetItem.assetNum;
                if (tempAsset.assetNum != 0)
                {
                    assetItem.assetNum += tempAsset.assetNum;
                }
                GameBase.Instance.GetModule<EventManager>().TriggerEvent(new EventUserAssetChange(tempAsset.assetId, oldNum, assetItem.assetNum));
            }
            TempAsset.Clear();
            Save();
        }
    }
}