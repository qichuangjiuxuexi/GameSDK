using System.Collections;
using AppBase.Module;

namespace AppBase
{
    public class GameBase: ModuleBase
    {
        //单例
        public static GameBase Instance { get; protected set; }
        public GameBase()
        {
            Instance = this;
        }
        
        protected sealed override void OnAfterDestroy()
        {
            if (Instance == this) Instance = null;
        }

        /// <summary>
        /// 游戏启动后，初始化流程，时序为：InitProcesses -> OnInit -> InitAfterConfig -> InitAfterLogin
        /// </summary>
        public virtual void InitProcesses()
        {
            Init();
        }
        
        /// <summary>
        /// 这里初始化依赖配置的模块
        /// </summary>
        public virtual IEnumerator InitAfterConfig()
        {
            yield break;
        }
        
        /// <summary>
        /// 这里初始化依赖存档的模块
        /// </summary>
        public virtual IEnumerator InitAfterLogin()
        {
            yield break;
        }
    }
}