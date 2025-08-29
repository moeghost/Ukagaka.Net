using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading.Tasks;

namespace Cocoa.AppKit
{
    public class NSBundle
    {
        private static readonly Dictionary<string, Dictionary<string, object>> _localizations = new Dictionary<string, Dictionary<string, object>>();
        private static CultureInfo _currentCulture = CultureInfo.CurrentUICulture;



        public NSBundle()
        {
            // 假设资源文件在 YourApp.Resources.Localizable.resx 中
            LoadAllLocalizations();
        }

        private static NSBundle mainBundle;
        internal static NSBundle MainBundle()
        {
            if (mainBundle == null)
            {
                mainBundle = new NSBundle();
            }
           
            return mainBundle;
        }

     

        public static CultureInfo CurrentCulture
        {
            get => _currentCulture;
            set
            {
                _currentCulture = value;
                CultureInfo.CurrentUICulture = value;
            }
        }

        /// <summary>
        /// 获取本地化字符串（兼容NSBundle API）
        /// </summary>
        /// <param name="key">键名（支持点分隔符如"BUTTONS.SAVE"）</param>
        /// <param name="defaultValue">默认值</param>
        /// <param name="tableName">表名（对应文件名）</param>
        public static string LocalizedString(string key, string defaultValue = null, string tableName = "Localizable")
        {
            try
            {
                string cultureName = _currentCulture.Name.ToLower();
                if (_localizations.TryGetValue(cultureName, out var tables))
                {

                    if (tables.TryGetValue(tableName, out var tableData))
                    {
                         
                        // 支持嵌套键查询
                        object result = GetNestedValue(tableData, key);
                        return result?.ToString() ?? defaultValue ?? key;
                    }
                }
            }
            catch
            {
                // 忽略错误
            }
            return defaultValue ?? key;
        }

        /// <summary>
        /// 获取本地化对象（支持复杂类型）
        /// </summary>
        public static object LocalizedObject(string key, string tableName = "Localizable")
        {
            try
            {
                string cultureName = _currentCulture.Name.ToLower();
                if (_localizations.TryGetValue(cultureName, out var tables) &&
                    tables.TryGetValue(tableName, out var tableData))
                {
                    return GetNestedValue(tableData, key);
                }
            }
            catch
            {
                // 忽略错误
            }
            return null;
        }

        private static object GetNestedValue(object data, string pathParts)
        {
           
            if (data is Dictionary<string, object> dict)
            {
                if (dict.TryGetValue(pathParts, out var nestedValue))
                {
                    return GetNestedValue(nestedValue, pathParts);
                }
            }
            else if (data is JObject jObject)
            {
                var token = jObject.SelectToken(pathParts);
                if (token != null)
                {
                    return GetNestedValue(token, pathParts);
                }
            }
            else if (data is string)
            {
                return data;

            }


                return null;
        }

        private static object GetNestedValue(object data, string[] pathParts, int index = 0)
        {
            if (index >= pathParts.Length) return data;

            if (data is Dictionary<string, object> dict)
            {
                if (dict.TryGetValue(pathParts[index], out var nestedValue))
                {
                    return GetNestedValue(nestedValue, pathParts, index + 1);
                }
            }
            else if (data is JObject jObject)
            {
                var token = jObject.SelectToken(pathParts[index]);
                if (token != null)
                {
                    return GetNestedValue(token, pathParts, index + 1);
                }
            }

            return null;
        }

        private static void LoadAllLocalizations()
        {
            string basePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources/Localizable");
            if (!Directory.Exists(basePath))
            {
                return;
            }
            foreach (var cultureDir in Directory.GetDirectories(basePath))
            {
                string cultureName = Path.GetFileName(cultureDir).ToLower();
                var cultureData = new Dictionary<string, object>();

                foreach (var file in Directory.GetFiles(cultureDir, "*.strings"))
                {
                    string tableName = Path.GetFileNameWithoutExtension(file);
                    cultureData[tableName] = ParseStringsFile(file);
                }

                _localizations[cultureName] = cultureData;
            }
        }

        private static Dictionary<string, object> ParseStringsFile(string filePath)
        {
            var result = new Dictionary<string, object>();
            var currentDict = result;
            var dictStack = new Stack<Dictionary<string, object>>();

            foreach (var line in File.ReadLines(filePath))
            {
                string trimmedLine = line.Trim();
                if (string.IsNullOrWhiteSpace(trimmedLine)) continue;

                // 处理嵌套开始 { 
                if (trimmedLine.EndsWith("{"))
                {
                    string key = trimmedLine.Split('=')[0].Trim().Trim('"');
                    var newDict = new Dictionary<string, object>();
                    currentDict[key] = newDict;
                    dictStack.Push(currentDict);
                    currentDict = newDict;
                    continue;
                }

                // 处理嵌套结束 }
                if (trimmedLine == "}")
                {
                    if (dictStack.Count > 0)
                    {
                        currentDict = dictStack.Pop();
                    }
                    continue;
                }

                // 处理键值对
                if (trimmedLine.Contains('=') && !trimmedLine.StartsWith("//"))
                {
                    var parts = line.Split(new[] { '=' }, 2);
                    string key = parts[0].Trim().Trim('"');
                    string value = parts[1].Trim()
                        .TrimEnd(';')
                        .Trim()
                        .Trim('"')
                        .Replace("%@", "{0}");

                    // 尝试解析JSON值（支持复杂对象）
                    if (value.StartsWith("{") || value.StartsWith("["))
                    {
                        try
                        {
                            currentDict[key] = JsonConvert.DeserializeObject<object>(value);
                            continue;
                        }
                        catch { /* 不是JSON，按普通字符串处理 */ }
                    }

                    currentDict[key] = value;
                }
            }

            return result;
        }

        // 热重载
        public static void Reload() => LoadAllLocalizations();


        internal string LocalizedStringForKey(string key, string value, string table)
        {
             
            
            return LocalizedString(key,value,table);

           // throw new NotImplementedException();
        }
    }
}
