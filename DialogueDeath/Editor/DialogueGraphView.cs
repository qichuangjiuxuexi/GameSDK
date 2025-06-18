using GraphProcessor;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

public class DialogueGraphView : BaseGraphView
{
    public DialogueGraphView(EditorWindow window) : base(window)
    {
    }
}

public class DialogueGraphWindow : BaseGraphWindow
{
    
    [OnOpenAsset(0)]
    public static bool OnBaseGraphOpened(int instanceID, int line)
    {
        DialogueGraph asset = EditorUtility.InstanceIDToObject(instanceID) as DialogueGraph;
        if (asset != null)
        {
            GetWindow<DialogueGraphWindow>().InitializeGraph(asset);
            return true;
        }
        return false;
    }
    
    protected override void InitializeWindow(BaseGraph graph)
    {
        titleContent = new GUIContent("对话流程图");
        if (graphView == null)
        {
            graphView = new DialogueGraphView(this);
        }
        rootView.Add(graphView);
    }
    
    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (graphView != null)
        {
            if (graphView.graph)
            {
                (graphView.graph as DialogueGraph)?.ReSortNode();
                EditorUtility.SetDirty(graphView.graph); 
                AssetDatabase.SaveAssetIfDirty(graphView.graph);
                EditorUtility.ClearDirty(graphView.graph);
                AssetDatabase.Refresh(); 
            }
        }
    }

}