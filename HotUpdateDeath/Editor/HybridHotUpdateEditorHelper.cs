using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AppBase.Tools;
using HybridCLR.Editor;
using HybridCLR.Editor.Commands;
using HybridCLR.Editor.HotUpdate;
using HybridCLR.Editor.Installer;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine;

public class HybridHotUpdateEditorHelper
{
    static AddressableAssetSettings setting => AddressableAssetSettingsDefaultObject.Settings;
    static string HotUpdateDllPath => $"{Application.dataPath}/../HybridCLRData/HotUpdateDlls/{EditorUserBuildSettings.activeBuildTarget}/";
    public static string HotUpdateDestinationPath => $"{Application.dataPath}/HotUpdateDlls/HotUpdateDll/";
    static string AOTGenericReferencesPath => $"{Application.dataPath}/HybridCLRGenerate/AOTGenericReferences.cs";
    static string MetaDataDLLPath => $"{Application.dataPath}/../HybridCLRData/AssembliesPostIl2CppStrip/{EditorUserBuildSettings.activeBuildTarget}/";
    static string MetaDataDestinationPath => $"{Application.dataPath}/HotUpdateDlls/MetaDataDll/";
    static string BuildDataPath => $"{Application.dataPath}/../BuildData/";

    static string CurrPlatformBuildDataPath => $"{BuildDataPath}{EditorUserBuildSettings.activeBuildTarget}/";
    static string META_DATA_DLLS_TO_LOAD_PATH = "Assets/HotUpdateDlls/MetaDataDllToLoad.txt";


    
    public static void BuildHotUpdateDlls(bool isBuildPlayer)
    {
        Debug.LogError("开始构建热更新DLL");
        //如果未安装，安装
        var controller = new InstallerController();
        if (!controller.HasInstalledHybridCLR())
            controller.InstallDefaultHybridCLR();

        //执行HybridCLR
        PrebuildCommand.GenerateAll();

        //如果是更新，则检查热更代码中是否引用了被裁减的AOT代码
        if (!isBuildPlayer)
            if (!CheckAccessMissingMetadata())
                return;
        
        //拷贝dll
        CopyHotUpdateDll();
        CopyMetaDataDll();
            
        //如果是发包，则拷贝AOT dll
        if (isBuildPlayer)
            CopyAotDllsForStripCheck();
            
        //收集RuntimeInitializeOnLoadMethod 先不处理这里
        CollectRuntimeInitializeOnLoadMethod();
    }
    
    private static void CopyHotUpdateDll()
    {
        var assemblies = SettingsUtil.HotUpdateAssemblyNamesExcludePreserved;
        var dir = new DirectoryInfo(HotUpdateDllPath);
        var files = dir.GetFiles();
        
        FileTools.SafeDeleteFile(HotUpdateDestinationPath);
        FileTools.CheckCreatFile(HotUpdateDestinationPath);
        
        List<string> hotUpdateDllPath = new List<string>();
        foreach (var fileInfo in files)
        {
            if (fileInfo.Extension == ".dll" && assemblies.Contains(fileInfo.Name.Substring(0, fileInfo.Name.Length - 4)))
            {
                var targetPah = HotUpdateDestinationPath + fileInfo.Name + ".bytes";
                fileInfo.CopyTo(targetPah, true);
                targetPah = FileTools.DiskPathToAssetPath(targetPah);
                hotUpdateDllPath.Add(targetPah);
            }
        }

        AssetDatabase.SaveAssets();

        // 手动更新HotfixAsm.dll文件的时间戳
        string hotfixAsmPath = HotUpdateDestinationPath + "HotfixAsm.dll.bytes";
        if (File.Exists(hotfixAsmPath))
        {
            DateTime lastWriteTime = File.GetLastWriteTime(hotfixAsmPath);
            File.SetLastWriteTime(hotfixAsmPath, DateTime.Now);
            AssetDatabase.ImportAsset(hotfixAsmPath);
            lastWriteTime = File.GetLastWriteTime(hotfixAsmPath);
        }
        else
        {
            Debug.LogError($"HotfixAsm.dll.bytes 文件不存在，路径为：{hotfixAsmPath}");
        }

        Debug.Log("CopyHotUpdateDll success!!!");
        
        // 使用 EditorUtility.RevealInFinder 方法打开 HotfixAsm.dll.bytes 文件所在的目录
        EditorUtility.RevealInFinder(HotUpdateDestinationPath);

        // 确保新拷贝的dll文件被Addressables系统管理
        foreach (var dllPath in hotUpdateDllPath)
        {
            string guid = AssetDatabase.AssetPathToGUID(dllPath);
            var hotUpdateGroup2 = setting.groups.Find(g => g.name == "HotUpdateDlls");
            if (hotUpdateGroup2 != null)
            {
                var entry = setting.CreateOrMoveEntry(guid, hotUpdateGroup2);
                if (entry != null)
                {
                }
            }
            else
            {
                Debug.LogError("HotUpdateDll Group not found!");
            }
        }


        // 打印HotUpdateDll组中的所有AssetEntry
        var hotUpdateGroup = setting.groups.Find(g => g.name == "HotUpdateDlls");
        if (hotUpdateGroup != null)
        {
            Debug.Log($"HotUpdateDll Group contains the following assets:");
            foreach (var entry in hotUpdateGroup.entries)
            {
                Debug.Log($"  - {entry.address} - {entry.guid}");
            }
        }
        else
        {
            Debug.Log("HotUpdateDll Group not found!");
        }

        // 打印HotUpdateDll组的配置信息
        if (hotUpdateGroup != null)
        {
            Debug.Log($"HotUpdateDll Group Schema: {hotUpdateGroup.Schemas.GetType()}");
            BundledAssetGroupSchema bundleSchema = hotUpdateGroup.GetSchema<BundledAssetGroupSchema>();
            if (bundleSchema != null)
            {
                Debug.Log($"HotUpdateDll Group Bundle Mode: {bundleSchema.BundleMode}");
            }
            else
            {
                Debug.Log("HotUpdateDll Group does not have BundledAssetGroupSchema!");
            }
        }
    }
    private static void CopyMetaDataDll()
    {
        List<String> dllList = GetMetaDataDllList();
        DirectoryInfo metaInfo = new DirectoryInfo(MetaDataDLLPath);
        var files = metaInfo.GetFiles();

        FileTools.SafeDeleteFile(MetaDataDestinationPath);
        FileTools.CheckCreatFile(MetaDataDestinationPath);
        
        
        List<string> metaDllPath = new List<string>();
        foreach (var fileInfo in files)
        {
            if (dllList.Contains(fileInfo.Name))
            {
                var targetPah = MetaDataDestinationPath + fileInfo.Name + ".bytes";
                fileInfo.CopyTo(targetPah, true);
                Debug.Log(fileInfo.Name);
                
                // 标准化路径分隔符
                targetPah = FileTools.DiskPathToAssetPath(targetPah);
                metaDllPath.Add(targetPah);
            }
        }
        
        
        
        var metaDataDllListStr = string.Join("|", dllList);
        if (!File.Exists(META_DATA_DLLS_TO_LOAD_PATH))
        {
            using (File.Create(META_DATA_DLLS_TO_LOAD_PATH))
            {
            }
        }
        File.WriteAllText(META_DATA_DLLS_TO_LOAD_PATH, metaDataDllListStr, Encoding.UTF8);
        
        AssetDatabase.SaveAssets();
        Debug.Log("CopyMetaDataDll success!!!");
    }
    

    private static List<string> GetMetaDataDllList()
    {
        var aotGenericRefPath = AOTGenericReferencesPath;
        List<string> result = new List<string>();
        using (StreamReader reader = new StreamReader(aotGenericRefPath))
        {
            var lineStr = "";
            while (!reader.ReadLine().Contains("new List<string>"))
            {
            }

            reader.ReadLine();
            while (true)
            {
                lineStr = reader.ReadLine().Replace("\t", "");
                if (lineStr.Contains("};"))
                    break;
                var dllName = lineStr.Substring(1, lineStr.Length - 3);
                result.Add(dllName);
            }
        }

        return result;
    }
    
    private static void CopyAotDllsForStripCheck()
    {
        if (!Directory.Exists(BuildDataPath))
            Directory.CreateDirectory(BuildDataPath);
        var dir = new DirectoryInfo(MetaDataDLLPath);
        var files = dir.GetFiles();
        var destDir = CurrPlatformBuildDataPath;
        if (Directory.Exists(destDir))
            Directory.Delete(destDir, true);
        Directory.CreateDirectory(destDir);
        foreach (var file in files)
        {
            if (file.Extension == ".dll")
            {
                var desPath = destDir + file.Name;
                file.CopyTo(desPath, true);
            }
        }
    }
    
    private static bool CheckAccessMissingMetadata()
    {
        BuildTarget target = EditorUserBuildSettings.activeBuildTarget;
        string aotDir = CurrPlatformBuildDataPath;
        var checker = new MissingMetadataChecker(aotDir, new List<string>());

        string hotUpdateDir = SettingsUtil.GetHotUpdateDllsOutputDirByTarget(target);
        foreach (var dll in SettingsUtil.HotUpdateAssemblyFilesExcludePreserved)
        {
            string dllPath = $"{hotUpdateDir}/{dll}";
            bool notAnyMissing = checker.Check(dllPath);
            if (!notAnyMissing)
            {
                Debug.LogError($"Update player failed!some hotUpdate dll:{dll} is using a stripped method or type in AOT dll!Please rebuild a player!");
                return false;
            }
        }

        return true;
    }
    
    private static void CollectRuntimeInitializeOnLoadMethod()
    {
        
    }
}
