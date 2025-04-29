using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 配置基类
/// </summary>
public abstract class BaseConfig
{
}

/// <summary>
/// 配置列表基类
/// </summary>
public interface IConfigList
{
    IList values { get; set; }
}
    
/// <summary>
/// 配置列表基类（泛型）
/// </summary>
public interface IConfigList<T> : IConfigList
{
    new List<T> values { get; }
}


