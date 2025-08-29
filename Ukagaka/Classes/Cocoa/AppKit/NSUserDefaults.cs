using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using System.IO;
using System.Windows.Input;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;

namespace Cocoa.AppKit
{
    public class NSUserDefaults
    {
        private  readonly Dictionary<string, object> _defaults = new Dictionary<string, object>();
        private  string _storagePath;
        private  readonly object _syncLock = new object();

        static NSUserDefaults instances;

        public static NSUserDefaults StandardUserDefaults()
        {
            NSUserDefaults instances = new NSUserDefaults();

            // Store settings in AppData/Local/[CompanyName]/[ProductName]

            instances._storagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "user");

            instances.LoadDefaults();

            return instances;
        }





        internal NSMutableDictionary DictionaryForKey(string key)
        {
            lock (_syncLock)
            {
                if (!_defaults.ContainsKey(key))
                {
                    return null;
                }

                if (_defaults[key] is NSMutableDictionary)
                {
                    return _defaults[key] as NSMutableDictionary;
                }


                return null;
            }
        }


        public void SetDictionaryForKey(NSMutableDictionary value, string key)
        {
            lock (_syncLock)
            {
                _defaults[key] = value;
                SaveDefaults();
            }
        }

        public void SetObjectForKey(object value, string key)
        {
            lock (_syncLock)
            {
                _defaults[key] = value;
                SaveDefaults();
            }
        }

        public  object ObjectForKey(string key)
        {
            lock (_syncLock)
            {
                return _defaults.TryGetValue(key, out var value) ? value : null;
            }
        }

        public void RemoveObjectForKey(string key)
        {
            lock (_syncLock)
            {
                if (_defaults.ContainsKey(key))
                {
                    _defaults.Remove(key);
                    SaveDefaults();
                }
            }
        }

        public void Synchronize()
        {
            // In this implementation, changes are saved immediately
            // so this method doesn't need to do anything
        }

        // Convenience methods for common types
        public void SetString(string value, string key) => SetObjectForKey(value, key);
        public string StringForKey(string key) => ObjectForKey(key) as string;

        public  void SetIntegerForKey(int value, string key) => SetObjectForKey(value, key);
        public  int IntegerForKey(string key) => (int)(ObjectForKey(key) ?? 0);

        public  void SetFloat(float value, string key) => SetObjectForKey(value, key);
        public  float FloatForKey(string key) => (float)(ObjectForKey(key) ?? 0f);

        public  void SetDouble(double value, string key) => SetObjectForKey(value, key);
        public  double DoubleForKey(string key) => (double)(ObjectForKey(key) ?? 0.0);

        public  void SetBool(bool value, string key) => SetObjectForKey(value, key);
        public  bool BoolForKey(string key) => (bool)(ObjectForKey(key) ?? false);

        public  string[] AllKeys
        {
            get
            {
                lock (_syncLock)
                {
                    return _defaults.Keys.ToArray();
                }
            }
        }

        public  void RegisterDefaults(Dictionary<string, object> registrationDictionary)
        {
            lock (_syncLock)
            {
                foreach (var kvp in registrationDictionary)
                {
                    if (!_defaults.ContainsKey(kvp.Key))
                    {
                        _defaults[kvp.Key] = kvp.Value;
                    }
                }
                SaveDefaults();
            }
        }


         
             

        public static ArrayList GetArrayList(Dictionary<string, object> dict, string key)
        {
            if (!dict.TryGetValue(key, out var value)) return null;

            return value switch
            {
                JArray jArray => jArray.ToObject<ArrayList>(),
                ArrayList list => list,
                _ => null
            };
        }

        public static Dictionary<string, object> GetDictionary(Dictionary<string, object> dict, string key)
        {
            if (!dict.TryGetValue(key, out var value)) return null;

            return value switch
            {
                JObject jObj => jObj.ToObject<Dictionary<string, object>>(),
                Dictionary<string, object> d => d,
                _ => null
            };
        }





        private void LoadDefaults()
        {
            lock (_syncLock)
            {
                if (!File.Exists(_storagePath)) return;

                try
                {
                    string json = File.ReadAllText(_storagePath);
                    var loaded = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);

                    if (loaded != null)
                    {
                        _defaults.Clear();
                        foreach (var kvp in loaded)
                        {
                            // 处理 Newtonsoft.Json 反序列化的 JToken 类型
                            _defaults[kvp.Key] = ConvertJTokenValue(kvp.Value);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"加载用户默认设置失败: {ex.Message}");
                    _defaults.Clear();
                }
            }
        }

        private void SaveDefaults()
        {
            lock (_syncLock)
            {
                try
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(_storagePath));

                    var settings = new JsonSerializerSettings
                    {
                        Formatting = Formatting.Indented,  // 美化输出
                        NullValueHandling = NullValueHandling.Ignore
                    };

                    string json = JsonConvert.SerializeObject(_defaults, settings);
                    File.WriteAllText(_storagePath, json);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"保存用户默认设置失败: {ex.Message}");
                }
            }
        }



        // 辅助方法：处理 Newtonsoft.Json 反序列化的 JToken 类型
        private object ConvertJValue(object value)
        {
            if (value is JToken token)
            {
                switch (token.Type)
                {
                    case JTokenType.String:
                        return token.Value<string>();
                    case JTokenType.Integer:
                        return token.Value<int>();
                    case JTokenType.Float:
                        return token.Value<double>();
                    case JTokenType.Boolean:
                        return token.Value<bool>();
                    case JTokenType.Null:
                        return null;
                    default:
                        return token.ToString();
                }
            }
            return value;
        }

        // 辅助方法：处理 Newtonsoft.Json 反序列化的 JToken 类型
        private object ConvertJTokenValue(object value)
        {

            switch (value)
            {

                case JObject jObject:

                    // 处理嵌套对象 -> 转为 Dictionary<string, object>
                    var dict = new NSMutableDictionary();
                    foreach (var prop in jObject.Properties())
                    {
                        dict[prop.Name] = ConvertJTokenValue(prop.Value);
                    }
                    return dict;

                case JArray jArray:
                    // 处理数组 -> 转为 List<object>
                    var list = new List<object>();
                    foreach (var item in jArray)
                    {
                        list.Add(ConvertJTokenValue(item));
                    }
                    return list;


                case JValue jValue:
                    return ConvertJValue(value);


                default:
                    return value;
            }
        }

         
 
    }
}
