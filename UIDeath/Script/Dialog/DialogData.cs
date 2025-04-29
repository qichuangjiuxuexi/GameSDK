using System;
using AppBase.Resource;
using UnityEngine;

namespace AppBase.UI.Dialog
{
    public class DialogData
    {
        /// <summary>
        /// addressable 地址
        /// </summary>
        public string address;

        public ResourceHandler handler;

        /// <summary>
        /// 传输的弹板数据
        /// </summary>
        public object data;

        /// <summary>
        /// 是否有黑色遮罩
        /// </summary>
        public bool hasBgMask = true;

        /// <summary>
        /// 开启动画
        /// </summary>
        public string openAnimName = "Open";

        /// <summary>
        /// 开启动画
        /// </summary>
        public string closeAnimName = "";

        /// <summary>
        /// 加在完成之后回调
        /// </summary>
        public Action<UIDialog> loadedCallback;
        public void OnLoadedCallback(UIDialog dialog)
        {
            loadedCallback?.Invoke(dialog);
            loadedCallback = null;
        }
        /// <summary>
        /// 开启动画完成之后回调
        /// </summary>
        public Action<UIDialog> openCallback;

        public void OnOpenCallback(UIDialog dialog)
        {
            openCallback?.Invoke(dialog);
            openCallback = null;
        }
        
        /// <summary>
        /// 关闭完成之后回调
        /// </summary>
        public Action<UIDialog> closeCallback;

        public void OnCloseCallback(UIDialog dialog)
        {
            closeCallback?.Invoke(dialog);
            closeCallback = null;
        }
        
        /// <summary>
        /// 背景遮罩颜色
        /// </summary>
        public Color bgMaskColor = new Color(0, 0, 0, 0.7f);

        public DialogData(){}

        public DialogData(string address, object data = null, Action<UIDialog> loadedCallback = null, Action<UIDialog> openCallback = null, Action<UIDialog> closeCallback = null)
        {
            this.address = address;
            this.data = data;
            this.loadedCallback = loadedCallback;
            this.openCallback = openCallback;
            this.closeCallback = closeCallback;
        }

    }
}