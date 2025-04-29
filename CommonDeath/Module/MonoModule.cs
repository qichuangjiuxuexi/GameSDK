using AppBase.Module;
using UnityEngine;

namespace AppBase.Module
{
    /// <summary>
    /// 带有GameObject的模块
    /// </summary>
    public class MonoModule : ModuleBase
    {
        private string gameObjectPath;
        private GameObject gameObject;

        /// <summary>
        /// 卸载时是否销毁GameObject
        /// </summary>
        protected virtual bool destroyObjectOnDestroy { get; set; }

        /// <summary>
        /// 路径
        /// </summary>
        public virtual string GameObjectPath
        {
            get { return gameObjectPath ?? $"_MonoModulesRoot/{GetType().Name}"; }
            set { gameObjectPath = value; }
        }

        /// <summary>
        /// GameObject
        /// </summary>
        public GameObject GameObject
        {
            get
            {
                if (gameObject != null) return gameObject;
                gameObject = CreateGameObject(GameObjectPath);
                return gameObject;
            }
        }

        /// <summary>
        /// Transform
        /// </summary>
        public Transform Transform
        {
            get { return GameObject?.transform; }
        }

        private GameObject CreateGameObject(string path)
        {
            return CreateGameObject(null, path.Split('/'), 0);
        }

        private GameObject CreateGameObject(Transform parent, string[] path, int index)
        {
            var go = parent == null ? GameObject.Find(path[index]) : parent.Find(path[index])?.gameObject;
            if (go == null)
            {
                //如果是手动创建出来的，卸载时自动销毁
                destroyObjectOnDestroy = true;
                go = new GameObject(path[index]);
                if (parent != null)
                {
                    go.transform.SetParent(parent);
                }
                else
                {
#if UNITY_EDITOR
                    if (Application.isPlaying)
#endif
                    {
                        GameObject.DontDestroyOnLoad(go);
                    }
                }
            }

            if (index < path.Length - 1)
            {
                return CreateGameObject(go.transform, path, index + 1);
            }

            return go;
        }

        protected override void OnDestroy()
        {
            if (gameObject != null && destroyObjectOnDestroy)
            {
                GameObject.Destroy(gameObject);
                gameObject = null;
            }

            base.OnDestroy();
        }
    }
}
