using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Ukagaka
{
    public class SCPluginManager
    {
        private readonly List<SCPlugin> _plugins = new List<SCPlugin>();

        public SCPluginManager()
        {
            Rescan();
        }

        public void Rescan()
        {
            // Rescan home/plugin directory and load any new plugins
            var homePlugin = new DirectoryInfo(Path.Combine(SCFoundation.GetParentDirOfBundle(), "home", "plugin"));

            if (!homePlugin.Exists)
                return;

            foreach (var pluginDir in homePlugin.GetDirectories())
            {
                // Skip if already loaded
                if (FindPlugin(pluginDir) != null)
                    continue;

                // First try to load alias.txt
                Utils.File aliasFile = new Utils.File(pluginDir.FullName, "alias.txt");
                string fileName = null;

                if (aliasFile.Exists())
                {
                    var descm = new SCDescription(aliasFile);
                    fileName = descm.GetStrValue("plugin");
                }

                if (fileName == null)
                {
                    var jarFile = new FileInfo(Path.Combine(pluginDir.FullName, "plugin.dll"));
                    if (jarFile.Exists)
                    {
                        fileName = "plugin.dll";
                    }
                    else
                    {
                        var classFile = new FileInfo(Path.Combine(pluginDir.FullName, "plugin.dll"));
                        if (classFile.Exists)
                        {
                            fileName = "plugin.dll";
                        }
                        else
                        {
                            continue; // Invalid plugin
                        }
                    }
                }

                // Load the class
                var targetFile = new FileInfo(Path.Combine(pluginDir.FullName, fileName));
                if (!targetFile.Exists)
                    continue;

                Type pluginType;
                if (fileName.EndsWith(".dll"))
                {
                    try
                    {
                        var assembly = Assembly.LoadFrom(targetFile.FullName);
                        // Look for the plugin main class attribute
                        var pluginAttr = assembly.GetCustomAttribute<SCPluginMainClassAttribute>();
                        if (pluginAttr == null)
                            continue;

                        pluginType = assembly.GetType(pluginAttr.MainClassName);
                        if (pluginType == null)
                            continue;
                    }
                    catch
                    {
                        continue;
                    }
                }
                else
                {
                    // Unsupported format in .NET
                    continue;
                }

                // Create SCPlugin instance
                var plugin = SCPlugin.NewInstance(pluginType, pluginDir);
                if (plugin != null)
                {
                    _plugins.Add(plugin);
                }
            }
        }

        public SCPlugin FindPlugin(DirectoryInfo pluginDir)
        {
            return _plugins.FirstOrDefault(p =>
                p.GetRootDir().FullName.Equals(pluginDir.FullName, StringComparison.OrdinalIgnoreCase));
        }

        public SCPlugin[] GetPlugins()
        {
            return _plugins.ToArray();
        }

        public SCPlugin[] GetPlugins(string type)
        {
            return _plugins.Where(p => p.GetType().Equals(type, StringComparison.OrdinalIgnoreCase))
                          .ToArray();
        }
    }

}
