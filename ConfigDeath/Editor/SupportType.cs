using System;
using System.Collections.Generic;

namespace AppBase.ConfigDeath
{
	//属性类型
	public static class SupportType
	{
		public const string Int = "int";
		public const string Float = "float";
		public const string String = "string";
		public const string IntArray = "int[]";
		public const string FloatArray = "float[]";
		public const string StringArray = "string[]";

		public static Type GetTypeByString(string type)
		{
			switch (type.ToLower())
			{
				case Float:
					return typeof(float);
				case Int:
					return typeof(int);
				case String:
					return typeof(string);
				case IntArray:
					return typeof(List<int>);
				case FloatArray:
					return typeof(List<float>);
				default:
					return Type.GetType(type, true, true);
			}
		}
	}
}
