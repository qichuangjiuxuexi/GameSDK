using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AppBase.CommonDeath
{
    /// <summary>
    /// 回调链工具
    /// </summary>

    public class FlowUtil : MonoBehaviour
    {
        private static Queue<GameObject> pool = new Queue<GameObject>();
        private static GameObject parent;
        private Queue<Func<IEnumerator>> actions = new Queue<Func<IEnumerator>>();

        // 创建方法用于创建GameObject并添加ActionFlow组件
        public static FlowUtil Create(GameObject targetParent = null)
        {
            if (parent == null) parent = new GameObject("_FlowNodePool");
            if (targetParent == null) targetParent = parent;
            if (targetParent == null)
            {
                targetParent = parent;
            }
            
            GameObject flowObject;
            if (pool.Count > 0)
            {
                flowObject = pool.Dequeue();
                flowObject.SetActive(true);
                return flowObject.GetComponent<FlowUtil>();
            }
            flowObject = Instantiate(parent, targetParent.transform);
            flowObject.name = "ActionFlowNode";
            return flowObject.AddComponent<FlowUtil>();
        }

        // 添加异步动作（协程）
        public void Add(IEnumerator action)
        {
            actions.Enqueue(() => action);
        }

        // 添加带显式回调的同步动作
        public void Add(Action<Action> action)
        {
            IEnumerator WrappedCallback()
            {
                var isOver = false;
                action(()=>
                {
                    isOver = true;
                });
                yield return new WaitUntil(() => isOver); // 确保是一个有效的协程
            }
        
            actions.Enqueue(WrappedCallback);
        }

        // 添加自动回调的同步动作
        public void Add(Action action)
        {
            IEnumerator WrappedSync()
            {
                action();
                yield return null;  // 确保返回一个有效的协程
            }

            actions.Enqueue(() => WrappedSync());
        }

        // 启动动作序列
        public void Invoke(Action callBack = null)
        {
            StartCoroutine(ExecuteActions(callBack));
        }

        // 执行队列中的所有动作
        private IEnumerator ExecuteActions(Action callBack = null)
        {
            while (actions.Count > 0)
            {
                yield return StartCoroutine(actions.Dequeue().Invoke());
            }

            if (transform.parent.gameObject == parent)
            {
                actions = new Queue<Func<IEnumerator>>();
                pool.Enqueue(gameObject);
                gameObject.SetActive(false);   
            }
            else
            {
                Destroy(gameObject);
            }
            callBack?.Invoke();
        }

        public void Add(Func<IEnumerator> testFlow)
        {
            actions.Enqueue(testFlow);
        }
    }

}