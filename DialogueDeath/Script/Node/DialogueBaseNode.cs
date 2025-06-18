using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GraphProcessor;
using UnityEditor.Graphs;

public class DialogueBaseNode : BaseNode
{

    public DialogueGraph Graph {private set; get; }
    
    /// <summary>
    /// 展示对话框
    /// </summary>
    /// <param name="skillGraph"></param>
    /// <returns></returns>
    public async UniTask<bool> ShowDialogue(DialogueGraph skillGraph)
    {
        Graph = skillGraph;
        Debuggers.Log(Graph.name,$"执行技能步骤 {name}");
        var rst= await ShowDialogueInner();
        if (!rst)
        {
            Debuggers.Log(Graph.name,$"执行技能步骤失败 {name}");
        }
        return rst;
    }
    
    protected virtual async UniTask<bool> ShowDialogueInner()
    {
        return true;
    }
}