using System;
using System.Collections;
using System.Collections.Generic;
using AppBase.Module;
using UnityEngine;

namespace AppBase.CommonDeath.Timing
{
    public class TimingManager : MonoModule
    {
        public TimingRuntimeComponent runtime { get; private set; }
        private List<IUpdateFrame> updateFrameList = new();
        private List<IUpdateSecond> updateSecondList = new();
        
        private float lastUpdateTime;

        
        protected override void OnInit()
        {
            base.OnInit();
            runtime = GameObject.GetComponent<TimingRuntimeComponent>();
            if (runtime == null) runtime = GameObject.AddComponent<TimingRuntimeComponent>();
            runtime.Init(this);
        }
        
        /// <summary>
        /// 注册每秒更新的管理器
        /// </summary>
        public void SubscribeSecondUpdate(IUpdateSecond updateSecond)
        {
            if (updateSecond == null) return;
            if (updateSecondList.Contains(updateSecond))
            {
                Debug.LogError("Register updateSecond already exist");
                return;
            }
            updateSecondList.Add(updateSecond);
        }
        
        /// <summary>
        /// 取消注册每帧更新的管理器
        /// </summary>
        public void UnsubscribeFrameUpdate(IUpdateFrame updateFrame)
        {
            if (updateFrame == null) return;
            if (!updateFrameList.Contains(updateFrame)) return;
            updateFrameList.Remove(updateFrame);
        }
        
        /// <summary>
        /// 取消注册每秒更新的管理器
        /// </summary>
        public void UnsubscribeSecondUpdate(IUpdateSecond updateSecond)
        {
            if (updateSecond == null) return;
            if (!updateSecondList.Contains(updateSecond)) return;
            updateSecondList.Remove(updateSecond);
        }
        
        public void Update()
        {
            for (int i = 0; i < updateFrameList.Count; i++)
            {
                updateFrameList[i].Update();
            }
            
            if (updateSecondList.Count != 0)
            {
                var time = Time.realtimeSinceStartup;
                if (time - lastUpdateTime >= 1f)
                {
                    lastUpdateTime = time;
                    for (int i = 0; i < updateSecondList.Count; i++)
                    {
                        if (updateSecondList[i] != null)
                        {
                            updateSecondList[i].OnUpdateSecond();
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// 全局的延迟回调
        /// </summary>
        public Coroutine GlobalDelayCall(float delay, Action callBack, bool isIgnoreTimeScale = true)
        {
            if (callBack == null) return null;
            var iEnumerator = _delayCallBack(delay, callBack, isIgnoreTimeScale);
            return runtime.StartCoroutine(iEnumerator);
        }
        
        private static IEnumerator _delayCallBack(float delay, Action callBack, bool isIgnoreTimeScale)
        {
            if (isIgnoreTimeScale)
            {
                yield return new WaitForSecondsRealtime(delay);
            }
            else
            {
                yield return new WaitForSeconds(delay);
            }
            callBack.Invoke();
        }
        
        /// <summary>
        /// 全局的延迟回调
        /// </summary>
        public Coroutine GlobalDelayCallFrame(float delay, Action callBack)
        {
            if (callBack == null) return null;
            var iEnumerator = _delayCallBackFrame(delay, callBack);
            return runtime.StartCoroutine(iEnumerator);
        }

        public IEnumerator _delayCallBackFrame(float delay, Action callBack)
        {
            while (delay > 0)
            {
                delay--;
                yield return null;
            }
            callBack.Invoke();
        }
        
        /// <summary>
        /// 运行协程，执行完成后回调
        /// </summary>
        /// <param name="enumerator">协程对象</param>
        /// <param name="callback">执行完成后的回调</param>
        public Coroutine StartCoroutine(IEnumerator enumerator, Action callback = null)
        {
            if (enumerator == null)
            {
                return null;
            }
            if (callback == null)
            {
                return runtime.StartCoroutine(enumerator);
            }
            else
            {
                return runtime.StartCoroutine(_InvokeCoroutine(enumerator, callback));
            }
        }
        
        private static IEnumerator _InvokeCoroutine(IEnumerator enumerator, Action callback)
        {
            yield return enumerator;
            callback?.Invoke();
        }
    }
}