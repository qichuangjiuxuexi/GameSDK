using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using Newtonsoft.Json;
using UnityEngine;


/// <summary>
/// 配置列表
/// </summary>
[Serializable]
public class BaseConfigList<T> : IConfigList<T>, IList<T>, IReadOnlyList<T>, IList
    where T : BaseConfig
{
    /// <summary>
    /// 数据列表
    /// </summary>
    public List<T> values = new();

    #region IConfigList接口

    public IEnumerator<T> GetEnumerator() => values.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => values.GetEnumerator();
    public void Add(T item) => values.Add(item);
    public int Add(object value) => ((IList)values).Add(value);
    public void Clear() => values.Clear();
    public bool Contains(object value) => ((IList)values).Contains(value);
    public int IndexOf(object value) => ((IList)values).IndexOf(value);
    public void Insert(int index, object value) => ((IList)values).Insert(index, value);
    public void Remove(object value) => ((IList)values).Remove(value);
    public bool Contains(T item) => values.Contains(item);
    public void CopyTo(T[] array, int arrayIndex) => values.CopyTo(array, arrayIndex);
    public bool Remove(T item) => values.Remove(item);
    public void CopyTo(Array array, int index) => ((IList)values).CopyTo(array, index);
    [JsonIgnore, XmlIgnore] public int Count => values.Count;
    [JsonIgnore, XmlIgnore] public bool IsSynchronized => ((IList)values).IsSynchronized;
    [JsonIgnore, XmlIgnore] public object SyncRoot => ((IList)values).SyncRoot;
    [JsonIgnore, XmlIgnore] public bool IsReadOnly => ((ICollection<T>)values).IsReadOnly;
    public int IndexOf(T item) => values.IndexOf(item);
    public void Insert(int index, T item) => values.Insert(index, item);
    public void RemoveAt(int index) => values.RemoveAt(index);
    [JsonIgnore, XmlIgnore] public bool IsFixedSize => ((IList)values).IsFixedSize;

    public T this[int index]
    {
        get => values[index];
        set => values[index] = value;
    }

    object IList.this[int index]
    {
        get => ((IList)values)[index];
        set => ((IList)values)[index] = value;
    }

    [JsonIgnore, XmlIgnore]
    IList IConfigList.values
    {
        get => values;
        set => values = (List<T>)value;
    }

    [JsonIgnore, XmlIgnore] List<T> IConfigList<T>.values => values;
    public static implicit operator List<T>(BaseConfigList<T> listObject) => listObject.values;

    #endregion
}