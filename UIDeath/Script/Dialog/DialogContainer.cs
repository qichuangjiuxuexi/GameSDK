using AppBase;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace AppBase.UI.Dialog
{
    /// <summary>
    /// 存储dialog容器
    /// </summary>
    public class DialogContainer : MonoBehaviour
    {
        public string dialogAddress;
        public Graphic mask;
        public UIDialog dialog;

        public static DialogContainer Create(GameObject parent, string address)
        {
            GameObject obj = parent.AddGameObject(address);
            DialogContainer container = obj.AddComponent<DialogContainer>();
            //屏蔽点击
            var maskImg = container.gameObject.AddComponent<Image>();
            maskImg.color = Color.clear;
            maskImg.isMaskingGraphic = true;
            container.dialogAddress = address;
            obj.AddFullScreenRectTransform();
            
            
            return container;
        }
        
        public UIDialog AddUIDialog(GameObject prefab, DialogData dialogData)
        {
            AddColorMask(dialogData);
            var obj = gameObject.AddInstantiate(prefab);
            UIDialog dialog = obj.GetOrAddComponent<UIDialog>();
            this.dialog = dialog;
            return dialog;
        }

        private void AddColorMask(DialogData dialogData)
        {
            var mask = gameObject.AddGameObject("Mask");
            
            mask.AddFullScreenRectTransform();
            Image image = mask.AddComponent<Image>();
            image.color = dialogData.bgMaskColor;
            image.isMaskingGraphic = false;
            this.mask = mask.GetComponent<Graphic>();
        }
    }
}