using System;
using System.Collections.Generic;
using AppBase;
using AppBase.Resource;
using AppBase.UI;
using UnityEngine;
using UnityEngine.Pool;


namespace AppBase.UI.Scene
{
    public class SceneBase : UIView
    {
    
        /// <summary>
        /// 场景数据
        /// </summary>
        public SceneData sceneData;
        
        
        #region 生命周期
        public virtual void OnLoad(Action callback)
        {
            callback?.Invoke();
        }
    
        public virtual void OnAwake(Action callback)
        {
            callback?.Invoke();
        }

        public virtual void OnBeforeExit(Action callback)
        {
            callback?.Invoke();
        }
    
        public virtual void OnPlayExitAnim(Action callback)
        {
            callback?.Invoke();
        }
        public virtual void OnPlayEnterAnim(Action callback)
        {
            callback?.Invoke();
        }
    
        public virtual void OnBeforeDestroy()
        {
        
        }
        #endregion
        
        
        #region 场景对象池

        private Dictionary<string, ObjectPool<GameObject>> AllPool = new();
        private Dictionary<string, List<GameObject>> AllGameObject = new();

        public GameObject GetGameObjectForPool(string address, Transform parent)
        {
            if (!AllPool.ContainsKey(address))
            {
                AllPool.Add(address, new ObjectPool<GameObject>(() =>
                {
                    var handler = GameBase.Instance.GetModule<ResourceManager>().InstantGameObject(address, parent);
                    return handler.WaitForCompletion<GameObject>();
                }, actionOnDestroy: OnDestroyGameObject));
            }
            GameObject o = AllPool[address].Get();
            // AllPool[address].Release(o);
            o.transform.SetParent(parent);
            o.transform.localPosition = Vector3.zero;
            o.transform.localScale = Vector3.one;
            o.transform.localEulerAngles = Vector3.zero;
            o.SetActive(true);
            if (!AllGameObject.ContainsKey(address))
                AllGameObject.Add(address, new List<GameObject>());
            AllGameObject[address].Add(o);
            return o;
        }

        public void ReleaseGameObject(GameObject o)
        {
            string addres = RemoveGameobj(o);
            if (!string.IsNullOrEmpty(addres))
                AllPool[addres].Release(o);
        }

        private string RemoveGameobj(GameObject o)
        {
            if (o == null)
                return "";
            foreach (var item in AllGameObject)
            {
                if (item.Value.Remove(o))
                {
                    o.SetActive(false);
                    o.transform.SetParent(transform);
                    return item.Key;
                }
            }
            return "";
        }

        private void OnDestroyGameObject(GameObject o)
        {
            RemoveGameobj(o);
            Destroy(o);
        }

        /// <summary>
        /// 释放缓存池
        /// </summary>
        public void ReleaseAllPool()
        {
            foreach (var item in AllPool)
            {
                item.Value.Dispose();
            }
            foreach (var item in AllGameObject)
            {
                for (int i = item.Value.Count - 1; i >= 0; i--)
                {
                    Destroy(item.Value[i]);
                }
            }
        }
        
        /// <summary>
        /// 内部使用
        /// </summary>
        internal void OnInternalDestroy()
        {
            ReleaseAllPool();
        }

        #endregion
    }   
}
