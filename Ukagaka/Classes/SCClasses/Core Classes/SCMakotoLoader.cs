using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Utils;

namespace Ukagaka 
{

    public static class SCMakotoLoader
    {
        private static readonly List<Type> _makotoModules = new List<Type> { typeof(SCExtMakoto) };

        public static void InstallMakotoModule(Type module)
        {
            // Replace if module with same name already exists
            string moduleName = GetModuleName(module);

            for (int i = 0; i < _makotoModules.Count; i++)
            {
                Type currentModule = _makotoModules[i];
                if (GetModuleName(currentModule) == moduleName)
                {
                    _makotoModules.RemoveAt(i);
                    i--;
                }
            }

            _makotoModules.Insert(0, module);
        }


        public static List<SCMakotoAccessor> LoadMakotoModules(Utils.File spiritRoot)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(spiritRoot.GetFullName());
            return LoadMakotoModules(directoryInfo);
        }


        public static List<SCMakotoAccessor> LoadMakotoModules(DirectoryInfo spiritRoot)
        {
            var result = new List<SCMakotoAccessor>();

            // Read descript.txt, fall back to alias.txt if needed
            string entryMakoto;
            var descriptTxt = new SCDescription(new Utils.File(spiritRoot.FullName, "descript.txt"));
            if (descriptTxt.Exists("makoto"))
            {
                entryMakoto = descriptTxt.GetStrValue("makoto");
            }
            else
            {
                var aliasTxt = new SCDescription(new Utils.File(spiritRoot.FullName, "alias.txt"));
                entryMakoto = aliasTxt.Exists("makoto") ? aliasTxt.GetStrValue("makoto") : "makoto.dll";
            }

            if (entryMakoto.StartsWith("[")) // Pipe syntax
            {
                string[] files = entryMakoto.Substring(1, entryMakoto.Length - 2).Split(',');
                foreach (string file in files)
                {
                    string filePath = Path.Combine(spiritRoot.FullName, file.Trim());
                     
                    if (System.IO.File.Exists(filePath))
                    {
                        SCMakotoAccessor makoto = LoadMakoto(new FileInfo(filePath));
                        if (makoto != null)
                        {
                            result.Add(makoto);
                        }
                    }
                }
            }
            else
            {
                string filePath = Path.Combine(spiritRoot.FullName, entryMakoto);
                if (System.IO.File.Exists(filePath))
                {
                    SCMakotoAccessor makoto = LoadMakoto(new FileInfo(filePath));
                    if (makoto != null)
                    {
                        result.Add(makoto);
                    }
                }
            }

            return result;
        }

        private static SCMakotoAccessor LoadMakoto(FileInfo makotoFile)
        {
            foreach (Type module in _makotoModules)
            {
                if (CheckIfUsable(module, makotoFile))
                {
                    SCMakotoAccessor makoto = NewAccessor(module, makotoFile);
                    if (makoto != null)
                    {
                        return makoto;
                    }
                }
            }
            return null;
        }

        public static ArrayList GetModuleNames(Utils.File spiritRoot)
        {
            var result = new ArrayList();

            var aliasTxt = new SCDescription(new Utils.File(spiritRoot, "alias.txt"));
            string entryMakoto = aliasTxt.Exists("makoto") ? aliasTxt.GetStrValue("makoto") : "makoto.dll";

            if (entryMakoto.StartsWith("[")) // Pipe syntax
            {
                string[] files = entryMakoto.Substring(1, entryMakoto.Length - 2).Split(',');
                foreach (string file in files)
                {
                    string filePath = Path.Combine(spiritRoot.GetFullName(), file.Trim());
                    if (System.IO.File.Exists(filePath))
                    {
                        string modName = GetModuleName(new FileInfo(filePath));
                        result.Add(modName ?? "");
                    }
                }
            }
            else
            {
                string filePath = Path.Combine(spiritRoot.GetFullName(), entryMakoto);
                if (System.IO.File.Exists(filePath))
                {
                    string modName = GetModuleName(new FileInfo(filePath));
                    result.Add(modName ?? "");
                }
            }

            return result;
        }

        public static string GetModuleName(Type module)
        {
            try
            {
                MethodInfo getModuleName = module.GetMethod("GetModuleName", Type.EmptyTypes);
                return (string)getModuleName.Invoke(null, null);
            }
            catch (MissingMethodException e)
            {
                Console.Error.WriteLine($"SCMakotoLoader error: Makoto module class \"{module.Name}\" doesn't have method GetModuleName().");
                Console.Error.WriteLine(e);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine($"SCMakotoLoader error: Exception occurred while accessing makoto module class \"{module.Name}\".");
                Console.Error.WriteLine(e);
            }
            return null;
        }

        public static string GetModuleInfo(Type module)
        {
            try
            {
                MethodInfo getModuleInfo = module.GetMethod("GetModuleInfo", Type.EmptyTypes);
                return (string)getModuleInfo.Invoke(null, null);
            }
            catch
            {
                return null;
            }
        }

        public static bool CheckIfUsable(Type module, FileInfo makotoFile)
        {
            try
            {
                MethodInfo checkRoutine = module.GetMethod("CheckIfUsable", new[] { typeof(FileInfo) });
                return (bool)checkRoutine.Invoke(null, new object[] { makotoFile });
            }
            catch (MissingMethodException e)
            {
                Console.Error.WriteLine($"SCMakotoLoader error: Makoto module class \"{module.Name}\" doesn't have method CheckIfUsable().");
                Console.Error.WriteLine(e);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine($"SCMakotoLoader error: Exception occurred while accessing makoto module class \"{module.Name}\".");
                Console.Error.WriteLine(e);
            }
            return false;
        }

        public static SCMakotoAccessor NewAccessor(Type module, FileInfo makotoFile)
        {
            try
            {
                ConstructorInfo constructor = module.GetConstructor(new[] { typeof(FileInfo) });
                return new SCMakotoAccessor(constructor.Invoke(new object[] { makotoFile }));
            }
            catch (MissingMethodException e)
            {
                Console.Error.WriteLine($"SCMakotoLoader error: Makoto module class \"{module.Name}\" doesn't have required constructor.");
                Console.Error.WriteLine(e);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine($"SCMakotoLoader error: Exception occurred while accessing makoto module class \"{module.Name}\".");
                Console.Error.WriteLine(e);
            }
            return null;
        }

        private static string GetModuleName(FileInfo makotoFile)
        {
            foreach (Type module in _makotoModules)
            {
                if (CheckIfUsable(module, makotoFile))
                {
                    return GetModuleName(module);
                }
            }
            return null;
        }
    }
}
