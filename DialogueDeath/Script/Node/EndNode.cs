using System;
using GraphProcessor;

[Serializable, NodeMenuItem("Dialogue/EndNode")]
public class EndNode : BaseNode
{
    public override string name => "结束对话";
    
    [Input] public int Input;        
}