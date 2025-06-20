using AppBase;
using AppBase.Module;
using AppBase.Resource;
using Cysharp.Threading.Tasks;
using GraphProcessor;
using UnityEngine;

public class DialogueManager : MonoModule
{
    public override string GameObjectPath => "UICanvas/Dialogue";
    
    private DialogueUIView dialogueUIView;
    private BranchUIView branchUIView;
    
    
    protected override void OnInit()
    {
        base.OnInit();
        
        dialogueUIView = GameObject.GetComponentInChildren<DialogueUIView>(true);
        dialogueUIView?.gameObject.SetActive(false);
        
        branchUIView = GameObject.GetComponentInChildren<BranchUIView>(true);
        branchUIView?.gameObject.SetActive(false);

    }

    public DialogueUIView GetDialogueUIView()
    {
        return dialogueUIView;
    }
    
    public BranchUIView GetBranchUIView()
    {
        return branchUIView;
    }
    
    public async UniTaskVoid ExecuteDialogue(DialogueData dialogue)
    {
        Debuggers.Log(TAG, "Execute Dialogue path: " + dialogue.DialoguePath);
        
        var startTime = Time.realtimeSinceStartupAsDouble;
        
        var graph = LoadInstanceDialogue(dialogue);
        if (graph == null)
        {
            return;
        }
        
        graph.ShowDialogue().Forget();
        Debuggers.Log(TAG,$"Show Dialogue CostTime {Time.realtimeSinceStartupAsDouble - startTime}");

    }
    

    private DialogueGraph LoadInstanceDialogue(DialogueData dialogue)
    {
        var path = dialogue.DialoguePath;
        if (string.IsNullOrEmpty(path))
        {
            return null;
        }

        var graph = LoadGraph(path);
        if (graph == null)
        {
            return null;
        }

        string json = JsonUtility.ToJson(graph);
        DialogueGraph newInstance = ScriptableObject.CreateInstance<DialogueGraph>();
        JsonUtility.FromJsonOverwrite(json, newInstance);
        newInstance.name = graph.name;
        newInstance.Init(dialogue);

        return newInstance;
    }


    public DialogueGraph LoadGraph(string path)
    {
        return (GameBase.Instance.GetModule<ResourceManager>().LoadAsset<DialogueGraph>(path, this.GetResourceReference())
            .WaitForCompletion() as DialogueGraph);
    }
}