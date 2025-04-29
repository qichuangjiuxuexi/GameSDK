using System;
using System.Collections.Generic;
using System.Reflection;

namespace AppBase.UI
{
    public class UIView : UIBinding
    {
        public Dictionary<string, UIBinding> binds;
        Type type;
        PropertyInfo propertyInfo;

        public T FindUIBind<T>(string path) where T : UIBinding
        {
            if (binds != null && binds.TryGetValue(path, out var bind))
            {
                return (T)bind;
            }

            bind = transform.Find(path)?.GetComponent<T>();
            if (bind == null) return null;

            binds ??= new Dictionary<string, UIBinding>();
            binds[path] = bind;
            return (T)bind;
        }

        public T GetNode<T>(string nodeName)where T:UIBinding
        {
            type??= GetType();
            propertyInfo??= type.GetProperty(nodeName);
            if (propertyInfo != null && propertyInfo.CanRead)
            {
                return (T)propertyInfo.GetValue(this);
            }
            return null;
        }
        
        public UIBinding GetNode(string nodeName)
        {
            type??= GetType();
            propertyInfo??= type.GetProperty(nodeName);
            if (propertyInfo != null && propertyInfo.CanRead)
            {
                return (UIBinding)propertyInfo.GetValue(this);
            }
            return null;
        }
        
        
    }
}