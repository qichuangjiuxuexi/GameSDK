using System;
using System.Collections.Generic;
using System.Linq;
using AppBase.Module;


namespace AppBase.EventDeath
{
    // 事件管理器类
    public class EventManager: ModuleBase
    {
        private Dictionary<string, List<EventListener>> eventListenersDictionary;

        protected override void OnInit()
        {
            base.OnInit();
            eventListenersDictionary = new Dictionary<string, List<EventListener>>();
        }
        // 注册带数据的事件
        public void AddEventListener<T>(Action<T> eventCallback, int priority = 0) where T : IEventData
        {
            string eventName = typeof(T).Name;
            RegisterEvent(eventName, eventCallback, priority);
        }
        
        // 注册带数据的事件
        public void AddEventListener<T>(string eventName, Action<T> eventCallback, int priority = 0) where T : IEventData
        {
            RegisterEvent(eventName, eventCallback, priority);
        }

        // 注册不带数据的事件
        public void AddEventListener(string eventName, Action eventCallback, int priority = 0)
        {
            RegisterEvent(eventName, eventCallback, priority);
        }

        /// <summary>
        /// 真正注册事件
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="eventCallback"></param>
        /// <param name="priority"></param>
        private void RegisterEvent(string eventName, Delegate eventCallback, int priority)
        {
            if (!eventListenersDictionary.ContainsKey(eventName))
            {
                eventListenersDictionary[eventName] = new List<EventListener>();
            }
            //已经绑定过的不能再次绑定
            var sameListener = eventListenersDictionary[eventName]
                .FirstOrDefault(l => l.eventCallback == eventCallback);
            if (sameListener != null)
            {
                return;
            }

            var eventListener = new EventListener(eventName, eventCallback, priority);
            eventListenersDictionary[eventName].Insert(0, eventListener);

            eventListenersDictionary[eventName] = eventListenersDictionary[eventName]
                .OrderBy(listener => listener.priority).ToList();
        }

        // 注销事件监听器
        public void RemoveEventListener(string eventName, Action eventCallback)
        {
            if (eventListenersDictionary.ContainsKey(eventName))
            {
                var listenerToRemove = eventListenersDictionary[eventName]
                    .FirstOrDefault(l => l.eventCallback == (Delegate)eventCallback);

                if (listenerToRemove != null)
                {
                    eventListenersDictionary[eventName].Remove(listenerToRemove);
                }

                if (eventListenersDictionary[eventName].Count == 0)
                {
                    eventListenersDictionary.Remove(eventName);
                }
            }
        }
        
        // 注销事件监听器
        public void RemoveEventListener<T>(string eventName, Action<T> eventCallback) where T : IEventData
        {
            if (eventListenersDictionary.ContainsKey(eventName))
            {
                var listenerToRemove = eventListenersDictionary[eventName]
                    .FirstOrDefault(l => l.eventCallback == (Delegate)eventCallback);

                if (listenerToRemove != null)
                {
                    eventListenersDictionary[eventName].Remove(listenerToRemove);
                }

                if (eventListenersDictionary[eventName].Count == 0)
                {
                    eventListenersDictionary.Remove(eventName);
                }
            }
        }
        
        // 注销事件监听器
        public void RemoveEventListener<T>( Action<T> eventCallback) where T : IEventData
        {
            string eventName = typeof(T).Name;
            if (eventListenersDictionary.ContainsKey(eventName))
            {
                var listenerToRemove = eventListenersDictionary[eventName]
                    .FirstOrDefault(l => l.eventCallback == (Delegate)eventCallback);

                if (listenerToRemove != null)
                {
                    eventListenersDictionary[eventName].Remove(listenerToRemove);
                }

                if (eventListenersDictionary[eventName].Count == 0)
                {
                    eventListenersDictionary.Remove(eventName);
                }
            }
        }

        // 触发带数据的事件
        public void TriggerEvent<T>(T eventData = default) where T: IEventData
        {
            string eventName = typeof(T).Name;
            TriggerEventInternal<T>(eventName, eventData);
        }

        // 触发不带数据的事件
        public void TriggerEvent(string eventName)
        {
            TriggerEventInternal(eventName);
        }
        
        private void TriggerEventInternal(string eventName)
        {
            if (eventListenersDictionary.TryGetValue(eventName, out var listeners))
            {
                // 创建一个只读的列表副本
                var listenersSnapshot = listeners.ToList();

                foreach (var listener in listenersSnapshot)
                {
                    if (listener == null) continue;
                    if (listener.eventCallback is Action actionWithoutArgs)
                    {
                        actionWithoutArgs.Invoke();
                    }
                    else
                    {
                        var action = listener.eventCallback;
                        var arg = new object[1];
                        action.DynamicInvoke(arg);
                    }
                }
            }
        }

        /// <summary>
        /// 带数据的触发事件
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="eventArgs"></param>
        /// <typeparam name="T"></typeparam>
        private void TriggerEventInternal<T>(string eventName, T eventArgs) where T : IEventData
        {
            if (eventListenersDictionary.TryGetValue(eventName, out var listeners))
            {
                var listenersSnapshot = listeners.ToList();

                foreach (var listener in listenersSnapshot)
                {
                    if (listener == null) continue;
                    if (listener.eventCallback is Action<T> actionWithArgs)
                    {
                        actionWithArgs.Invoke(eventArgs);
                    }
                    else if (listener.eventCallback is Action actionWithoutArgs)
                    {
                        actionWithoutArgs.Invoke();
                    }
                }
            }
        }

        
        /// <summary>
        /// 注销某一个事件
        /// </summary>
        public void RemoveAllListeners(string eventName)
        {
            eventListenersDictionary.Remove(eventName);
        }
        
        /// <summary>
        /// 注销所有事件
        /// </summary>
        public void UnregisterAllEvents()
        {
            eventListenersDictionary.Clear();
        }
    }
}