using System;
using System.IO;
using System.Linq;
using AppBase.UI;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using AppBase.Tools;
using UnityEditor;
using UnityEngine;

namespace AppBase.UI
{
    public class UIBindingUtil
    {
        [MenuItem("Assets/生成UIBinding", true)] // 验证函数
        static bool ValidateSelection()
        {
            // 确保选中的是 Prefab
            return Selection.activeObject != null &&
                   AssetDatabase.GetAssetPath(Selection.activeObject).EndsWith(".prefab");
        }

        [MenuItem("Assets/生成UIBinding", false, 0)]
        public static void UpdateUIBinding()
        {
            GameObject prefab = (GameObject)Selection.activeObject;

            UIBinding uiBinding = prefab.GetComponent<UIBinding>();
            if (uiBinding == null)
            {
                Debug.LogError("根节点需要添加UIBinding脚本");
                return;
            }

            Type viewType = uiBinding.GetType();

            if (viewType.Name == "UIBinding")
            {
                Debug.LogError("根节点不能直接绑定UIBinding啊啊啊啊");
                return;
            }

            string originPath = FileTools.ExchangeRealPath(AssetDatabase.GetAllAssetPaths()
                .FirstOrDefault(p => p.EndsWith($"/{viewType.Name}.cs")));


            //生成UIBinding代码
            CreateUIBindingCode(prefab, viewType, originPath);
        }

        public static void CreateUIBindingCode(GameObject prefab, Type viewType, string originPath)
        {
            string codePath = originPath.Replace(viewType.Name + ".cs", viewType.Name + ".UIBinding.cs");
            if (Directory.Exists(codePath)) Directory.Delete(codePath);


            //获取到所有的 uibinging
            List<UIBinding> uiBinds = prefab.GetComponentsInChildren<UIBinding>(true).ToList();
            List<UIBinding> removeList = new List<UIBinding>();
            for (int i = 0; i < uiBinds.Count; i++)
            {
                var uiBinding = uiBinds[i];
                if (uiBinding.gameObject == prefab)
                {
                    removeList.Add(uiBinding);
                }
                else
                {
                    Type type = uiBinding.GetType();
                    if (type.Name != "UIBinding")
                    {
                        string path = FileTools.ExchangeRealPath(AssetDatabase.GetAllAssetPaths()
                            .FirstOrDefault(p => p.EndsWith($"/{type.Name}.cs")));
                        CreateUIBindingCode(uiBinding.gameObject, type, path);
                        List<UIBinding> remove = uiBinding.GetComponentsInChildren<UIBinding>().ToList();
                        remove.Remove(uiBinding);
                        removeList.AddRange(remove);
                    }   
                }
            }

            uiBinds.RemoveAll(item => removeList.Contains(item));
            

            using (StreamWriter streamWriter = new StreamWriter(codePath))
            {
                streamWriter.WriteLine("using AppBase.UI;");
                streamWriter.WriteLine();
                streamWriter.WriteLine();
                streamWriter.WriteLine("public partial class " + viewType.Name);
                streamWriter.WriteLine("{"); 
                streamWriter.WriteLine();

                foreach (var uiBind in uiBinds)
                {
                    string type = uiBind.GetType().Name;
                    if (uiBind.transform.parent != null)
                    {
                        string path = GetInPrefabPath(prefab.transform, uiBind.transform);
                        string code = "    public {0} {1} => FindUIBind<{2}>(\"{3}\");";
                        string name = string.IsNullOrEmpty(uiBind.BindingName)
                            ? uiBind.gameObject.name
                            : uiBind.BindingName;
                        Debug.Log(name + " --生成成功");

                        streamWriter.WriteLine(string.Format(code, type, name, type, path));
                    }
                }

                streamWriter.WriteLine();

                streamWriter.WriteLine("}");
            }
            
            //修改uidialog的主类方法为 partial
            List<string> lines = new List<string>(File.ReadAllLines(originPath));
            for (int i = 0; i < lines.Count; i++)
            {
                var strs = lines[i].Split(" ");
                bool isClassLine = false;
                bool isPartial = false;
                bool isTarget = false;
                for (int j = 0; j < strs.Length; j++)
                {
                    if (strs[j] == "class") isClassLine = true;
                    if (strs[j] == "partial") isPartial = true;
                    if (strs[j] ==  viewType.Name) isTarget = true;

                }
                
                if (isClassLine && isTarget && !isPartial)
                {
                    string line = "";
                    for (int j = 0; j < strs.Length; j++)
                    {
                        if (strs[j] == "class")
                        {
                            line += "partial ";
                        }
                        line += strs[j] + " ";
                    }
                    lines[i] = line;
                    File.WriteAllLines(originPath, lines);
                    break;
                }
            }
        }

        private static string GetInPrefabPath(Transform root, Transform target)
        {
            string path = target.name;
            target = target.parent;
            while (target != null && target.name != root.name)
            {
                path = target.name + "/" + path;
                target = target.parent;
            }

            return path;
        }
    }
}