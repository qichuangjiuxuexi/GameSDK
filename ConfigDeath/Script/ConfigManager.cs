using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using AppBase.Module;
using AppBase.Resource;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace AppBase.ConfigDeath
{
    /// <summary>
    /// 配置文件获取 控制器
    /// </summary>
    public class ConfigManager : ModuleBase
    {
        
        /// <summary>
        /// 缓存数据
        /// </summary>
        private Dictionary<string, IConfigList> configAssets = new ();

        protected override void OnInit()
        {
            base.OnInit();
        }
        
        /// <summary>
        /// 获取配置文件信息（数组）
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="address">配置文件路径</param>
        /// <returns>配置数组</returns>
        public List<T> GetConfigList<T>(string address) where T : BaseConfig
            => GetConfigFormJson<BaseConfigList<T>>(address)?.values ?? new();
        
        
        public T GetConfigList<T, TJ>(string address) where T : BaseConfigList<TJ>, new() where TJ : BaseConfig => GetConfigFormJson<T>(address);
        
        /// <summary>
        /// 同步加载配置
        /// </summary>
        /// <param name="address">配置文件路径</param>
        protected T GetConfigFormJson<T>(string address) where T : class, IConfigList, new()
        {
            if (string.IsNullOrEmpty(address)) return null;
            //缓存里面拿
            if (configAssets.ContainsKey(address))
            {
                return (T)configAssets[address];
            }
            
            //获取资源
            var handler = GameBase.Instance.GetModule<ResourceManager>().LoadAsset<TextAsset>(address, this.GetResourceReference());
            string str = handler?.WaitForCompletion<TextAsset>().text;

            T obj = new T();
            JsonUtility.FromJsonOverwrite(str, obj);
            configAssets[address] = obj;
            return (T)obj;
        }

        /// <summary>
        /// 获取配置文件信息（数组）
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="address">配置文件路径</param>
        /// <returns>配置数组</returns>
        public void GetConfigListAsync<T>(string address, Action<BaseConfigList<T>> callback) where T : BaseConfig
            => GetConfigFormJsonAsync(address, callback);
        
        protected void GetConfigFormJsonAsync<T>(string address, Action<T> callback) where T : class, IConfigList, new()
        {
            if (string.IsNullOrEmpty(address)) return;
            //缓存里面拿
            if (configAssets.ContainsKey(address))
            {
                callback((T)configAssets[address]);
                return;
            }
            
             
            GameBase.Instance.GetModule<ResourceManager>().LoadAsset<TextAsset>(address, this.GetResourceReference(),
                asset =>
                {
                    T obj = new T();
                    JsonUtility.FromJsonOverwrite(asset.text, obj);
                      
                    if (obj != null)
                    {
                        configAssets[address] = obj;
                        callback((T)configAssets[address]);
                    }
                    else
                    {
                        Debug.LogError("加载失败 ： "+ address);
                    }
                });
        }

        public string GetValueWithKey<T>(string address, string key) where T : BaseConfig
        {
            Type type = typeof(T);
            string value = "";
            var kField = type.GetField("Key", BindingFlags.Public | BindingFlags.Instance);
            var vField = type.GetField("Value", BindingFlags.Public | BindingFlags.Instance);
            if (kField == null || vField == null) return value;
            
            List<T> t = GetConfigList<T>(address);
            foreach (var baseConfig in t)
            {
                string k = kField.GetValue(baseConfig).ToString();
                if (key == k)
                {
                    value = vField.GetValue(baseConfig).ToString();
                }
            }
            return value;
        } 
    }
}