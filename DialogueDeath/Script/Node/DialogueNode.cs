using System;
using System.Collections;
using System.Collections.Generic;
using AppBase;
using Cysharp.Threading.Tasks;
using GraphProcessor;
using UnityEngine;

[Serializable, NodeMenuItem("Dialogue/Dialogue Node")]
public class DialogueNode : DialogueBaseNode
{
    [Input] public int Input;
    
    [Output("Next")] public int next;

    [TextArea] public string speaker;
    
    [TextArea] public string content;
    
    protected override async UniTask<bool> ShowDialogueInner()
    {
        DialogueUIView view = GameBase.Instance.GetModule<DialogueManager>().GetDialogueUIView();

        await view.ShowDialogue(this);
        Debug.LogError("展示成功！！！！！");
        return true;
    }
}
