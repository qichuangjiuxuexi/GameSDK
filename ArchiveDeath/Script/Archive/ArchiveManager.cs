using System;
using System.Collections.Generic;
using AppBase.Module;
using Newtonsoft.Json;
using UnityEngine;

namespace AppBase.ArchiveDeath
{
    public class ArchiveManager : ModuleBase
    {
        private const string datExtName = ".json";
        public const string datDirPath = "ArchiveData/";

        private Dictionary<string, BaseArchiveData> archiveDict = new();
        public T GetArchiveData<T>(string archiveName) where T : BaseArchiveData, new()
        {
            if (archiveDict.TryGetValue(archiveName, out var value))
            {
                return (T)value;
            }

            value = ReadArchive<T>(archiveName);
            if (value != null)
            {
                archiveDict.Add(archiveName, value);
                return (T)value;
            }
            return null;
        }
        
        /// <summary>
        /// 只是更新数据，不存储
        /// </summary>
        /// <param name="archiveName"></param>
        /// <param name="data"></param>
        public void UpdateArchiveData(string archiveName, BaseArchiveData data)
        {
            if (string.IsNullOrEmpty(archiveName) || data == null) return;
            archiveDict[archiveName] = data;
        }
        
        private T ReadArchive<T>(string archiveName) where T : BaseArchiveData
        {
            try
            {
                var json = ReadFromEs3(archiveName);
                if (!string.IsNullOrEmpty(json))
                {
                    return JsonConvert.DeserializeObject<T>(json);
                }
                return null;
            }
            catch (Exception e)
            {
                Debug.LogError(TAG+" msg: "+ e.Message);
                return null;
            }
        }

        private string ReadFromEs3(string archiveName, string defaultContent = null)
        {
            var path = datDirPath + archiveName + datExtName;
            if (!ES3.FileExists(path) && !ES3.RestoreBackup(path))
            {
                return defaultContent;
            }
            
            string ReadFile(string p)
            {
                try
                {
                    return ES3.LoadRawString(p);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }

            string result = ReadFile(path);
            if (string.IsNullOrEmpty(result) && ES3.RestoreBackup(path))
            {
                result = ReadFile(path);
            }

            return string.IsNullOrEmpty(result) ? defaultContent : result;

        }

        public void Save(string archiveName)
        {
            if (archiveDict.TryGetValue(archiveName, out var data))
            {
                string json = JsonConvert.SerializeObject(data);
                WriteToES3(archiveName, json);
                archiveDict[archiveName] = data;
            }
        }

        private void WriteToES3(string archiveName, string content)
        {
            var path = datDirPath + archiveName + datExtName;
            ES3.CreateBackup(path);
            ES3.SaveRaw(content, path);
        }
    }
}
