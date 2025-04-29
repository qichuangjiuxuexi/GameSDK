using System;
using System.Collections.Generic;   

namespace AppBase.Module
{
    public class ModuleBase : IDisposable
    {
        
        protected virtual string TAG => GetType().Name;
        /// <summary>
        /// 记录子模块
        /// </summary>
        protected Dictionary<Type, ModuleBase> ModuleDic;
        protected List<ModuleBase> ModuleList;

        private byte isModuleInited;
        public bool IsModuleInited
        {
            get {
                return isModuleInited != 0;
            }
        }
        //记录父类模块
        protected ModuleBase parentModule;
        protected Object moduleData;


        protected virtual void OnBeforeInit()
        {
            
        }
        
        
        protected virtual void OnInit()
        {
            
        }
        
        protected virtual void OnAfterInit()
        {
            
        }

        protected virtual void OnDestroy(){
            
        }
        
        protected virtual void OnAfterDestroy(){
            
        }
        
        protected virtual void OnRemoveModule(){
            
        }
        

        public void Init()
        {
            if (IsModuleInited) return;
            
            OnBeforeInit();
            isModuleInited = 1;
            OnInit();
            
            //有父类模块，afterinit→父类支配
            ModuleList?.ForEach(m => m.Init());
            if (parentModule == null){
                AfterInit();
            }
        }
        
        public void AfterInit()
        {
            if (isModuleInited == 1){
                isModuleInited = 2;
                OnAfterInit();
            }
            ModuleList?.ForEach(module => module.AfterInit());
        }

        public T AddModule<T>(Object data = null) where T : ModuleBase, new()
        {
            var type = typeof(T);
            if (ModuleDic != null && ModuleDic.TryGetValue(type, out var m))
            {
                return (T)m;
            }
            return AddModule(new T(), data);
        }

        public T AddModule<T>(T t, Object data)where T : ModuleBase, new()
        {
            ModuleDic ??= new Dictionary<Type, ModuleBase>();
            ModuleList ??= new List<ModuleBase>();
            ModuleDic[t.GetType()] = t;
            ModuleList.Add(t);
            
            t.parentModule = this;
            t.moduleData = data;
            //子模块在父模块初始化后再初始化
            if (IsModuleInited)
            {
                t.Init();
            }
            return t;
        }

        public T GetModule<T>() where T : ModuleBase, new()
        {
            var t = typeof(T);
            if (ModuleDic.TryGetValue(t, out var m))
            {
                return (T)m;
            }

            foreach (var moduleBase in ModuleList)
            {
                m = moduleBase.GetModule<T>();
            }
            
            return (T)m;
        }

        /// <summary>
        /// 移除所有子模块
        /// </summary>
        public void RemoveAllModules()
        {
            foreach (var map in ModuleDic)
            {
                ModuleBase m = map.Value;
                m.Dispose();
            }
            OnRemoveModule();
            ModuleDic.Clear();
            ModuleDic = null;
            ModuleList.Clear();
            ModuleList = null;

        }
    
        /// <summary>
        /// 析构
        /// </summary>
        public void Dispose()
        {
            RemoveAllModules();
            if (IsModuleInited)
            {
                OnDestroy();
                OnAfterDestroy();   
            }
            isModuleInited = 0;
            parentModule = null;
        }
    }
   
}