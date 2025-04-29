using System;
using System.Collections.Generic;
using System.IO;
using AppBase.Tools;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine;

public class HotUpdateEditorHelper
{
    private const string CONTENT_UPDATE_GROUP_NAME = "Content Update";

    // 重试次数常量
    private const int RETRY_COUNT = 3;

    // 超时时间常量
    private const int TIME_OUT = 10;
    private const string HotUpdateLabel = "HotUpdate";
    private static AddressableAssetSettings DefaultSettings => AddressableAssetSettingsDefaultObject.Settings;

    private static Dictionary<string, AddressableAssetEntry> removeEntry = new Dictionary<string, AddressableAssetEntry>(); 

    private static AddressableAssetGroup ContentUpdateGroup =>
        DefaultSettings.groups.Find(g => g.name.Contains(CONTENT_UPDATE_GROUP_NAME));
    
    private static List<string> HotUpdateDlls = new List<string>{"HotUpdateDlls"};
    
    // 菜单项:构建内容和玩家
    [MenuItem("Tools/Build/BuildContentAndPlayer")]
    private static void BuildContentAndPlayerWithHybridCLR()
    {
        // 构建热更新DLL
        HybridHotUpdateEditorHelper.BuildHotUpdateDlls(true);
        // 构建内容和玩家
        BuildContentAndPlayer();
        AssetDatabase.Refresh();
    }

    // 菜单项:更新已构建的玩家
    [MenuItem("Tools/Build/UpdatePreviousBuild")]
    private static void UpdatePreviousPlayerWithHybridCLR()
    {
        // 检查编辑器状态
        if (EditorApplication.isCompiling || EditorApplication.isUpdating)
        {
            Debug.LogError("Cannot update while editor is compiling or updating");
            return;
        }

        // 检查是否处于播放模式
        if (EditorApplication.isPlaying)
        {
            Debug.LogError("Cannot update while in play mode");
            return;
        }

        // BackupHotfixDl();
        
        // 构建热更新DLL
        HybridHotUpdateEditorHelper.BuildHotUpdateDlls(false);

        // 更新玩家
        UpdatePreviousPlayer();

        
        // 刷新资源数据库
        AssetDatabase.Refresh();
    }

    // public static void BackupHotfixDl()
    // {
    //     string sourceFilePath = Path.Combine(HybridHotUpdateEditorHelper.HotUpdateDestinationPath, "HotfixAsm.dll.bytes");
    //     string backupFilePath = Path.Combine(HybridHotUpdateEditorHelper.HotUpdateDestinationPath, "HotfixAsm.dll.bytes.Bak");
    //
    //     // 检查源文件是否存在
    //     if (File.Exists(sourceFilePath))
    //     {
    //         // 复制文件并重命名为备份文件
    //         File.Copy(sourceFilePath, backupFilePath, overwrite: true);
    //         Debug.Log($"File copied and renamed to {backupFilePath}");
    //     }
    //     else
    //     {
    //         Debug.LogError($"Source file does not exist: {sourceFilePath}");
    //     }
    // }

    // public static void RestoreHotfixDll()
    // {
    //     string sourceFilePath = Path.Combine(HybridHotUpdateEditorHelper.HotUpdateDestinationPath, "HotfixAsm.dll.bytes");
    //     string backupFilePath = Path.Combine(HybridHotUpdateEditorHelper.HotUpdateDestinationPath, "HotfixAsm.dll.bytes.Bak");
    //
    //     // 检查备份文件是否存在
    //     if (File.Exists(backupFilePath))
    //     {
    //         // 复制备份文件并覆盖原文件
    //         File.Copy(backupFilePath, sourceFilePath, overwrite: true);
    //         FileTools.SafeDeleteFile(backupFilePath);
    //         Debug.Log($"File restored from {backupFilePath} to {sourceFilePath}");
    //         var hotUpdateGroup = DefaultSettings.groups.Find(g => g.name == "HotUpdateDlls");
    //         foreach (var addressableAssetEntry in removeEntry)
    //         {
    //             DefaultSettings.CreateOrMoveEntry(AssetDatabase.AssetPathToGUID(addressableAssetEntry.Key), hotUpdateGroup);
    //         }
    //     }
    //     else
    //     {
    //         Debug.LogError($"Backup file does not exist: {backupFilePath}");
    //     }
    // }

    private static void BuildContentAndPlayer()
    {
        BuildAddressableContent();
        OnlyBuildPlayer();
    }

    private static void BuildAddressableContent()
    {
        DeleteContentGroup();
        AddressableAssetSettings.BuildPlayerContent(out AddressablesPlayerBuildResult result);

        // 检查构建是否成功
        bool success = string.IsNullOrEmpty(result.Error);

        if (success)
        {
            Debug.LogError("构建AddressableContent失败");
        }
    }

    // 更新已构建的玩家
    private static void UpdatePreviousPlayer()
    {
        // 删除Content Update Group
        DeleteContentGroup();

        // 清除Addressable Asset Settings中的缓存
        AddressableAssetSettings.CleanPlayerContent();

        // 获取内容状态数据路径
        var path = ContentUpdateScript.GetContentStateDataPath(false);

        // 检查路径是否有效
        if (string.IsNullOrEmpty(path))
        {
            Debug.LogError($"路径为空! 路径: {path}");
            return;
        }

        // 收集已修改的资源条目
        var modifiedEntries = ContentUpdateScript.GatherModifiedEntries(DefaultSettings, path);

        // 确保标签在项目的标签表中存在
        var existingLabels = DefaultSettings.GetLabels();
        
        if (!existingLabels.Contains(HotUpdateLabel))
        {
            DefaultSettings.AddLabel(HotUpdateLabel);
        }

        // 创建列表存储有效修改的资源
        var validModifiedEntries = new List<AddressableAssetEntry>();
        // 详细检查每个资源的修改状态
        foreach (var entry in modifiedEntries)
        {
            var assetPath = AssetDatabase.GUIDToAssetPath(entry.guid);
            if (!string.IsNullOrEmpty(assetPath))
            {
                // 检查资源是否被真正修改
                var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);
                if (asset != null)
                {
                    validModifiedEntries.Add(entry);
                }
            }
        }

        if (validModifiedEntries.Count == 0)
        {
            // RestoreHotfixDll();
            Debug.LogError("没有检测到需要更新的资源");
            return;
        }
        
        HotfixDllList hotfixDllListAsset = ScriptableObject.CreateInstance<HotfixDllList>();
        bool hasDllUpdate = false;
        List<AddressableAssetEntry> dllList = new List<AddressableAssetEntry>();
        foreach (var entry in validModifiedEntries)
        {
            var assetPath = entry.AssetPath;
            bool isUpdateDll = false;
            if (assetPath.EndsWith("bytes"))
            {
                dllList.Add(entry);
                for (int i = 0; i < HotUpdateDlls.Count; i++)
                {
                    if (Path.GetFileName(assetPath)!.Contains(HotUpdateDlls[i]))
                    {
                        isUpdateDll = true;
                        // 从文件系统读取字节数据
                        string filePath = entry.AssetPath;
                        byte[] dllData = File.ReadAllBytes(filePath);
                        HotfixDllData data = new HotfixDllData
                        {
                            name = Path.GetFileName(filePath),
                            dllData = dllData
                        };
                        hotfixDllListAsset.list.Add(data);
                        removeEntry[filePath] = entry;
                        hasDllUpdate = true;
                    }
                }
            }
            if (!isUpdateDll)
            {
                entry.SetLabel(HotUpdateLabel, true, true);
            }
        }
        //清理dll文件从热更包
        validModifiedEntries.RemoveAll(entry =>
        {
            var exists = dllList.Exists(item => item == entry);
            return exists;
        });
        //创建热更dll 文件放入热更包
        if (hasDllUpdate)
        {
            AssetDatabase.CreateAsset(hotfixDllListAsset, HotfixDllList.Path);
            var hotUpdateGroup = DefaultSettings.groups.Find(g => g.name == "HotUpdateDlls");
            AddressableAssetEntry newEntry = DefaultSettings.CreateOrMoveEntry(AssetDatabase.AssetPathToGUID(HotfixDllList.Path), hotUpdateGroup);
            newEntry.SetLabel(HotUpdateLabel, true, true);
            newEntry.address = HotfixDllList.Address;
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            validModifiedEntries.Add(newEntry);   
        }
       
        // RestoreHotfixDll();
        
        // 创建Content Update Group
        ContentUpdateScript.CreateContentUpdateGroup(DefaultSettings, validModifiedEntries, CONTENT_UPDATE_GROUP_NAME);


        // 获取BundledAssetGroupSchema
        var schema = ContentUpdateGroup.GetSchema<BundledAssetGroupSchema>();
        
        // 设置group的重试次数和超时时间
        schema.RetryCount = RETRY_COUNT;
        schema.Timeout = TIME_OUT;

        // 构建内容更新
        ContentUpdateScript.BuildContentUpdate(DefaultSettings, path);
    }

    private static void OnlyBuildPlayer()
    {
        var options = new BuildPlayerOptions();
        BuildPlayerOptions playerOptions = BuildPlayerWindow.DefaultBuildMethods.GetBuildPlayerOptions(options);

        BuildPipeline.BuildPlayer(playerOptions);
    }

    private static void DeleteContentGroup()
    {
        if (ContentUpdateGroup != null)
        {
            DefaultSettings.RemoveGroup(ContentUpdateGroup);
        }
    }
}