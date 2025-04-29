using AppBase.UI;
using UnityEngine;
using UnityEditor;

[InitializeOnLoad]
public class HierarchyLabelManager
{
    static HierarchyLabelManager()
    {
        EditorApplication.hierarchyWindowItemOnGUI += HierarchyWindowItemOnGUI;
    }

    static void HierarchyWindowItemOnGUI(int instanceID, Rect selectionRect)
    {
        GameObject go = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
        
        if (go != null && go.GetComponent<UIBinding>() != null)
        {
            // 计算标签位置
            Rect labelRect = new Rect(selectionRect);
            labelRect.x = selectionRect.xMax - 60; // 在原有名字后面偏移
            labelRect.width = 40; // 标签宽度

            // 绘制黄色文字
            GUI.color = Color.yellow;
            GUI.Label(labelRect, "UIBinding");
            GUI.color = Color.white; // 恢复默认颜色
        }
    }
}