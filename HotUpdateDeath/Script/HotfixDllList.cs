using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 热更新程序集名字列表
/// </summary>
public class HotfixDllList : ScriptableObject
{
    public const string Address = "HotfixDllList";
    public const string Path = "Assets/Project/" + Address + ".asset";
    public List<HotfixDllData> list = new();
}

[Serializable]
public class HotfixDllData
{
    public string name;
    public byte[] dllData; // 程序集原始数据
}