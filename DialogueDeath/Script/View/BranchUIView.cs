using System;
using AppBase.UI;
using Cysharp.Threading.Tasks;

public partial class BranchUIView : UIView
{
    public async UniTask<bool> ShowDialogue(BranchNode data)
    {
        gameObject.SetActive(true);
        
        Desc1.TextMeshProUGUI.text = data.options[0];
        Desc2.TextMeshProUGUI.text = data.options[1];

        return true;
    }
    
    public async UniTask<int> GetOption()
    {
        var option = 0;
        var nextCompletion = new UniTaskCompletionSource<bool>();
        void OnClick1()
        {
            option = 0;
            nextCompletion.TrySetResult(true);
        }
        void OnClick2()
        {
            option = 1;
            nextCompletion.TrySetResult(true);
        }
        
        Opption1.Button.onClick.AddListener(OnClick1);
        Opption2.Button.onClick.AddListener(OnClick2);
        
        await nextCompletion.Task;
        
        Opption1.Button.onClick.RemoveAllListeners();
        Opption2.Button.onClick.RemoveAllListeners();
        
        gameObject.SetActive(false);
        return option;
    }
}
