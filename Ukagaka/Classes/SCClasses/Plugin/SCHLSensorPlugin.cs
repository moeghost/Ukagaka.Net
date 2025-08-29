using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace Ukagaka
{
    public class SCHLSensorPlugin : SCPlugin
    {
        private readonly MethodInfo _methodRequest;
        private readonly object _instance;

        public SCHLSensorPlugin(Type plugin, string type, string name, DirectoryInfo pluginRoot)
            : base(plugin, type, name, pluginRoot)
        {
            try
            {
                // Get the Request method that takes an InputStream
                _methodRequest = plugin.GetMethod("Request", new[] { typeof(Stream) });

                // Get constructor that takes a FileInfo
                ConstructorInfo constructor = plugin.GetConstructor(new[] { typeof(DirectoryInfo) });
                _instance = constructor.Invoke(new object[] { pluginRoot });
            }
            catch (MissingMethodException e)
            {
                Console.Error.WriteLine($"SCHLSensorPlugin: Class {plugin.Name} doesn't have required method Request().");
                Console.Error.WriteLine(e);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine($"SCHLSensorPlugin: An exception occurred while accessing class {plugin.Name}.");
                Console.Error.WriteLine(e);
            }
        }

        public SCHLSensorResponse Request(string request)
        {
            try
            {
                using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(request)))
                {
                    return Request(stream);
                }
            }
            catch
            {
                return null;
            }
        }

        public SCHLSensorResponse Request(Stream requestStream)
        {
            try
            {
                // Invoke the Request method
                var responseStream = (Stream)_methodRequest.Invoke(_instance, new object[] { requestStream });

                using (var reader = new StreamReader(responseStream, Encoding.UTF8))
                {
                    // Read header
                    string header = reader.ReadLine();

                    // Read headers into dictionary
                    var headers = new Dictionary<string, string>();
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (string.IsNullOrEmpty(line)) break;

                        int colonIndex = line.IndexOf(':');
                        if (colonIndex == -1) continue;

                        string label = line.Substring(0, colonIndex).Trim();
                        string data = line.Substring(colonIndex + 1).Trim();

                        headers[label] = data;
                    }

                    // Read remaining content
                    string content = reader.ReadToEnd();

                    return new SCHLSensorResponse(header, headers, content);
                }
            }
            catch
            {
                return null;
            }
        }
    }
     
}