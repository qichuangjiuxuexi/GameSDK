using System.Collections.Generic;
using System.IO;
using System.Linq;
using AppBase.Tools;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine;

namespace AppBase.Resource
{
    public class AddressAbleUtil
    {
        private const string FeaturePath = "Assets/Project/Feature/Address"; //feature文件夹，检测此文件夹所有文件 结尾为_dl 的需要生成一个引用
        private const string IgnoreFile = ".meta,.DS_Store,.cs";
        private const string AAConstPath = "Assets/Project/Feature/Common/Script/AAConst.cs";
        private const string HotUpdateDlls = "HotUpdateDlls";
        private const string MetaDataDlls = "MetaDataDlls";

        static AddressableAssetSettings Settings
        {
            get { return AddressableAssetSettingsDefaultObject.Settings; }
        }


        [MenuItem("Tools/UpdateAddressAble")]
        public static void UpdateAddressAble()
        {
            // 清除所有组
            var groups = Settings.groups;
            var groupsToDelete = new List<AddressableAssetGroup>(groups);
            AddressableAssetGroup defaultGroup = Settings.DefaultGroup;
            foreach (var group in groupsToDelete)
            {
                // 确保不删除默认组
                if (group != null && group != defaultGroup && group.HasSchema<PlayerDataGroupSchema>() == false
                    && group.Name != HotUpdateDlls && group.Name != MetaDataDlls)
                {
                    Settings.RemoveGroup(group);
                    Debug.Log($"Removed Addressable Group: {group.Name}");
                }
            }

            UpdateAddressAble(FeaturePath);

            //更新AAConst文件
            UpdateConstScript();
        }

        public static void UpdateAddressAble(string targetPath)
        {
            var allFile = FileTools.GetAllDirectories(targetPath);
            foreach (var path in allFile)
            {
                string m_fileName = FileTools.GetFileName(path);

                //查看是否需要创建group
                if (m_fileName.StartsWith("m_"))
                {
                    var group = Settings.FindGroup(m_fileName);
                    if (group == null)
                    {
                        // 创建并添加一个 BundledAssetGroupSchema
                        var bundledSchema = ScriptableObject.CreateInstance<BundledAssetGroupSchema>();
                        bundledSchema.BuildPath.SetVariableByName(Settings, AddressableAssetSettings.kLocalBuildPath);
                        bundledSchema.LoadPath.SetVariableByName(Settings, AddressableAssetSettings.kLocalLoadPath);
                        group = Settings.CreateGroup(m_fileName,
                            false, //不使用默认分组
                            false, //是否只读
                            true, //使用本地组
                            new List<AddressableAssetGroupSchema> { bundledSchema });
                        // 添加 Content Update Group Schema
                        ContentUpdateGroupSchema contentUpdateGroupSchema = group.AddSchema<ContentUpdateGroupSchema>();
                        // 可以根据需要修改 schema 的属性
                        contentUpdateGroupSchema.StaticContent = true;
                    }

                    // 移除组中的所有资源
                    var entries = group.entries.ToArray();
                    foreach (var entry in entries)
                    {
                        group.RemoveAssetEntry(entry);
                    }

                    //将需要加入group的资源放入group
                    var files = FileTools.GetAllFiles(path);
                    foreach (var filepath in files)
                    {
                        string fileType = FileTools.GetFileType(filepath);
                        if (!IgnoreFile.Contains(fileType))
                        {
                            AddressableAssetEntry entry =
                                Settings.CreateOrMoveEntry(AssetDatabase.AssetPathToGUID(filepath), group, false,
                                    false);
                            EditorUtility.SetDirty(Settings);
                        }
                    }

                    AssetDatabase.SaveAssets();
                }
            }

            UpdateConstScript();
            AssetDatabase.SaveAssets();
        }

        private static void UpdateConstScript()
        {
            if (Directory.Exists(AAConstPath))
            {
                //存在则先删除AAConst文件
                File.Delete(AAConstPath);
                File.Delete(AAConstPath + ".meta"); // 也删除.meta文件
            }

            List<string> dynamicList = new List<string>(2);
            var allFile = FileTools.GetAllDirectories(FeaturePath);
            foreach (var path in allFile)
            {
                string m_fileName = FileTools.GetFileName(path);
                if (m_fileName.EndsWith("_dl"))
                {
                    //将需要加入group的资源放入group
                    var files = FileTools.GetAllFiles(path);
                    foreach (var filePath in files)
                    {
                        string fileType = FileTools.GetFileType(filePath);
                        if (!IgnoreFile.Contains(fileType))
                        {
                            dynamicList.Add(filePath);
                        }
                    }
                }
            }

            using (StreamWriter writer = new StreamWriter(AAConstPath))
            {
                writer.WriteLine("using UnityEngine;");
                writer.WriteLine("using System.Collections.Generic;");
                writer.WriteLine();
                writer.WriteLine("public class AAConst");
                writer.WriteLine("{");

                // 插入属性和字段
                foreach (var filePath in dynamicList)
                {
                    string fileName = FileTools.GetFileName(filePath, true);
                    // Debug.Log(fileName);
                    writer.WriteLine($"    public  const string {fileName} = \"{filePath}\";");
                }


                writer.WriteLine("");
                writer.WriteLine("");


                //插入一个字典
                writer.WriteLine(
                    "    private static readonly Dictionary<string, string> AddressableDict = new Dictionary<string, string>()");
                writer.WriteLine("    {");
                foreach (var filePath in dynamicList)
                {
                    string fileName = FileTools.GetFileName(filePath, true);
                    writer.WriteLine("        {" + $"\"{fileName}\", {fileName}" + "},");
                }

                writer.WriteLine("    };");
                // 插入一个示例方法
                writer.WriteLine();
                writer.WriteLine("    public static string GetAddress(string key)");
                writer.WriteLine("    {");
                writer.WriteLine("        if(string.IsNullOrEmpty(key))");
                writer.WriteLine("        {");
                writer.WriteLine("            return null;");
                writer.WriteLine("        }");
                writer.WriteLine("        if(AddressableDict.TryGetValue(key, out var address))");
                writer.WriteLine("        {");
                writer.WriteLine("            return address;");
                writer.WriteLine("        }");
                writer.WriteLine("        Debug.LogError(\"address 不存在，key: \"+key);");
                writer.WriteLine("        return \"\";");
                writer.WriteLine("    }");

                writer.WriteLine("}");
            }

            AssetDatabase.Refresh();
        }
    }
}