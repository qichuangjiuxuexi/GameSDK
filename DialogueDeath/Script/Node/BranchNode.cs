using System;
using GraphProcessor;


[Serializable, NodeMenuItem("Dialogue/Branch Node")]
public class BranchNode : DialogueBaseNode
{
    public override string name => "分支节点";
    
    
    [Input] public int Input;
    [Output("Option 1")] public int option1;
    [Output("Option 2")] public int option2;
    
    public string[] options;
}