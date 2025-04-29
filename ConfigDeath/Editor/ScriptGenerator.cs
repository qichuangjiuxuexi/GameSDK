using System.IO;
using AppBase.Tools;
using ConfigDeath;
using UnityEditor;
using UnityEngine;

namespace AppBase.ConfigDeath
{
	//代码生成器
	public class ScriptGenerator
	{
		
		private string[] Nodes;
		private string[] Types;
		private string[] Names;
		private string SheetName;
		private string InputPath;
		
		public ScriptGenerator(string inputPath, string sheetName, string[] nodes, string[] names, string[] types)
		{
			Nodes = nodes;
			InputPath = inputPath;
			SheetName = sheetName;
			Names = names;
			Types = types;
		}

		public void GeneratorCode()
		{
			var path = Path.Combine(ConfigUtil.DataTempPath, SheetName + ".cs");
			FileTools.SafeDeleteFile(path);
			AssetDatabase.Refresh();

			using (StreamWriter writer = new StreamWriter(path))
			{
				writer.WriteLine("//Auto create by Framework");
				writer.WriteLine();
				writer.WriteLine("using System;");
				writer.WriteLine("using System.Collections.Generic;");
				writer.WriteLine("using UnityEngine.Scripting;");
				writer.WriteLine();
				writer.WriteLine();
				writer.WriteLine("[Serializable]");
				writer.WriteLine("public class " +SheetName+" : BaseConfig");
				writer.WriteLine("{");
				for (int i = 0; i < Types.Length; i++)
				{
					var type = Types[i];
					var name = Names[i];
					var note = Nodes[i];
					if (string.IsNullOrEmpty(type) || string.IsNullOrEmpty(name))
					{
						continue;
					}
					writer.WriteLine("    //"+note);
					writer.WriteLine("    public " + GetTrueType(type) + " " + name+";");
					writer.WriteLine();
				}
				
				writer.WriteLine("}");
			}

			GeneratorCodeList();
		}

		//创建list代码
		private void GeneratorCodeList()
		{
			var className = SheetName + "List.cs";
			var path = Path.Combine(ConfigUtil.DataTempPath, className);
			FileTools.SafeDeleteFile(path);
			using (StreamWriter writer = new StreamWriter(path))
			{
				writer.WriteLine("//Auto create by Framework");
				writer.WriteLine();
				writer.WriteLine("using System;");
				writer.WriteLine();
				writer.WriteLine();
				
				writer.WriteLine("[Serializable]");
				writer.WriteLine("public class " +className.Split('.')[0]+" : BaseConfigList<"+SheetName+">");
				writer.WriteLine("{");
				writer.WriteLine("}");
			}
		}

		private string GetTrueType(string type)
        {
            switch (type)
            {
                case SupportType.Int:
                    type = "int";
                    break;
                case SupportType.Float:
                    type = "float";
                    break;
                case SupportType.String:
                    type = "string";
                    break;
                case SupportType.IntArray:
                    type = "List<int>";
                    break;
                case SupportType.FloatArray:
                    type = "List<float>";
                    break;
                case SupportType.StringArray:
                    type = "List<string>";
                    break;
                default:
                    Debug.LogError("输入了错误的数据类型:  " + type + ", 类名:  " + SheetName + ", 位于:  " + InputPath);
                    break;
            }

            return type;
        }
		
	}
}
