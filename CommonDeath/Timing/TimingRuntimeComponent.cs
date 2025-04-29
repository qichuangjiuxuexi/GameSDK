using UnityEngine;

namespace AppBase.CommonDeath.Timing
{
    /// <summary>
    /// 游戏时机管理器运行时组件
    /// </summary>
    public class TimingRuntimeComponent : MonoBehaviour
    {
        public TimingManager timingManager;
        
        public void Init(TimingManager timingManager)
        {
            this.timingManager = timingManager;
        }

        protected void Update()
        {
            timingManager?.Update();
        }
    }
}