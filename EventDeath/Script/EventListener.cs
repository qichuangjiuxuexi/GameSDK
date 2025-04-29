using System;

namespace AppBase.EventDeath
{
    // 用于无数据事件的监听器
    public class EventListener
    {
        public string eventName;
        public Delegate eventCallback;
        public int priority;

        public EventListener(string evtName, Delegate callback, int prio = 0)
        {
            eventName = evtName;
            eventCallback = callback;
            priority = prio;
        }
    }
}