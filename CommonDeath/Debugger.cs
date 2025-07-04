using UnityEngine;

public static class Debuggers
{
    
    public static void Error(string logTag, object message)
    {
        Error(logTag, message, (Object)null);
    }

    public static void Error(string logTag, object message, Object context)
    {
        Debug.LogError((object)string.Format("[{0}]: {1}", (object)logTag, message), context);
    }
    public static void Log(string logTag, object message)
    {
        Log(logTag, message, (Object)null);
    }

    public static void Log(string logTag, object message, Object context)
    {
        Debug.Log((object)string.Format("[{0}]: {1}", (object)logTag, message), context);
    }
}