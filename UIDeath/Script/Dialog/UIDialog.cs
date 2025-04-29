using AppBase;
using System;
using AppBase.UI;
using UnityEngine.UI;

namespace AppBase.UI.Dialog
{
    public class UIDialog : UIView
    {
        public DialogData dialogData;
        public DialogContainer dialogContainer;
        private bool _isClosed = false;
        public Button closeBtn;

        public void Init(DialogData data)
        {
            dialogData = data;
            data?.OnLoadedCallback(this);
            OnInit();
        }

        #region 生命周期
        /// <summary>
        /// OnAwake > OnStart > BeforeOpenAnim > AfterOpenAnim > BeforeCloseAnim > AfterCloseAnim
        /// </summary>
        public virtual void OnInit()
        {
            
        }
        public virtual void OnLoaded(Action action)
        {
            action?.Invoke();
        }
        
        public virtual void OnAwake()
        {
            
        }

        public virtual void OnStart()
        {
            
        }
        
        public virtual void OnBeforeOpenAnim()
        {
            
        }
        public virtual void OnAfterOpenAnim()
        {
            
        }

        public virtual void OnBeforeCloseAnim()
        {
            
        }
        public virtual void OnAfterCloseAnim()
        {
            
        }

        public virtual void OnBeforeDestroy(){

        }
        

        #endregion

        /// <summary>
        /// 绑定默认控件，比如关闭按钮
        /// </summary>
        public virtual void OnBindComponents()
        {
            if (closeBtn == null)
            {
                closeBtn = transform.Find("UI/CloseBtn")?.GetComponent<Button>();
            }
            if (closeBtn != null)
            {
                closeBtn.AddListener(OnCloseClicked);
            }
        }

        public virtual void PlayOpenAnim(Action callback)
        {
            OnBeforeOpenAnim();
            var duration = transform.PlayAnimatorUpdate(dialogData.openAnimName);
            GameBase.Instance.GetModule<DialogManager>().PlayColorMaskOpenAnim(this, duration);
            if (duration > 0)
            {
                this.DelayCall(duration, () =>
                {
                    OnAfterOpenAnim();
                    callback?.Invoke();
                }, true);
            }
            else
            {
                OnAfterOpenAnim();
                callback?.Invoke();
            }
        }
        
        public virtual void PlayCloseAnim(Action callback)
        {
            OnBeforeCloseAnim();

            if (string.IsNullOrEmpty(dialogData.closeAnimName)){
                OnAfterCloseAnim();
                callback?.Invoke();
            }

            var duration = transform.PlayAnimatorUpdate(dialogData.closeAnimName);
            GameBase.Instance.GetModule<DialogManager>().PlayColorMaskOpenAnim(this, duration);
            if (duration > 0)
            {
                this.DelayCall(duration, () =>
                {
                    OnAfterCloseAnim();
                    callback?.Invoke();
                }, true);
            }
            else
            {
                OnAfterCloseAnim();
                callback?.Invoke();
            }
        }

        public virtual void OnCloseClicked()
        {
            CloseDialog();
        }

        public virtual void CloseDialog()
        {
            if (_isClosed) return;
            _isClosed = true;
            PlayCloseAnim(DestroyDialog);
        }

        public void DestroyDialog()
        {
            GameBase.Instance.GetModule<DialogManager>().DestroyDialog(this);
        }
    }
}