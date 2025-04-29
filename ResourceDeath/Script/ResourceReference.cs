
using System;
using System.Collections.Generic;
using AppBase.Module;
using Unity.VisualScripting;
using UnityEngine;

namespace AppBase.Resource
{
    public interface IResourceReference
    {
        public List<ResourceHandler> Handlers { get; }
    }
    
    //资源引用，需要手动细狗
    public class ResourceReference: IResourceReference, IDisposable
    {
        public List<ResourceHandler> Handlers { get; } = new();
        public void Dispose()
        {
            this.ReleaseAllHandlers();
        }
    }

    public class ResourceReferenceMono : MonoBehaviour, IResourceReference
    {
        public List<ResourceHandler> Handlers { get; } = new();

        protected void OnDestroy() => this.ReleaseAllHandlers();
    }
    
    public class ResourceReferenceModule : ModuleBase, IResourceReference
    {
        public List<ResourceHandler> Handlers { get; } = new();
        protected override void OnDestroy() => this.ReleaseAllHandlers();
    }

    public static class ResourceReferenceExtra
    {
        public static void AddHandler(this IResourceReference reference, ResourceHandler h)
        {
            if (h != null && !reference.Handlers.Contains(h))
            {
                h.Retain();
                reference.Handlers.Add(h);
            }
        }
        
        public static void ReleaseHandler(this IResourceReference reference, ResourceHandler h)
        {
            if (h != null && reference.Handlers.Contains(h))
            {
                h.Release();
                reference.Handlers.Remove(h);
            }
        }
        
        public static void ReleaseAllHandlers(this IResourceReference reference)
        {
            foreach (var h in reference.Handlers)
            {
                h.Release();
            }
            reference.Handlers.Clear();
        }

        /// <summary>
        /// 获取或创建一个资源引用模块
        /// </summary>
        public static IResourceReference GetResourceReference(this GameObject gameObject)
        {
            if (gameObject == null) return null;
            var behavior = gameObject.GetComponent<ResourceReferenceMono>();
            return behavior != null ? behavior : gameObject.AddComponent<ResourceReferenceMono>();
        }

        public static ResourceReferenceMono GetResourceReference(this Component component)
        {
            if (!component) return null;
            var mono = component.gameObject.GetComponent<ResourceReferenceMono>();
            if (mono == null)
            {
                mono = component.gameObject.AddComponent<ResourceReferenceMono>();
            }
            return mono;
        }
        
        public static ResourceReferenceModule GetResourceReference(this ModuleBase module)
        {
            if (module == null) return null;
            return (ResourceReferenceModule)module.AddModule<ResourceReferenceModule>();
        }
    }
    
}