using System;
using System.Collections.Generic;
using System.Globalization;

namespace AppBase.Tools
{
    public static class StringExtension
    {
        private static bool SetValueListDataByString<T>(string strContent,ref List<T> targetList,char splitChar = ',')
        {
            bool anyNoSupportType = false;
            List<string> strList = strContent.ToListString(splitChar);
            if (strList != null && strList.Count > 0)
            {
                for (int i = 0; i < strList.Count; i++)
                {
                    //分类型处理string
                    if (typeof(T) == typeof(string))
                    {
                        targetList.Add((T) (object) strList[i]);
                    }
                    //int
                    else if (typeof(T) == typeof(int))
                    {
                        targetList.Add((T) (object) int.Parse(strList[i]));
                    }
                    //float
                    else if (typeof(T) == typeof(float))
                    {
                        targetList.Add((T) (object) float.Parse(strList[i],CultureInfo.InvariantCulture));
                    }
                    //long
                    else if (typeof(T) == typeof(long))
                    {
                        targetList.Add((T) (object) long.Parse(strList[i]));
                    }
                    //double
                    else if (typeof(T) == typeof(double))
                    {
                        targetList.Add((T) (object) double.Parse(strList[i],CultureInfo.InvariantCulture));
                    }
                    //other 暂不支持
                    else
                    {
                        anyNoSupportType = true;
                    }
                }
            }

            return anyNoSupportType;
        }
       
        public static List<int> ToListInt(this string str,char splitChar = ':')
        {
            var arr = str.Split(splitChar, StringSplitOptions.RemoveEmptyEntries);
            if (arr.Length > 0)
            {
                var rst = new List<int>(arr.Length);
                for (int i = 0; i < arr.Length; i++)
                {
                    int id = -1;
                    int.TryParse(arr[i], out id);
                    rst.Add(id);
                }

                return rst;

            }

            return new List<int>(0);
        }
        
        public static List<float> ToListFloat(this string str,char splitChar = ',')
        {
            var arr = str.Split(splitChar, StringSplitOptions.RemoveEmptyEntries);
            if (arr.Length > 0)
            {
                var rst = new List<float>(arr.Length);
                for (int i = 0; i < arr.Length; i++)
                {
                    float id = -1;
                    float.TryParse(arr[i], out id);
                    rst.Add(id);
                }

                return rst;

            }

            return new List<float>(0);
        }
        
        public static List<string> ToListString(this string str,char splitChar = ',')
        {
            var arr = str.Split(splitChar, StringSplitOptions.RemoveEmptyEntries);
            if (arr.Length > 0)
            {
                var rst = new List<string>(arr.Length);
                for (int i = 0; i < arr.Length; i++)
                {
                    rst.Add(arr[i]);
                }
                return rst;

            }
            return new List<string>(0);
        }
        
        public static List<List<int>> ToIntListList(this string str, char splitCharD1 = ',', char splitCharD2 = ':')
        {
            List<List<int>> result = new List<List<int>>();
            List<int> list;
            var listStr = str.ToListString(splitCharD1);
            for (int i = 0; i < listStr.Count; i++)
            {
                list = new List<int>();
                var arr = listStr[i].Split(splitCharD2, StringSplitOptions.RemoveEmptyEntries);
                object a = 3;
                if (arr.Length > 1)
                {
                    list.Add(int.Parse(arr[0]));
                    list.Add(int.Parse(arr[1]));
                    result.Add(list);
                }
            }
            return result;
        }
        
        public static Dictionary<int,int> ToIntDictionary(this string str, char splitCharD1 = ',', char splitCharD2 = ':')
        {
            Dictionary<int, int> rst = new();
            var listStr = str.ToListString(splitCharD1);
            for (int i = 0; i < listStr.Count; i++)
            {
                var arr = listStr[i].Split(splitCharD2, StringSplitOptions.RemoveEmptyEntries);
                object a = 3;
                if (arr.Length > 1)
                {
                    rst.Add(int.Parse(arr[0]), int.Parse(arr[1]));
                }
            }
            return rst;
        }
    }
}