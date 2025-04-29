using AppBase.UI.Scene;

namespace Project.Level
{
    public enum ESceneSwitchType
    {
        None = 0,

        /// <summary>
        /// 关卡场景
        /// </summary>
        Level = 2
    }
    
    public interface ISwitchScene
    {
        /// <summary>
        /// 切换目标类型
        /// </summary>
        ESceneSwitchType TargetScene { get; }
        
        /// <summary>
        /// 目标数据
        /// </summary>
        SceneData SceneData { get; }
        
        /// <summary>
        /// 转场Loading
        /// </summary>
        string TransitionAddress { get; }
    }
}