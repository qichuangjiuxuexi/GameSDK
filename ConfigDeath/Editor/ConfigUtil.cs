using System;
using UnityEditor;
using UnityEngine;
using Excel;
using AppBase.ConfigDeath;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using AppBase.Resource;
using AppBase.Tools;

//数据表内每一格数据
internal class ConfigData
{
    public string Notes; //注释
    public string Type; //数据类型
    public string Name; //字段名
    public string Data; //数据值
}

namespace ConfigDeath
{
    public class ConfigUtil : ScriptableObject
    {

        public static string projectRoot = Directory.GetParent(Application.dataPath)?.FullName; //项目根目录

        public static string
            ExcelPath = Path.Combine(Directory.GetParent(projectRoot).FullName, "Config"); //存放excel表格的目录

        public const string ConfigsPath = "Assets/Project/Feature/Address/m_Configs"; //config组
        public const string DataTargetPath = "Assets/Project/Feature/Address/m_Configs/Data_dl"; //输出数据的目录
        public const string CodeAssembly = "HotfixAsm"; //由表生成的数据类型均在此命名空间内
        public const string DataTempPath = "Assets/Project/Feature/Address/m_Configs/DataTemp"; //创建代码类目录


        private static Dictionary<string, ScriptGenerator> codeList; //存放所有生成的类的代码

        private static Dictionary<string, List<ConfigData[]>> dataDict; //存放所有数据表内的数据，key：类名  value：数据

        [MenuItem("Tools/Config/1.更新数据脚本")]
        public static void LoadAllExcelData()
        {
            codeList = new Dictionary<string, ScriptGenerator>();
            dataDict = new Dictionary<string, List<ConfigData[]>>();
            //创建文件夹
            FileTools.CheckCreatFile(ExcelPath);
            //先删除Data数据
            FileTools.SafeDeleteAllFiles(DataTargetPath);
            IEnumerable<string> allExcel = FileTools.GetAllFiles(ExcelPath)
                .Where(file => file.EndsWith(".xls") || file.EndsWith(".xlsx"));
            foreach (string inputPath in allExcel)
            {
                GetExcelData(inputPath);
            }
            //生成数据模板代码
            GeneratorCode();
            AssetDatabase.Refresh();
        }

        //读取excel数据
        private static void GetExcelData(string inputPath, bool ignoreCreateCode = false)
        {
            FileStream stream = null;
            try
            {
                stream = File.Open(inputPath, FileMode.Open, FileAccess.Read);
            }
            catch
            {
                EditorUtility.DisplayDialog("注意！！！", "\n请关闭 " + inputPath + " 后再导表！", "确定");
                Debug.LogError("请关闭 " + inputPath + " 后再导表！");
                return;
            }

            IExcelDataReader excelReader = null;
            if (inputPath.EndsWith(".xls"))
            {
                excelReader = ExcelReaderFactory.CreateBinaryReader(stream);
            }
            else if (inputPath.EndsWith(".xlsx"))
            {
                excelReader = ExcelReaderFactory.CreateOpenXmlReader(stream);
            }

            if (excelReader is null or { IsValid: false })
            {
                Debug.LogError("无法读取的文件:  " + inputPath);
                return;
            }

            // 读取所有的sheet
            do
            {
                // sheet名字
                string sheetName = excelReader.Name;
                //一些临时数据
                string[] notes = null;
                string[] types = null;
                string[] names = null;
                List<ConfigData[]> dataList = new List<ConfigData[]>();
                int index = 0;

                //开始读取
                while (excelReader.Read())
                {
                    //这里读取的是每一行的数据
                    string[] datas = new string[excelReader.FieldCount];

                    for (int i = 0; i < excelReader.FieldCount; i++)
                    {
                        datas[i] = excelReader.GetString(i);
                    }

                    //空行不处理
                    if (datas.Length == 0 || string.IsNullOrEmpty(datas[0]))
                    {
                        ++index;
                        continue;
                    }

                    //第0行注释
                    if (index == 0)
                    {
                        notes = datas;
                    }
                    //第1行表示类型
                    else if (index == 1)
                    {
                        types = datas;
                    }
                    //第2行表示变量名
                    else if (index == 2)
                    {
                        names = datas;
                    }
                    else if (index > 2)
                    {
                        if (types == null || names == null || datas == null || notes == null)
                        {
                            Debug.LogError("数据错误！[" + sheetName + "]配置表！第" + index + "行" + inputPath);
                            continue;
                        }

                        List<ConfigData> configDataList = new List<ConfigData>();
                        for (int i = 0; i < datas.Length; i++)
                        {
                            ConfigData data = new ConfigData();
                            data.Notes = notes[i];
                            data.Type = types[i];
                            data.Name = names[i];
                            data.Data = datas[i];
                            if (string.IsNullOrEmpty(data.Type) || string.IsNullOrEmpty(data.Data))
                                continue; //空的数据不处理
                            configDataList.Add(data);
                        }

                        dataList.Add(configDataList.ToArray());
                    }

                    index++;
                }

                if (string.IsNullOrEmpty(sheetName))
                {
                    Debug.LogError("空的类名（excel页签名）, 路径:  " + inputPath);
                }

                //不创建代码则直接走
                if (ignoreCreateCode)
                {
                    dataDict.Add(sheetName, dataList);
                    continue;
                }

                if (names != null && types != null)
                {
                    //根据刚才的数据来生成C#脚本
                    ScriptGenerator generator = new ScriptGenerator(inputPath, sheetName, notes, names, types);
                    //所有生成的类的代码最终保存在这
                    if (codeList.ContainsKey(sheetName))
                    {
                        Debug.LogError("类名重复: " + sheetName + " ,路径:  " + inputPath);
                        continue;
                    }

                    codeList.Add(sheetName, generator);
                    dataDict.Add(sheetName, dataList);
                }
            } while (excelReader.NextResult());
        }

        //生成数据类代码
        private static void GeneratorCode()
        {
            foreach (var keyValuePair in codeList)
            {
                keyValuePair.Value.GeneratorCode();
            }
        }
        
        
        [MenuItem("Tools/Config/2.更新数据")]
        private static void AllScriptsReloaded()
        {
            FileTools.CheckCreatFile(DataTargetPath);
            //重新生成一下
            if (codeList == null)
            {
                codeList = new Dictionary<string, ScriptGenerator>();
            }
            else
            {
                codeList.Clear();
            }

            if (dataDict == null)
            {
                dataDict = new Dictionary<string, List<ConfigData[]>>();
            }
            else
            {
                dataDict.Clear();
            }

            var filse = FileTools.GetAllFiles(ExcelPath).Where(file => file.EndsWith(".xls") || file.EndsWith(".xlsx"))
                .ToArray();
            for (var i = 0; i < filse.Length; i++)
            {
                GetExcelData(filse[i], true);
            }

            // 获取当前执行的程序集
            Assembly hoxFixAsm = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(assembly => assembly.GetName().Name == CodeAssembly);
                
            foreach (KeyValuePair<string, List<ConfigData[]>> each in dataDict)
            {
                
                Type objType = hoxFixAsm.GetType(each.Key);
                Type listType =  hoxFixAsm.GetType(each.Key + "List");
                object listObj = Activator.CreateInstance(listType);
                
                // 创建一个新的List
                MethodInfo addMethod =  listType.GetMethod("Add", 
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, 
                    null, new Type[] { objType }, null);
                foreach (ConfigData[] configDatas in each.Value)
                {
                    object numb = Activator.CreateInstance(objType);
                    foreach (ConfigData configData in configDatas)
                    {
                        FieldInfo fieldInfo = objType.GetField(configData.Name);
                        var data = ChangeValueType(configData.Data, configData.Type);
                        fieldInfo.SetValue(numb, data);
                    }
                
                    addMethod.Invoke(listObj, new[] { numb });
                }
                string filePath = Path.Combine(DataTargetPath, each.Key + ".json");
                string str = JsonUtility.ToJson(listObj);
                File.WriteAllText(filePath, str);
                Debug.Log($"已序列化 {DataTargetPath}/each.Key");
            }
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            //更新Addressable
            AddressAbleUtil.UpdateAddressAble(ConfigsPath);
        }

        private static object ChangeValueType(object data, string type)
        {
            string str = "";
            switch (type)
            {
                case SupportType.IntArray:
                    str = data.ToString();
                    List<int> listInt = str.ToListInt(';');
                    return Convert.ChangeType(listInt, SupportType.GetTypeByString(type));
                case SupportType.FloatArray:
                    str = data.ToString();
                    List<float> listFloat = str.ToListFloat(';');
                    return Convert.ChangeType(listFloat, SupportType.GetTypeByString(type));
                default:
                    return Convert.ChangeType(data, SupportType.GetTypeByString(type));
            }
        }
    }
}