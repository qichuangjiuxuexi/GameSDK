using System;
using System.Collections.Generic;
using AppBase.Module;
using UnityEngine;
using UnityEngine.ResourceManagement.ResourceProviders;
using Object = UnityEngine.Object;

namespace AppBase.Resource
{
	public class ResourceManager : ModuleBase
	{
		/// <summary>
		/// 资源缓存池
		/// </summary>
		protected Dictionary<string, ResourceHandler> assetsPool = new();
    
		public ResourceHandler LoadAsset<T>(string address, IResourceReference reference, Action<T> successCallback = null, Action failureCallback = null) where T: Object
		{
			if (!assetsPool.TryGetValue(address, out var handler)){
				handler = new ResourceHandler(address);
				assetsPool[address] = handler;
			}
			
			reference.AddHandler(handler);
			handler.LoadAsset<T>(h =>
			{
				if (handler.IsSuccess)
				{
					var asset = h.GetAsset<T>();
					successCallback?.Invoke(asset);
					handler.Retain();
					return;
				}
				
				h.CheckRetainCount();
				failureCallback?.Invoke();
			});
		
			return handler;
		}
		
		/// <summary>
		/// 实例化游戏对象，资源生命周期跟随实例化的游戏对象
		/// </summary>
		/// <param name="address">地址</param>
		/// <param name="parent">实例化父节点</param>
		/// <param name="successCallback">加载成功时的回调</param>
		/// <param name="failureCallback">加载失败时的回调</param>
		/// <typeparam name="T">资源类型</typeparam>
		/// <returns>加载器</returns>
		public ResourceHandler InstantGameObject(string address, Transform parent, Action<GameObject> successCallback = null, Action failureCallback = null)
		{
			return InstantGameObject(address, new InstantiationParameters(parent , false), successCallback, failureCallback);
		}
		
		
		/// <summary>
		/// 实例化游戏对象，资源生命周期跟随实例化的游戏对象
		/// </summary>
		/// <param name="address">地址</param>
		/// <param name="instantParams">实例化参数</param>
		/// <param name="successCallback">加载成功时的回调</param>
		/// <param name="failureCallback">加载失败时的回调</param>
		/// <typeparam name="T">资源类型</typeparam>
		/// <returns>加载器</returns>
		public ResourceHandler InstantGameObject(string address, InstantiationParameters instantParams, Action<GameObject> successCallback = null, Action failureCallback = null)
		{
			var handler = new ResourceHandler(address);
			handler.LoadInstantiation(instantParams, h =>
			{
				if (h.IsSuccess)
				{
					var obj = h.GetAsset<GameObject>();
					if (obj != null)
					{
						obj.name = obj.name.Replace("(Clone)", "");
						obj.GetResourceReference().AddHandler(h);
						successCallback?.Invoke(obj);
						return;
					}
				}
				h.SafeRetainCount();
				failureCallback?.Invoke();
			});
			return handler;
		}
		
		public ResourceHandler LoadAssetHandler<T>(string address, Action<ResourceHandler> successCallback = null, Action failureCallback = null) where T : Object
		{
			if (!assetsPool.TryGetValue(address, out var handler))
			{
				handler = new ResourceHandler(address);
				assetsPool[address] = handler;
			}
			
			handler.LoadAsset<T>(h =>
			{
				if (h.IsSuccess)
				{
					h.Retain();
					successCallback?.Invoke(h);
					return;
				}
				
				h.CheckRetainCount();
				failureCallback?.Invoke();
			});
			return handler;
		}
		
		
	}
	
}