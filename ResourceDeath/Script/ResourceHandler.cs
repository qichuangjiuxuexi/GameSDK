using System;
using AppBase.Resource;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using Object = UnityEngine.Object;

namespace AppBase.Resource
{
	public class ResourceHandler: Retainable
    {
    	//地址key
    	public string Address;
    	//加载是否成功
    	public bool IsSuccess => _handler.IsValid() && _handler.Status == AsyncOperationStatus.Succeeded;
    	//加载是否成功
    	public bool IsLoading;
    	//加载之后的回调
    	private event Action<ResourceHandler> _callback;
    	//加载之后的返回值
    	private AsyncOperationHandle _handler;
    	
    	public ResourceHandler(string address)
    	{
    		Address = address;
    	}
    	public ResourceHandler LoadAsset<T>(Action<ResourceHandler> callback) where T : Object
    	{
    		if (IsSuccess)
    		{
    			callback.Invoke(this);
    			return this;
    		}
    
    		if (callback != null)
    		{
    			_callback += callback;
    		}
    		if (!IsLoading)
    		{
    			_handler = Addressables.LoadAssetAsync<T>(Address);
    			_handler.Completed += OnLoadCompleted;
    			return this;
    		}
    		
    		return this;
    	}
    
    	private void OnLoadCompleted(AsyncOperationHandle handle)
    	{
    		IsLoading = false;
    		handle.Completed -= OnLoadCompleted;
    		if (handle.Status != AsyncOperationStatus.Succeeded)
    		{
    			Debug.LogError("资源加载失败 address: " + Address);
    		}
    		_callback?.Invoke(this);
    		_callback = null;
    	}
        
        /// <summary>
        /// 加载资源并实例化
        /// </summary>
        /// <param name="instantParams">实例化参数</param>
        /// <param name="callback">加载完成回调，无论成功失败都会回调，需要调用IsSuccess自行判定是否加载成功</param>
        public ResourceHandler LoadInstantiation(InstantiationParameters instantParams, Action<ResourceHandler> callback)
        {
	        if (IsSuccess)
	        {
		        callback?.Invoke(this);
		        return this;
	        }
	        if (callback != null) this._callback += callback;
	        if (!IsLoading)
	        {
		        IsLoading = true;
		        _handler = Addressables.InstantiateAsync(Address, instantParams);
		        _handler.Completed += OnLoadCompleted;
	        }
	        return this;
        }
    	
    	/// <summary>
    	/// 获取资源T
    	/// </summary>
    	public T GetAsset<T>() where T : Object
    	{
    		if (!IsSuccess) return null;
		    
    		return _handler.Result as T;
    	}

	    protected override void OnDestroy()
	    {
		    _callback = null;
		    if (_handler.IsValid())
		    {
			    _handler.Completed -= OnLoadCompleted;
			    Addressables.Release(_handler);
		    }
	    }
	    
	    /// <summary>
	    /// 同步等待资源加载完成
	    /// </summary>
	    /// <returns>返回资源</returns>
	    internal object WaitForCompletionInternal()
	    {
		    var obj = _handler.WaitForCompletion();
		    if (_callback != null)
		    {
			    OnLoadCompleted(_handler);
		    }
		    return obj;
	    }
    }
	
}

/// <summary>
/// 资源加载器扩展方法，防止Handler为空导致空指针异常
/// </summary>
public static class ResourceHandlerExtension
{
	/// <summary>
	/// 同步等待资源加载完成
	/// </summary>
	/// <returns>返回资源</returns>
	public static object WaitForCompletion(this ResourceHandler handler)
	{
		return handler?.WaitForCompletionInternal();
	}

	/// <summary>
	/// 同步等待资源加载完成
	/// </summary>
	/// <returns>返回资源</returns>
	public static T WaitForCompletion<T>(this ResourceHandler handler) where T : Object
	{
		return handler?.WaitForCompletionInternal() as T;
	}
}