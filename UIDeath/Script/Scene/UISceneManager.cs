using AppBase;
using AppBase.Module;
using AppBase.Resource;
using AppBase.CommonDeath;
using UnityEngine;

namespace AppBase.UI.Scene
{
    public class UISceneManager : ModuleBase
    {
        
        /// <summary>
        /// Normal场景挂点
        /// </summary>
        public GameObject NormalSceneRoot
        {
            get
            {
                if (_normalSceneRoot != null) return _normalSceneRoot;
                _normalSceneRoot = GameObject.Find("Scene");
                if (_normalSceneRoot == null)
                {
                    _normalSceneRoot = new GameObject("Scene");
                    _normalSceneRoot.transform.SetAsFirstSibling();
                }
                GameObject.DontDestroyOnLoad(_normalSceneRoot);
                return _normalSceneRoot;
            }
        }
        private GameObject _normalSceneRoot;
        /// <summary>
        /// UI场景挂点
        /// </summary>
        private GameObject UISceneRoot
        {
            get
            {
                if (_uiSceneRoot != null) return _uiSceneRoot;
                var canvas = GameObject.Find("UICanvas");
                if (canvas == null)
                {
                    return null;
                }
                GameObject.DontDestroyOnLoad(canvas);
                _uiSceneRoot = canvas.transform.Find("Scenes")?.gameObject;
                if (_uiSceneRoot == null)
                {
                    _uiSceneRoot = canvas.AddFullScreenRectTransform().gameObject;
                    _uiSceneRoot.name = "Scenes";
                    _uiSceneRoot.transform.SetAsFirstSibling();
                }
                return _uiSceneRoot;
            }
        }
        private GameObject _uiSceneRoot;
        
        /// <summary>
        /// 当前场景的GameObject
        /// </summary>
        private GameObject CurrentSceneObj;

        /// <summary>
        /// 当前的场景的SceneBase
        /// </summary>
        public SceneBase CurrentScene => CurrentSceneObj != null && CurrentSceneObj.TryGetComponent(out SceneBase scene) ? scene : null;

        /// <summary>
        /// 当前场景的数据
        /// </summary>
        public SceneData CurrentSceneData => CurrentScene != null ? CurrentScene.sceneData : null;
        
        /// <summary>
        /// 切换场景前的场景数据
        /// </summary>
        public SceneData LastSceneData { get; private set; }

        
        protected override void OnInit()
        {
            base.OnInit();
            if (UISceneRoot) UISceneRoot.SetActive(true);
            if (UISceneRoot) CurrentSceneObj = UISceneRoot.transform.Find("LaunchScene")?.gameObject ?? UISceneRoot.transform.Find("SplashScene")?.gameObject;
            // if (CurrentSceneObj && !CurrentSceneObj.GetComponent<UIScene>())
            // {
            //     var uiScene = CurrentSceneObj.AddComponent<UIScene>();
            //     uiScene.sceneData = new UISceneData(AAConst.LaunchScene);
            // }
        }
        
        /// <summary>
        /// 切换场景
        /// </summary>
        /// <param name="sceneData">场景数据</param>
        public SceneData SwitchScene(SceneData sceneData)
        {
            if (sceneData == null || string.IsNullOrEmpty(sceneData.address)) return sceneData;
            if (sceneData is TransitionData transData) transData.PreSceneData = CurrentSceneData;
            var resource = GameBase.Instance.GetModule<ResourceManager>();
            sceneData.handler = resource.LoadAssetHandler<GameObject>(sceneData.address, h => OnSceneLoaded(h, sceneData), () => sceneData.handler = null);
            
            return sceneData;
        }
        
        
        
         /// <summary>
        /// 场景加载完毕，播放入场动画
        /// </summary>
        private void OnSceneLoaded(ResourceHandler handler, SceneData newSceneData)
        {
            var oldSceneObj = CurrentSceneObj;
            var oldScene = CurrentScene;
            var oldSceneData = CurrentSceneData;
            LastSceneData = oldSceneData;
            SceneBase newScene = handler.GetAsset<GameObject>().GetComponent<SceneBase>();
            var flow = FlowUtil.Create();
            if (oldScene != null) flow.Add(oldScene.OnBeforeExit);
            
            
            //时序：oldScene.OnBeforeExit -> OnLoad -> Awake -> OnAwake -> oldScene.OnPlayExitAnim -> OnPlayEnterAnim -> oldScene.OnBeforeDestroy -> oldScene.OnDestroy
            flow.Add(InitGameObject);
            flow.Add(next =>
            {
                newSceneData.OnLoadedCallback(newScene);
                newScene.OnLoad(next);
            });
            
            flow.Add(() => CurrentSceneObj.SetActive(true));
            flow.Add(next => newScene.OnAwake(next));
            if (oldScene != null) flow.Add(oldScene.OnPlayExitAnim);
            flow.Add(next => newScene.OnPlayEnterAnim(next));
            if (oldScene != null) flow.Add(oldScene.OnBeforeDestroy);
            if (oldScene != null) flow.Add(oldScene.OnInternalDestroy);
            if (oldSceneObj != null) flow.Add(() =>
            {
                GameObject.Destroy(oldSceneObj);
                OnDestroyScene(oldSceneData);
            });
            flow.Invoke(() =>
            {
                newSceneData.OnSwitchCallback(newScene);
            });

            //初始化普通场景
            void InitGameObject()
            {
                var prefab = handler.GetAsset<GameObject>();
                prefab.SetActive(false);
                var prefabScene = prefab.GetComponent<SceneBase>();
                var isUIScene = prefabScene is UIScene;
                var parentRoot = isUIScene ? UISceneRoot : NormalSceneRoot;
                CurrentSceneObj = parentRoot.AddInstantiate(prefab);
                newScene = isUIScene ? CurrentSceneObj.GetComponent<UIScene>() : CurrentSceneObj.GetComponent<SceneBase>();
                CurrentSceneObj.GetResourceReference().AddHandler(handler);
                handler.Release();
                newScene.sceneData = newSceneData;
            }
        }

        private void OnDestroyScene(SceneData sceneData)
        {
            if (sceneData != null && !string.IsNullOrEmpty(sceneData.address))
            {
                
            }
        }
    }
}