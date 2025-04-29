
using System;
using UnityEngine.AddressableAssets;

namespace AppBase.Resource
{
	//引用计数器
	public class Retainable : IDisposable
	{
		
		/// <summary>
		/// 引用计数
		/// </summary>
		public int RetainCount { get; protected set; }

		public int Retain()
		{
			RetainCount++;
			return RetainCount;
		}

		public int Release()
		{
			RetainCount--;
			return CheckRetainCount();
		}

		public int SafeRetainCount()
		{
			if (RetainCount <= 0)
			{
				Dispose();
				return 0;
			}

			return RetainCount;
		}

		//手动销毁
		public void Dispose()
		{
			RetainCount = 0;
			OnDestroy();
		}

		/// <summary>
		/// 销毁
		/// </summary>
		protected virtual void OnDestroy()
		{
			
		}
		
		/// <summary>
		/// 检查引用计数，如果为0会调用析构
		/// </summary>
		/// <returns>剩余的引用计数</returns>
		public int CheckRetainCount()
		{
			if (RetainCount <= 0)
			{
				Dispose();
			}
			return RetainCount;
		}
	}	
}
