using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// UI组件内部工具类
/// </summary>

namespace AppBase.UI
{
    internal static class UIUtil
    {
        internal static RectTransform AddFullScreenRectTransform(this GameObject obj)
        {
            if (obj == null) return null;
            var rect = obj.GetOrAddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            return rect;
        }

        internal static T GetOrAddComponent<T>(this Component transform) where T : Component
        {
            if (transform == null)
            {
                return null;
            }

            T behaviour = transform.GetComponent<T>();
            if (behaviour == null)
            {
                behaviour = transform.gameObject.AddComponent<T>();
            }

            return behaviour;
        }

        internal static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component
        {
            if (gameObject == null)
            {
                return null;
            }

            if (!gameObject.TryGetComponent(out T behaviour))
            {
                behaviour = gameObject.AddComponent<T>();
            }

            return behaviour;
        }

        internal static void DestroyChildren(this Transform @this)
        {
            var childCount = @this.childCount;
            for (int i = childCount - 1; i >= 0; i--)
            {
                GameObject.Destroy(@this.GetChild(i).gameObject);
            }
        }

        internal static GameObject AddGameObject(this GameObject gameObject, string subObjName)
        {
            if (gameObject == null) return null;
            var subObject = new GameObject(subObjName);
            subObject.transform.SetParent(gameObject.transform);
            subObject.transform.localPosition = Vector3.zero;
            subObject.transform.localScale = Vector3.one;
            subObject.layer = gameObject.layer;
            return subObject;
        }

        internal static GameObject AddInstantiate(this GameObject gameObject, GameObject template)
        {
            if (gameObject == null || template == null) return null;
            var subObject = GameObject.Instantiate(template, gameObject.transform, false);
            subObject.name = subObject.name.Replace("(Clone)", "");
            subObject.transform.localPosition = Vector3.zero;
            subObject.transform.localScale = Vector3.one;
            return subObject;
        }

        internal static void SetAlpha(this Graphic @this, float value)
        {
            if (@this != null)
            {
                var c = @this.color;
                c.a = value;
                @this.color = c;
            }
        }

        internal static Coroutine DelayCallFrame(this MonoBehaviour obj, int delay, Action callBack)
        {
            if (obj == null || callBack == null) return null;
            IEnumerator iEnumerator = _delayCallBackFrame(delay, callBack);
            return obj.StartCoroutine(iEnumerator);
        }

        private static IEnumerator _delayCallBackFrame(int delay, Action callBack)
        {
            while (delay > 0)
            {
                delay--;
                yield return null;
            }

            callBack?.Invoke();
        }

        internal static Coroutine DelayCall(this MonoBehaviour obj, float delay, Action callBack,
            bool isIgnoreTimeScale = true)
        {
            if (obj == null || callBack == null) return null;
            IEnumerator iEnumerator = _delayCallBack(delay, callBack, isIgnoreTimeScale);
            return obj.StartCoroutine(iEnumerator);
        }

        private static IEnumerator _delayCallBack(float delay, Action callBack, bool isIgnoreTimeScale = true)
        {
            if (isIgnoreTimeScale)
            {
                yield return new WaitForSecondsRealtime(delay);
            }
            else
            {
                yield return new WaitForSeconds(delay);
            }

            callBack?.Invoke();
        }

        internal static void SetActive(this Component component, bool isActive)
        {
            if (component != null && component.gameObject.activeSelf != isActive)
            {
                component.gameObject.SetActive(isActive);
            }
        }

        internal static float PlayAnimatorUpdate(this Component component, string animName, float offset = 0)
        {
            if (component == null) return 0;
            var animator = component.GetComponent<Animator>();
            if (animator == null || string.IsNullOrEmpty(animName)) return 0;
            animator.keepAnimatorStateOnDisable = true;
            animator.PlayInFixedTime(animName, 0, offset);
            animator.Update(0);
            var duration = animator.GetCurrentAnimatorStateInfo(0).length - offset;
            return duration;
        }

        internal static Transform Finds(this Transform transform, params string[] paths)
        {
            if (transform == null || paths.Length == 0) return null;
            foreach (var name in paths)
            {
                var t = transform.Find(name);
                if (t != null) return t;
            }

            return null;
        }

        internal static void AddListener(this Button button, UnityAction action)
        {
            if (button == null || action == null) return;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(action);
        }
    }
}