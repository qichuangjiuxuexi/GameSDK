using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using HybridCLR;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public class GameLauncher : MonoBehaviour
{
    public Image progress;
    public TextMeshProUGUI progressText;
    public TextMeshProUGUI descText;
    
    
    public bool enableHybridCLR = true;
    private byte[] _dllBytes;
    private Dictionary<string, Assembly> _allHotUpdateAssemblies = new();

    private long _hotFixDownloadSize;
    private bool _hasNewVersion;
    private bool _isVerifyRemoteCatalog;
    private Assembly hotfixAsm;
    


    private Coroutine _launchCoroutine;
    private string META_DATA_DLLS_TO_LOAD_PATH = "Assets/HotUpdateDlls/MetaDataDllToLoad.txt";
    static string META_DATA_DLL_PATH = "Assets/HotUpdateDlls/MetaDataDll/";
    const string HOT_UPDATE_DLL_PATH = "Assets/HotUpdateDlls/HotUpdateDll/";
    private string HotUpdateDll = "HotfixAsm.dll";
    
    private string versionUrl = "https://www.qichuangjiuxuexi.cn/Version.txt"; // 替换为您的版本文件 URL
    private string localVersion = "1.0"; // 替换为您的本地版本
    private string targetVersion = "0.0.1";
    private string hotUpdateLabel = "HotUpdate"; // 热更新 Label
    private string catalogUrl = "https://www.qichuangjiuxuexi.cn/CatalogUrl.txt";
    private string catalogPath = "";
    private AsyncOperationHandle m_InitializeOperationHandle;

    
    public string CatalogLocalPath => Path.Combine(Application.persistentDataPath, "com.unity.addressables", $"catalog_{targetVersion}.json");

    ///热更程序集依赖的热更程序集，这些程序集要先于gameplay程序集加载，需要手动填写
    private readonly List<string> _gamePlayDependencyDlls = new List<string>()
    {
    };

    private void Start()
    {
        OnProcess(0);
        m_InitializeOperationHandle = Addressables.InitializeAsync();
        _launchCoroutine = StartCoroutine(Launch());
    }

    private IEnumerator Launch()
    {
#if UNITY_EDITOR
        enableHybridCLR = false;
#else
        yield return CheckVersion();
        yield return CheckForCatalogUpdates();
        if (_isVerifyRemoteCatalog && _hasNewVersion)
        {
            descText.text += "Check For Catalog Updates!" + "\n";
            Debug.Log("Check For Catalog Updates!");
            yield return CheckDownloadSize();
            descText.text += "Start Download Assets!" + "\n";
            Debug.Log("Start Download Assets!");
            yield return VersionUpdate();
        }
#endif
        
        descText.text += "start loadAsm! " + "\n";
        Debug.Log("start loadAsm!");

        yield return LoadAssemblies();
        descText.text += "start Game! " + "\n";
        Debug.Log("start Game!");
        yield return StartGame();
    }
    
    IEnumerator CheckVersion()
    {
        yield return m_InitializeOperationHandle.Task;
        
        // 获取 catalogPath，从服务器获取catalogPath，提高灵活性
        UnityWebRequest catalogRequest = UnityWebRequest.Get(catalogUrl);
        yield return catalogRequest.SendWebRequest();

        if (catalogRequest.result == UnityWebRequest.Result.Success)
        {
            catalogPath = catalogRequest.downloadHandler.text;
            Debug.Log("catalogPath: " + catalogPath);
            descText.text += "catalogPath: " + catalogPath + "\n";
            Debug.Log("catalogPath: " + catalogPath);

        }
        else
        {
            Debug.LogError("catalogPath check failed: " + catalogRequest.error);
            descText.text += "catalogPath check failed: " + catalogRequest.error + "\n";
            Debug.Log("catalogPath check failed: " + catalogRequest.error);
            yield break;
        }

        descText.text += "addressable initialize success! " + "\n";
        Debug.Log("addressable initialize success! ");
        UnityWebRequest www = UnityWebRequest.Get(versionUrl);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            string serverVersion = www.downloadHandler.text;
            if (serverVersion != localVersion)
            {
                descText.text += "new version: " + serverVersion + "\n";
                Debug.Log("new version: " + serverVersion);
                _hasNewVersion = true;
                descText.text += "Verify Remote Catalog!" + "\n";
                Debug.Log("Verify Remote Catalog!");
                using (UnityWebRequest request = UnityWebRequest.Get(catalogPath))
                {
                    yield return request.SendWebRequest();
        
                    if (request.result == UnityWebRequest.Result.Success)
                    {
                        Debug.Log("远程目录文件存在且可访问");
                        descText.text += " catalog can load " + "\n";
                        Debug.Log("catalog can load");
                        _isVerifyRemoteCatalog = true;
                        if (!Directory.Exists(Path.GetDirectoryName(CatalogLocalPath)))
                        {
                            Directory.CreateDirectory(Path.GetDirectoryName(CatalogLocalPath)!);
                        }   
                        File.WriteAllText(CatalogLocalPath, request.downloadHandler.text);
                    }
                    else
                    {
                        Debug.LogError($"无法访问远程目录: {request.error}");
                    }
                }
            }
            else
            {
                Debug.Log("当前已是最新版本");
            }
        }
        else
        {
            Debug.LogError("版本检查失败: " + www.error);
        }
    }

    IEnumerator CheckForCatalogUpdates()
    {
        if (File.Exists(CatalogLocalPath))
        {
            Addressables.ClearResourceLocators();
            yield return null; // 等待一帧以确保清理完成
        
            var handler = Addressables.LoadContentCatalogAsync(CatalogLocalPath);
            yield return handler;
            descText.text += "Update catalog succeeded!" + "\n";
            Debug.Log("Update catalog succeeded!");
            Addressables.Release(handler);
        }
        yield return null; // 等待一帧以确保清理完成
    }

    IEnumerator CheckDownloadSize()
    {
        // 添加重试机制，应对下载失败的情况
        int retryCount = 3;
        AsyncOperationHandle<long> getSizeHandle = Addressables.GetDownloadSizeAsync(hotUpdateLabel);
        yield return getSizeHandle.IsDone;
        
        while (retryCount > 0 && getSizeHandle.Status != AsyncOperationStatus.Succeeded)
        {
            if (getSizeHandle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError($"Check Catalog update failed, remaining retries: {retryCount}, error message: {getSizeHandle.OperationException}");
                descText.text += $"Check Catalog update failed, remaining retries: {retryCount} \n";
                Debug.Log($"Check Catalog update failed, remaining retries: {retryCount}");
                retryCount--;
                if (retryCount > 0)
                {
                    getSizeHandle = Addressables.GetDownloadSizeAsync(hotUpdateLabel);
                }
                else
                {
                    descText.text += "Check download size field!" + "\n";
                    Debug.Log("Check download size field!");
                    Addressables.Release(getSizeHandle);
                    yield break;
                }
            }
        }

        descText.text += "Check download size end! \n";
        Debug.Log("Check download size end!");
        
        // 2. 检查是否有错误
        if (getSizeHandle.Status == AsyncOperationStatus.Succeeded)
        {

            var size = getSizeHandle.Result;
            _hotFixDownloadSize = size;
            descText.text += $"Need download size: {size}" + "\n";
            Debug.Log($"Need download size: {size}");

            if (size == 0)
            {
                Addressables.Release(getSizeHandle);
                yield break;
            }
        }
        
        Addressables.Release(getSizeHandle); // 释放资源
    }


    IEnumerator VersionUpdate()
    {
        descText.text += "start download!!" + "\n";
        Debug.Log("start download!!");
        if (_hotFixDownloadSize == 0)
        {
            descText.text += "size is 0 !!" + "\n";
            Debug.Log("size is 0 !!");
            yield break;
        }
        // 添加重试机制和下载进度显示
        int retryCount = 3;
        // 1. 使用 Addressables 下载 Label 为 "HotUpdate" 的资源
        Debug.Log("Start download HotUpdate resources");
        AsyncOperationHandle downloadHandle = Addressables.DownloadDependenciesAsync(hotUpdateLabel);
        while (retryCount > 0 && downloadHandle.Status != AsyncOperationStatus.Succeeded)
        {
            downloadHandle.Completed += (op) =>
            {
                if (op.Status == AsyncOperationStatus.Succeeded)
                {
                    Debug.Log("Hot update resource download succeeded");
                    descText.text += "Hot update download succeeded" + "\n";
                }
                else
                {
                    Debug.LogError("Hot update resource download failed: " + op.OperationException);
                    descText.text += "Hot update download failed" + "\n";
                }
            };
            yield return downloadHandle;
            if (downloadHandle.Status == AsyncOperationStatus.Succeeded)
            {
                break;
            }
            else
            {
                Debug.LogError($"Hot update resource download failed, remaining retries: {retryCount}, error message: {downloadHandle.OperationException}");
                descText.text += $"Hot update resource download failed, remaining retries: {retryCount}\n";
                retryCount--;
                if (retryCount > 0)
                {
                    downloadHandle = Addressables.DownloadDependenciesAsync(hotUpdateLabel);
                }
                else
                {
                    descText.text += "Hot update download failed\n";
                    Debug.Log("Hot update download failed");
                    Addressables.Release(downloadHandle); // 释放资源
                    yield break;
                }
            }
        }

        // 2. 检查下载结果
        if (downloadHandle.Status == AsyncOperationStatus.Succeeded)
        {
            Debug.Log("热更新资源下载成功");
            descText.text += "fix update download succeeded" + "\n";
        }
        Addressables.Release(downloadHandle); // 释放资源
    }

    private IEnumerator LoadAssemblies()
    {
        if (!enableHybridCLR)
            yield break;
        Debug.Log("LoadAssemblies start!");
        yield return LoadMetadataForAOTAssemblies();
        yield return LoadGamePlayDependencyAssemblies();
        yield return LoadGamePlayAssemblies();
        Debug.Log("LoadAssemblies finish!");
        yield return null;
    }
    
    private IEnumerator StartLoadNewHotFix()
    {
        Debug.Log("Start load HotFixBundleHotUpdateDll");
        var handler = Addressables.LoadAssetAsync<HotfixDllList>(HotfixDllList.Address);
        yield return handler;
        Debug.Log(handler.Status);
        if (handler.Status == AsyncOperationStatus.Succeeded)
        {
            LoadAssembly(handler.Result.list);
        }
    }

    private void LoadAssembly(List<HotfixDllData> resultList)
    {
        foreach (var hotfixDllData in resultList)
        {
            var data = hotfixDllData.dllData;
            var assembly = Assembly.Load(data);
            _allHotUpdateAssemblies.Add(assembly.FullName, assembly);
        }
    }
    //补充元数据
    private IEnumerator LoadMetadataForAOTAssemblies()
    {
        string[] aotAssemblies = null;
        
        var iEnumerator = GetMetaDataDllToLoad(strings =>
        {
            aotAssemblies = strings;
        });
        
        yield return iEnumerator;
        
        if (aotAssemblies == null)
        {
            yield break;
        }
            
        foreach (var aotDllName in aotAssemblies)
        {
            if(string.IsNullOrEmpty(aotDllName))
                continue;
            var path = $"{META_DATA_DLL_PATH}{aotDllName}.bytes";
            iEnumerator = ReadDllBytes(path);
            
            yield return iEnumerator;
            
            if (_dllBytes != null)
            {
                var err = RuntimeApi.LoadMetadataForAOTAssembly(_dllBytes, HomologousImageMode.SuperSet);
                Debug.Log($"LoadMetadataForAOTAssembly:{aotDllName}. ret:{err}");
            }
        }

        Debug.Log("LoadMetadataForAOTAssemblies finish!");
    }
    
    private IEnumerator GetMetaDataDllToLoad(Action<string[]> callback)
    {
        string[] result = null;
        var operation = Addressables.LoadAssetAsync<TextAsset>(META_DATA_DLLS_TO_LOAD_PATH);
        yield return operation;

        if (operation.Status == AsyncOperationStatus.Succeeded)
        {
            var text = operation.Result.text;
            Debug.Log($"load metaDataText");
            result = text.Split('|');
        }
        else
        {
            Debug.LogError($"cant load metaDataText, path:{META_DATA_DLLS_TO_LOAD_PATH}");
        }

        Addressables.Release(operation);

        // 调用回调，返回数据
        callback?.Invoke(result);
    }
    
    private IEnumerator ReadDllBytes(string path)
    {
        var operation = Addressables.LoadAssetAsync<TextAsset>(path);
        yield return operation;

        byte[] dllBytes = null;
        if (operation.Status == AsyncOperationStatus.Succeeded)
        {
            _dllBytes = operation.Result.bytes;
            Debug.Log($"load dllText, path:{path}");
        }
        else
        {
            Debug.LogError($"cant load dllText, path:{path}");
        }
        Addressables.Release(operation);
    }
    
    
    //加载GamePlay依赖的第三方程序集
    private IEnumerator LoadGamePlayDependencyAssemblies()
    {
        foreach (var dllName in _gamePlayDependencyDlls)
        {
            yield return LoadDependencyAssembly(dllName);
        }

        Debug.Log("LoadGamePlayDependencyAssemblies finish!");
    }
    
    private IEnumerator LoadDependencyAssembly(string dllName)
    {
        var path = $"{HOT_UPDATE_DLL_PATH}{dllName}.bytes";
        yield return ReadDllBytes(path);
        if (_dllBytes != null)
        {
            var assembly = Assembly.Load(_dllBytes);
            _allHotUpdateAssemblies.Add(assembly.FullName, assembly);
            Debug.Log($"Load Assembly success,assembly Name:{assembly.FullName}");
        }
        yield return null;
    }
    
    private IEnumerator LoadHotUpdateAssembly()
    {
        var path = $"{HOT_UPDATE_DLL_PATH}{HotUpdateDll}.bytes";
        yield return ReadDllBytes(path);
        if (_dllBytes != null)
        {
            var assembly = Assembly.Load(_dllBytes);
            hotfixAsm = assembly;
            _allHotUpdateAssemblies.Add(assembly.FullName, assembly);
            Debug.Log($"Load Assembly success,assembly Name:{assembly.FullName}");
        }
        yield return null;
    }
    
    //加载HotUpdate程序集
    private IEnumerator LoadGamePlayAssemblies()
    {
        var newDllCheck = Addressables.LoadResourceLocationsAsync(HotfixDllList.Address);
        yield return newDllCheck;
        if (newDllCheck.Status == AsyncOperationStatus.Succeeded && newDllCheck.Result.Count > 0)
        {
            yield return StartLoadNewHotFix();
            Debug.Log("Load downloaded HotFixBundleHotUpdateDll");
        }
        else
        {
            yield return LoadHotUpdateAssembly();
            Debug.Log("Load HotUpdateDll from local");
        }
        Debug.Log("LoadHotUpdateAssemblies finish!");
    }
    
    private IEnumerator StartGame()
    {
        string typeName = "Game";
        string methodName = "Start";
        Debug.Log("GetAssemblies HotfixAsm");
        // 获取当前执行的程序集
        Assembly assembly = hotfixAsm == null? AppDomain.CurrentDomain.GetAssemblies()
            .FirstOrDefault(assembly => assembly.GetName().Name == "HotfixAsm"): hotfixAsm;
        
        // 获取类型
        Type type = assembly.GetType(typeName);
        if (type == null)
        {
            Debug.LogError($"Type {typeName} not found");
            yield break;
        }

        Debug.Log("获取到类型");
        // 创建实例 (如果不是静态方法)
        object instance = Activator.CreateInstance(type);
        Debug.Log("创建Game实例");


        // 获取方法
        MethodInfo methodInfo = type.GetMethod(methodName);
        Debug.Log("获取Start方法");

        // 调用方法
        Action<float> action = OnProcess;
        methodInfo.Invoke(instance, new object[] { action });
        yield return methodInfo;
        Debug.Log("EnterGame finish!");
    }
    
    
    
    public void OnProcess(float pro)
    {
        progress.fillAmount = pro;
        progressText.text = (pro*100) + "%";
    }
}
