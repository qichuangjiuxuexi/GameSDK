using System;
using AppBase.Module;
using AppBase.Resource;
using System.Collections.Generic;
using AppBase;
using UnityEngine;

namespace AppBase.UI.Dialog
{
    public class DialogManager : MonoModule
    {
        public override string GameObjectPath => "UICanvas/Dialogs";

        private List<DialogContainer> dialogList;

        public void PopupDialog(string address, object data = null, Action<UIDialog> loadedCallback = null, Action<UIDialog> openCallback = null, Action<UIDialog> closeCallback = null)
        {
            if(string.IsNullOrEmpty(address)) return;
            DialogData dialogData = new DialogData(address, data, loadedCallback, openCallback, closeCallback);
            PopupDialog(dialogData);
        }

        public void PopupDialog(DialogData data)
        {
            string address = data.address;
            if(string.IsNullOrEmpty(address)) return;
            data.handler = GameBase.Instance.GetModule<ResourceManager>().LoadAssetHandler<GameObject>(data.address, handler =>
            {
                var gameobject = handler.GetAsset<GameObject>();
                DialogContainer container = DialogContainer.Create(GameObject, data.address);
                UIDialog dialog = container.AddUIDialog(gameobject, data);
                //handle 给dialog
                dialog.gameObject.GetResourceReference().AddHandler(handler);
                dialog.Init(data);
                dialog.dialogContainer = container;

                dialogList ??= new List<DialogContainer>();
                dialogList.Add(container);
                
                dialog.OnLoaded(() => 
                {
                    //弹出
                    //load生命周期之后是 awake 和 start
                    dialog.GetOrAddComponent<DialogRuntime>();
                    //细狗加载使用的handle
                    handler.Release();
                    CheckTopMask();
                });
            }, () =>
            {
                data.handler = null;
                Debug.LogError("弹窗弹出失败 ->" + address);
            });
        }
        //mask open 动画
        public void PlayColorMaskOpenAnim(UIDialog uiDialog,float duration)
        {
            
        }
        public void DestroyDialog(UIDialog dialog)
        {
            if (dialog == null || dialog.dialogContainer == null) return;
            var container = dialog.dialogContainer;
            if (container.dialogAddress == null) return;
            container.dialogAddress = null;
            dialog.OnBeforeDestroy();
            dialog.dialogData?.OnCloseCallback(dialog);
            container.dialog = null;
            dialog.dialogContainer = null;
            GameObject.Destroy(container.gameObject);
            dialogList.Remove(container);
            CheckTopMask();
        }

        public UIDialog GetTopDialog()
        {
            if (dialogList.Count > 0){
                for (int i = 0; i < dialogList.Count; i++){
                    var container = dialogList[i];
                    if (container == null){
                        return null;
                    }
                    return container.dialog;
                }
            }
            
            return null;
        }

        private void CheckTopMask()
        {
            if (dialogList.Count > 0){
                for (int i = 0; i < dialogList.Count; i++){
                    var container = dialogList[i];
                    if (container != null && container.mask!=null){
                        container.mask.SetActive(i==dialogList.Count-1);
                    }
                }
            }
        }
    }
}