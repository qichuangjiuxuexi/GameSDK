using System;
using AppBase;
using Cysharp.Threading.Tasks;
using GraphProcessor;
using UnityEngine;


[Serializable, NodeMenuItem("Dialogue/Branch Node")]
public class BranchNode : DialogueBaseNode
{
    public override string name => "分支节点";
    
    
    [Input] public int Input;
    [Output("Option 1")] public int option1 = 0;
    [Output("Option 2")] public int option2 = 1;
    
    [TextArea] public string[] options;
    
    /// <summary>
    /// 展示ui
    /// </summary>
    /// <returns></returns>
    protected override async UniTask<bool> ShowDialogueInner()
    {
        BranchUIView view = GameBase.Instance.GetModule<DialogueManager>().GetBranchUIView();
        await view.ShowDialogue(this);
        
        
        Debug.LogError("展示成功！！！！！");
        return true;
    }
    
    /// <summary>
    /// 获取选项
    /// </summary>
    /// <param name="skillGraph"></param>
    /// <returns></returns>
    public async UniTask<int> GetOption()
    {
        Debuggers.Log(Graph.name,$"开始选择分支 {name}");
        int option = 1;
        
        BranchUIView view = GameBase.Instance.GetModule<DialogueManager>().GetBranchUIView();
        option = await view.GetOption();
        
        return option;
    }
}