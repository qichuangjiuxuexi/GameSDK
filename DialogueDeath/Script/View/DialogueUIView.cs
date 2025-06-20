using System;
using AppBase.UI;
using Cysharp.Threading.Tasks;

public partial class DialogueUIView : UIView
{
    public async UniTask<bool> ShowDialogue(DialogueNode data)
    {
        gameObject.SetActive(true);
        var nextCompletion = new UniTaskCompletionSource<bool>();
        void OnClick()
        {
            nextCompletion.TrySetResult(true);
        }
        Button.Button.onClick.AddListener(OnClick);
        
        Speaker.TextMeshProUGUI.text = data.speaker+" : ";
        Desc.TextMeshProUGUI.text = data.content;
        
        await nextCompletion.Task;
        
        Button.Button.onClick.RemoveListener(OnClick);
        
        gameObject.SetActive(false);
        return true;
    }
}
