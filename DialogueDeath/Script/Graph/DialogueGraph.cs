
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using GraphProcessor;
using UnityEngine;
using UnityEngine.Pool;


[Serializable, CreateAssetMenu(fileName = "NewDialogueGraph", menuName = "Dialogue/Dialogue Graph")]
public class DialogueGraph : BaseGraph
{
    private bool IsInit { get; set; }
    
    /// <summary>
    /// 正在执行的步骤
    /// </summary>
    private List<DialogueBaseNode> ExecutingSteps { set; get; }
    /// <summary>
    /// 已完成步骤数量
    /// </summary>
    private int FinishStepCount { set; get; }
        
    private int TotalStepCount { set; get; }
    
    private int ContextId { set; get; } 
    
    public Action<DialogueGraph> SuccessCallback { set; get; }
    public Action<DialogueGraph> ReleaseCallback { set; get; }
    
    private Dictionary<int, BaseNode> NodeCache { set; get; }
    

    public void Init(DialogueData skillData)
    {
        IsInit = true;
        ExecutingSteps = new List<DialogueBaseNode>();
        DictionaryPool<int, BaseNode>.Get(out var nodeCaches);
        NodeCache = nodeCaches;
        foreach (var node in nodes)
        {
            NodeCache.Add(node.computeOrder,node);
        }
    }
    
    public async UniTaskVoid ShowDialogue()
    {
        if (!IsInit)
        {
            Debug.LogError("Graph 还未初始化");
            return;
        }

        await PreLoadAsset();
        Debuggers.Log("Dialogue Graph",$"ShowDialogue {name}");
        var startNode = nodes.Find(s => s is StartNode) as StartNode;
        if (startNode == null)
        {
            Debug.LogError("未找到开始结点");
            return;
        }
        ExecuteNormalNode(ContextId, startNode);
        await UniTask.WaitUntil(() => TotalStepCount == FinishStepCount && ExecutingSteps.Count == 0);
        OnFinishGraph();
        Debuggers.Log("Dialogue Graph",$"Show Dialogue Over {name}");
        ReleaseCallback?.Invoke(this);
    }
    

    private void OnFinishGraph()
    {
        SetUsDialogueSuc();
    }
    
    /// <summary>
    /// 设置对话成功
    /// </summary>
    public void SetUsDialogueSuc()
    {
        SuccessCallback?.Invoke(this);
        SuccessCallback = null;
    }

    /// <summary>
    /// 提前计算一些数据，步数之类的
    /// </summary>
    private async Task PreLoadAsset()
    {
        
    }
    
    /// <summary>
    /// 执行基础节点
    /// </summary>
    /// <param name="contextId"></param>
    /// <param name="baseNode"></param>
    private void ExecuteNormalNode(int contextId, DialogueBaseNode baseNode)
    {
        if (baseNode == null)
        {
            return;
        }

        foreach (var node in GetOutPutNode(baseNode))
        {
            if (node is DialogueBaseNode stepNode)
            {
                ExecuteStep(contextId,stepNode).Forget();
            }
        }
    }

    private async UniTask ExecuteStep(int contextId, DialogueBaseNode dialogueNode)
    {
        ExecutingSteps.Add(dialogueNode);
        TotalStepCount++;

        bool isSucc = await dialogueNode.ShowDialogue(this);

        var outNodes = GetOutPutNode(dialogueNode).ToList();
        if (isSucc)
        {
            if (dialogueNode is BranchNode branchNode)
            {
                int option = await branchNode.GetOption();
                await ExecuteStep(contextId, (DialogueBaseNode)outNodes[option]);
            }
            else
            {
                foreach (var node in outNodes)
                {
                    if (node is DialogueBaseNode baseNode)
                    {
                        await ExecuteStep(contextId, baseNode);
                    }
                }   
            }
        }

        FinishStepCount++;
        ExecutingSteps.Remove(dialogueNode);
    }

    private IEnumerable<BaseNode> GetOutPutNode(DialogueBaseNode baseNode)
    {
        foreach (var i in baseNode.outputOrder)
        {
            yield return NodeCache[i];
        }
    }

    #region Editor

    public void ReSortNode()
    {
        var startNode = nodes.Find(s => s is StartNode) as StartNode;
        if (startNode == null)
        {
            Debug.LogError("重排列节点错误 没有开始结点");
            return;
        }

        void SortOrderInner(BaseNode baseNode)
        {
            baseNode.outputOrder = new List<int>();
            if (baseNode is BranchNode)
            {
                Dictionary<string, int> dic = new();
                int index = 0;
                foreach (var node in baseNode.GetOutputNodes())
                {
                    dic[baseNode.outputPorts[index].portData.displayName] = node.computeOrder;
                    index++;
                }   
                var list = dic
                    .OrderBy(pair => 
                        int.Parse(Regex.Match(pair.Key, @"\d+").Value))
                    .Select(pair => pair.Value)
                    .ToList();
                baseNode.outputOrder = list;
            }
            else
            {
                foreach (var node in baseNode.GetOutputNodes())
                {
                    if (baseNode.outputOrder.Contains(node.computeOrder))
                    {
                        Debug.LogError($"图{name} 排序结点错误 {baseNode}的输出序列有重复 {node} {node.computeOrder}");
                        return;
                    }
                    baseNode.outputOrder.Add(node.computeOrder);
                }   
            }
            foreach (var node in baseNode.GetOutputNodes())
            {
                SortOrderInner(node);
            }
        }
        SortOrderInner(startNode);
    }
    
    protected override void OnEnable()
    {
        base.OnEnable();

        if (nodes.Count == 0)
        {
            AddNode(BaseNode.CreateFromType<StartNode>(Vector2.zero));
        }
    }

    #endregion

}