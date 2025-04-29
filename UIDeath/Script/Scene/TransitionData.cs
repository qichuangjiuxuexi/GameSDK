namespace AppBase.UI.Scene
{
    public class TransitionData : SceneData
    {
        /// <summary>
        /// 上一个场景数据，自动获得
        /// </summary>
        public SceneData PreSceneData { get; internal set; }

        /// <summary>
        /// 下一个场景数据
        /// </summary>
        public SceneData NextSceneData { get; set; }
        
        /// <summary>
        /// 打开动画名称
        /// </summary>
        public string openAnimName = "Open";

        /// <summary>
        /// 关闭动画名称
        /// </summary>
        public string closeAnimName = "Close";

        public TransitionData()
        {
        }

        /// <summary>
        /// 转场场景数据
        /// </summary>
        /// <param name="address">转场场景地址</param>
        /// <param name="nextSceneData">下一个场景数据</param>
        /// <param name="sceneType">转场场景是否是UI场景</param>
        public TransitionData(string address, SceneData nextSceneData = null) : base(address, nextSceneData)
        {
            NextSceneData = nextSceneData;
        }
    }
}