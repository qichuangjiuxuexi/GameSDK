using System;
using GraphProcessor;


/// <summary>
/// 对话流程开始节点
/// </summary>
[Serializable, NodeMenuItem("Dialogue/StartNode")]
public class StartNode : DialogueBaseNode
{
    public override string name => "对话开始执行";
    
    [Output] 
    public int next;
}