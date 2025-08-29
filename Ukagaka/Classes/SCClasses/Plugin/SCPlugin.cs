using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Ukagaka
{

    public class SCPlugin
    {
        private readonly Type _pluginType;
        private readonly string _type;
        private readonly string _name;
        private readonly DirectoryInfo _pluginRoot;

        protected SCPlugin(Type pluginType, string type, string name, DirectoryInfo pluginRoot)
        {
            _pluginType = pluginType ?? throw new ArgumentNullException(nameof(pluginType));
            _type = type ?? throw new ArgumentNullException(nameof(type));
            _name = name ?? throw new ArgumentNullException(nameof(name));
            _pluginRoot = pluginRoot ?? throw new ArgumentNullException(nameof(pluginRoot));
        }

        public static SCPlugin NewInstance(Type pluginClass, DirectoryInfo pluginRoot)
        {
            // Get plugin type and name
            string pluginType = null;
            string pluginName = null;

            try
            {
                MethodInfo getPluginType = pluginClass.GetMethod("GetPluginType", Type.EmptyTypes);
                pluginType = (string)getPluginType.Invoke(null, null);

                MethodInfo getPluginName = pluginClass.GetMethod("GetPluginName", Type.EmptyTypes);
                pluginName = (string)getPluginName.Invoke(null, null);
            }
            catch (MissingMethodException e)
            {
                Console.Error.WriteLine($"SCPlugin: Class {pluginClass.Name} doesn't have some required method.");
                Console.Error.WriteLine(e);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine($"SCPlugin: An exception occurred while accessing class {pluginClass.Name}.");
                Console.Error.WriteLine(e);
            }

            // Create appropriate plugin type based on the plugin type
            if (pluginType == "hlsensor")
            {
                return new SCHLSensorPlugin(pluginClass, pluginType, pluginName, pluginRoot);
            }
            else if (pluginType == "makoto")
            {
                // Call public static void NotifyRootDir(FileInfo) if exists
                try
                {
                    MethodInfo notifyRootDir = pluginClass.GetMethod("NotifyRootDir", new[] { typeof(DirectoryInfo) });
                    notifyRootDir?.Invoke(null, new object[] { pluginRoot });
                }
                catch { /* Ignore if method doesn't exist */ }

                // Install to SCMakotoLoader
                SCMakotoLoader.InstallMakotoModule(pluginClass);
                return new SCPlugin(pluginClass, pluginType, pluginName, pluginRoot);
            }
            else if (pluginType == "shiori")
            {
                // Call public static void NotifyRootDir(FileInfo) if exists
                try
                {
                    MethodInfo notifyRootDir = pluginClass.GetMethod("NotifyRootDir", new[] { typeof(DirectoryInfo) });
                    notifyRootDir?.Invoke(null, new object[] { pluginRoot });
                }
                catch { /* Ignore if method doesn't exist */ }

                // Call public static void NotifySaoriPath(string) if exists
                try
                {
                    string path = Path.Combine(SCFoundation.GetParentDirOfBundle(), "home", "saori");
                    MethodInfo notifySaoriPath = pluginClass.GetMethod("NotifySaoriPath", new[] { typeof(string) });
                    notifySaoriPath?.Invoke(null, new object[] { path });
                }
                catch { /* Ignore if method doesn't exist */ }

                // Install to SCShioriLoader
                SCShioriLoader.InstallShioriModule(pluginClass);
                return new SCPlugin(pluginClass, pluginType, pluginName, pluginRoot);
            }
            else
            {
                return new SCPlugin(pluginClass, pluginType, pluginName, pluginRoot);
            }
        }

        public string GetType() => _type;

        public string GetName() => _name;

        public DirectoryInfo GetRootDir() => _pluginRoot;

        public string GetDirName() => _pluginRoot.Name;

        public Type GetPluginClass() => _pluginType;
    }
}
