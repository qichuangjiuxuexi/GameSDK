using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

namespace AppBase.Tools
{
    public class FileTools
    {
        /// <summary>
        /// 将目录中的\转换为/
        /// </summary>
        /// <param name="path"></param>
        public static string ExchangeRealPath(string path)
        {
            return path.Replace("\\", "/");
        }


        /// <summary>
        /// 检测是否需要创建一个文件夹
        /// </summary>
        /// <param name="path"></param>
        public static void CheckCreatFile(string path)
        {
            if (string.IsNullOrEmpty(path)) return;
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        /// <summary>
        /// 获取这个路径里的所有文件夹
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static List<string> GetAllDirectories(string path)
        {
            // 确保路径是目录并且存在
            if (!Directory.Exists(path))
            {
                Debug.LogError("地址：" + path + "不存在");
            }

            // 获取所有子目录的完整路径
            List<string> directories = Directory.GetDirectories(path, "*", SearchOption.AllDirectories).ToList();
            for (int i = 0; i < directories.Count; i++)
            {
                directories[i] = ExchangeRealPath(directories[i]);
            }

            return directories;
        }

        /// <summary>
        /// 获取这个路径里的所有文件
        /// </summary>
        /// <param name="inputPath"></param>
        /// <returns></returns>
        public static List<string> GetAllFiles(string inputPath)
        {
            // 确保路径是目录并且存在
            if (!Directory.Exists(inputPath))
            {
                Debug.LogError("地址：" + inputPath + "不存在");
            }

            List<string> directories = Directory.GetFiles(inputPath).ToList();
            for (int i = 0; i < directories.Count; i++)
            {
                directories[i] = ExchangeRealPath(directories[i]);
            }

            var filepaths = GetAllDirectories(inputPath);
            foreach (var pFilepath in filepaths)
            {
                var files = GetAllFiles(pFilepath);
                foreach (var file in files)
                {
                    directories.Add(file);
                }
            }

            return directories;
        }

        /// <summary>
        /// 获取路径中的 文件名字
        /// </summary>
        /// <param name="fullPath"></param>
        /// /// <param name="ignoreType"></param>
        /// <returns></returns>
        public static string GetFileName(string fullPath, bool ignoreType = false)
        {
            if (ignoreType)
            {
                return Path.GetFileNameWithoutExtension(fullPath);
            }
            else
            {
                return Path.GetFileName(fullPath);
            }
        }

        /// <summary>
        /// 通过地址获取 文件类型
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static string GetFileType(string filePath)
        {
            string fileName = GetFileName(filePath);
            var strs = fileName.Split('.');
            return strs[^1];
        }


        /// <summary>
        /// 安全删除目录
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static bool SafeDeleteFile(string filePath)
        {
            try
            {
                if (string.IsNullOrEmpty(filePath))
                {
                    return true;
                }

                if (!File.Exists(filePath))
                {
                    return true;
                }

                File.SetAttributes(filePath, FileAttributes.Normal);
                File.Delete(filePath);
                return true;
            }
            catch (System.Exception ex)
            {
                Debug.LogError(string.Format("SafeDeleteFile failed! path = {0} with err: {1}", filePath, ex.Message));
                return false;
            }
        }

        public static bool SafeDeleteAllFiles(string filePath)
        {
            try
            {
                if (string.IsNullOrEmpty(filePath))
                {
                    return true;
                }

                if (!Directory.Exists(filePath))
                {
                    return true;
                }

                // 获取文件夹中的所有文件
                string[] files = Directory.GetFiles(filePath);

                foreach (string file in files)
                {
                    // 删除文件
                    File.Delete(file);
                }

                return true;
            }
            catch (System.Exception ex)
            {
                Debug.LogError(string.Format("SafeDeleteFile failed! path = {0} with err: {1}", filePath, ex.Message));
                return false;
            }
        }
        
        
        //磁盘路径转 Assets 路径 
        public static string DiskPathToAssetPath(string diskPath)
        {
            // 标准化路径分隔符
            diskPath = diskPath.Replace('\\', '/');
    
            // 获取项目路径
            string projectPath = Application.dataPath;
            // 移除 Assets 文件夹
            projectPath = projectPath.Substring(0, projectPath.Length - 6);
    
            // 如果磁盘路径包含项目路径，转换为相对路径
            if (diskPath.StartsWith(projectPath))
            {
                return diskPath.Substring(projectPath.Length);
            }
    
            return null;
        }
    }
}