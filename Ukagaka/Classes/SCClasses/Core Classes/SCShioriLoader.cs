using System;
using System.Collections.Generic;
//using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Collections;
using Utils;
namespace Ukagaka
{




    public class SCShioriLoader
    {
        private static List<Type> SHIORI_MODULES;

        static SCShioriLoader()
        {
            // 初始化 SHIORI 模块列表
            SHIORI_MODULES = new List<Type>
        {
           // typeof(SCKawari),
            typeof(aya.Aya),
         //   typeof(satori.Satori),
          //  typeof(SCEseShiori),
          //  typeof(misaka.Misaka),
          //  typeof(SCNiseShiori)
            // typeof(SCDebugShiori) // 调试用
        };
        }

        public static void InstallShioriModule(Type module)
        {
            // 已存在同名模块则替换
            string moduleName = GetModuleName(module);
            SHIORI_MODULES.RemoveAll(m => GetModuleName(m) == moduleName);
            SHIORI_MODULES.Insert(0, module); // 插入到首位
        }

        public static string GetShioriDllFileName(File spiritRoot)
        {
            SCDescription desc = new SCDescription(new File(spiritRoot.GetFullName(), "descript.txt"));
            if (desc.Exists("shiori"))
                return desc.GetStrValue("shiori");

            SCDescription alias = new SCDescription(new File(spiritRoot.GetFullName(), "alias.txt"));
            if (alias.Exists("shiori"))
                return alias.GetStrValue("shiori");

            return "shiori.dll";
        }

        public static SCShioriAccessor LoadShioriModule(File spiritRoot)
        {
            SCShioriAccessor shiori = null;
            string shioriDllName = GetShioriDllFileName(spiritRoot);

            File shioriDllFile = new File(spiritRoot.GetFullName(), shioriDllName);
             
            string testInput = "GET SHIORI/3.0\r\n" +
             "Charset: Shift_JIS\r\n" +
             "Sender: SSP\r\n" +
             "ID: OnBoot\r\n" +
             "\r\n\0";



            if (shioriDllFile.Exists())
            {
                // 直接加载 AYA5 DLL
                try
                {

                    var ayaWrapper = new aya.Aya5ShioriWrapper(shioriDllFile.GetFullName());
                    if (ayaWrapper.Load(spiritRoot.GetParentFile().GetFullName())) // 调用 Load 初始化
                    {

                        var response = ayaWrapper.Request("version"); // 测试基础功能
 
                        return new SCShioriAccessor(ayaWrapper);
                    }
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"AYA5 加载失败: {ex.Message}");
                }
                return null;

                //return new SCShioriAccessor(new aya.Aya5ShioriWrapper(shioriDllFile));
            }



            // 检查是否是 aya.dll
            if (shioriDllName.Equals("yaya.dll", StringComparison.OrdinalIgnoreCase))
            {
                if (shioriDllFile.Exists())
                {
                    try
                    {
                        var ayaWrapper = new aya.Aya5ShioriWrapper(shioriDllFile.GetFullName());
                        if (ayaWrapper.Load(spiritRoot.GetFullName())) // 调用 Load 初始化
                        {

                            testInput = "GET SHIORI/3.0\r\n" +
                             "Charset: UTF-8\r\n" +
                              "ID: OnBoot\r\n" +
                             "\r\n";
                            var response = ayaWrapper.Request(testInput); // 测试基础功能

                             

                            return new SCShioriAccessor(ayaWrapper);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine($"AYA5 加载失败: {ex.Message}");
                    }
                    return null;
                }
            }







            foreach (Type module in SHIORI_MODULES)
            {
                if (CheckIfUsable(module, spiritRoot, shioriDllName))
                {
                    shiori = NewAccessor(module, spiritRoot, shioriDllName);
                    if (shiori != null) break;
                }
            }

            if (shiori == null)
            {
                File substitute = new File(
                    SCFoundation.GetParentDirOfBundle(), "home/_system/substitute");
                /*
                if (SCKawari.CheckIfUsable(substitute))
                {
                    try
                    {
                        shiori = new SCShioriAccessor(new SCKawari(substitute));
                    }
                    catch { }
                }*/
            }

            return shiori;
        }

        public static string GetModuleName(File spiritRoot)
        {
            string shioriDllName = GetShioriDllFileName(spiritRoot);

            foreach (Type module in SHIORI_MODULES)
            {
                if (CheckIfUsable(module, spiritRoot, shioriDllName))
                {
                    return GetModuleName(module);
                }
            }
            return null;
        }

        public static string GetModuleName(Type module)
        {
            try
            {
                MethodInfo method = module.GetMethod("GetModuleName", BindingFlags.Public | BindingFlags.Static);
                return (string)method.Invoke(null, null);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine($"SCShioriLoader error : Failed to access GetModuleName() in {module.Name}");
                Console.Error.WriteLine(e);
                return null;
            }
        }

        public static string GetModuleInfo(Type module)
        {
            try
            {
                MethodInfo method = module.GetMethod("GetModuleInfo", BindingFlags.Public | BindingFlags.Static);
                return (string)method.Invoke(null, null);
            }
            catch
            {
                return null;
            }
        }

        public static bool CheckIfUsable(Type module, File rootDir, string dllName)
        {
            try
            {
                // 优先使用 checkIfUsable(File, File)
                MethodInfo method = module.GetMethod("CheckIfUsable", new[] { typeof(File), typeof(File) });
                if (method != null)
                {
                    return (bool)method.Invoke(null, new object[]
                    {
                    rootDir, new File(rootDir.GetFullName(), dllName)
                    });
                }
                else
                {
                    // 回退到 checkIfUsable(File)
                    method = module.GetMethod("CheckIfUsable", new[] { typeof(File) });
                    if (method != null)
                        return (bool)method.Invoke(null, new object[] { rootDir });
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine($"SCShioriLoader error : Exception in CheckIfUsable() for {module.Name}");
                Console.Error.WriteLine(e);
            }
            return false;
        }

        public static SCShioriAccessor NewAccessor(Type module, File rootDir, string dllName)
        {
            try
            {
                // 优先使用 (File, File) 构造器
         

                ConstructorInfo ctor = module.GetConstructor(new[] { typeof(File), typeof(File) });
                if (ctor != null)
                {
                    object instance = ctor.Invoke(new object[]
                    {
                    rootDir, new File(rootDir.GetFullName(), dllName)
                    });
                    return new SCShioriAccessor(instance);
                }

                // 回退到 (File) 构造器
                ctor = module.GetConstructor(new[] { typeof(File) });
                if (ctor != null)
                {
                    object instance = ctor.Invoke(new object[] { rootDir });
                    return new SCShioriAccessor(instance);
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine($"SCShioriLoader error : Exception in NewAccessor() for {module.Name}");
                Console.Error.WriteLine(e);
            }
            return null;
        }
    }







































    public class SCShioriLoader1
    {
        private static ArrayList SHIORI_MODULES; // content: (Type)
        static SCShioriLoader1()
        {
            // SHIORI_MODULESには、栞モジュールクラス一覧が入っています。
            // Typeオブジェクトさえあれば、クラスはファイルからロードしようがネットワーク越しにロードしようが全く構いません。
            // 例えばhome/pluginに栞モジュールをプラグインとしてインストールし、それをこのリストに追加する等の処理が考えられます。
            SHIORI_MODULES = new ArrayList();
            //SHIORI_MODULES.Add(typeof(SCKawari));
            SHIORI_MODULES.Add(typeof(aya.Aya));
          //  SHIORI_MODULES.Add(typeof(satori.Satori));
           // SHIORI_MODULES.Add(typeof(SCEseShiori));
          //  SHIORI_MODULES.Add(typeof(misaka.Misaka));
         //   SHIORI_MODULES.Add(typeof(SCNiseShiori)); // 偽栞は一番最後に判定しなければならない。
                                                      //SHIORI_MODULES.Add(typeof(SCDebugShiori)); // 偽林檎本体のデバッグ用
        }

        public static void InstallShioriModule(Type module)
        {
            // 既に同名のモジュールが存在すれば置き換える。
            string moduleName = GetModuleName(module);

            for (int i = 0; i < SHIORI_MODULES.Count; i++)
            {
                Type currentModule = (Type)SHIORI_MODULES[i];
                if (GetModuleName(currentModule) == moduleName)
                {
                    SHIORI_MODULES.RemoveAt(i);
                    i--;
                }
            }

            SHIORI_MODULES.Insert(0, module); // 先頭に。
        }

        public static string GetShioriDllFileName(File spiritRoot)
        {
            SCDescription desc = new SCDescription(new File(spiritRoot.GetPath(), "descript.txt"));
            if (desc.Exists("shiori"))
            {
                return desc.GetStrValue("shiori");
            }

            SCDescription alias = new SCDescription(new File(spiritRoot.GetPath(), "alias.txt"));
            if (alias.Exists("shiori"))
            {
                return alias.GetStrValue("shiori");
            }

            return "shiori.dll";
        }

         


        public static SCShioriAccessor LoadShioriModule(File spiritRoot)
        {
            // 認識できるSHIORIが無ければ、home/_system/substituteの代用ゴーストがロードされます。
            // DLLを判別する為にspirit_rootのdescript.txtもしくはalias.txtを読みます。
            SCShioriAccessor shiori = null;
            string shioriDllName = GetShioriDllFileName(spiritRoot);
            File shioriDllFile = new File(spiritRoot.GetFullName(), shioriDllName);
             

            // 检查是否是 aya.dll
            if (shioriDllName.Equals("aya.dll", StringComparison.OrdinalIgnoreCase))
            {
                if (shioriDllFile.Exists())
                {
                    // 直接加载 AYA5 DLL
                    return new SCShioriAccessor(new aya.Aya5ShioriWrapper(shioriDllFile.GetFullName()));
                }
            }





            foreach (Type currentModule in SHIORI_MODULES)
            {
                if (CheckIfUsable(currentModule, spiritRoot, shioriDllName))
                {
                    shiori = NewAccessor(currentModule, spiritRoot, shioriDllName);
                    if (shiori != null)
                    {
                        break;
                    }
                }
            }

            if (shiori == null)
            {
                File substitute = new File(SCFoundation.GetParentDirOfBundle(), "home/_system/substitute");
              //  if (SCKawari.CheckIfUsable(substitute))
                {
                //    try { shiori = new SCShioriAccessor(new SCKawari(substitute)); } catch (Exception) { }
                }
            }

            return shiori;
        }

        public static string GetModuleName(File spiritRoot)
        {
            // DLLを判別する為にspirit_rootのdescript.txtを読みます。
            string moduleName = null;
            string shioriDllName = GetShioriDllFileName(spiritRoot);

            foreach (Type currentModule in SHIORI_MODULES)
            {
                if (CheckIfUsable(currentModule, spiritRoot, shioriDllName))
                {
                    moduleName = GetModuleName(currentModule);
                    break;
                }
            }

            return moduleName;
        }

        public static string GetModuleName(Type module)
        {
            try
            {
                MethodInfo getModuleName = module.GetMethod("GetModuleName", Type.EmptyTypes);
                return (string)getModuleName.Invoke(null, null);
            }
           /* catch (NoSuchMethodException)
            {
                Console.Error.WriteLine($"SCShioriLoader error : Shiori module class \"{module.FullName}\" doesn't have method GetModuleName().");
            }*/
            catch (Exception e)
            {
                Console.Error.WriteLine($"SCShioriLoader error : Exception has occured during accessing shiori module class \"{module.FullName}\".");
                Console.Error.WriteLine(e.StackTrace);
            }
            return null;
        }

        public static string GetModuleInfo(Type module)
        {
            // public static string GetModuleInfo()を呼ぶ。無ければnullを返す。
            try
            {
                MethodInfo getModuleInfo = module.GetMethod("GetModuleInfo", Type.EmptyTypes);
                return (string)getModuleInfo.Invoke(null, null);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static bool CheckIfUsable(Type module, File rootdir, string dllName)
        {
            // public static bool CheckIfUsable(DirectoryInfo ghostDir, FileInfo shioriDll) 優先
            // public static bool CheckIfUsable(DirectoryInfo ghostDir) フォールバック
            try
            {
                try
                {
                    MethodInfo checkRoutine = module.GetMethod("CheckIfUsable", new Type[] { typeof(File), typeof(File) });

                    if (checkRoutine == null)
                    {
                        return false;
                    }

                    bool response = (bool)checkRoutine.Invoke(null, new object[] { rootdir, new File(rootdir.GetPath(), dllName) });
                    return response;
                }
                catch
                { 
                    return false; 
                
                }
                /*
                catch (NoSuchMethodException)
                {
                    MethodInfo checkRoutine = module.GetMethod("CheckIfUsable", new Type[] { typeof(DirectoryInfo) });
                    bool response = (bool)checkRoutine.Invoke(null, new object[] { rootdir });
                    return response;
                }*/
            }
            /*
            catch (NoSuchMethodException)
            {
                Console.Error.WriteLine($"SCShioriLoader error : Shiori module class \"{module.FullName}\" doesn't have method CheckIfUsable() .");
            }*/
            catch (Exception e)
            {
                Console.Error.WriteLine($"SCShioriLoader error : Exception has occured during accessing shiori module class \"{module.FullName}\".");
                Console.Error.WriteLine(e.StackTrace);
            }
            return false;
        }

        public static SCShioriAccessor NewAccessor(Type module, File rootdir, string dllName)
        {
            // public Foo(DirectoryInfo ghostDir, FileInfo shioriDll) 優先
            // public Foo(DirectoryInfo ghostDir) フォールバック
            try
            {
                try
                {
                    ConstructorInfo con = module.GetConstructor(new Type[] { typeof(File), typeof(File) });
                    return new SCShioriAccessor(con.Invoke(new object[] { rootdir, new File(rootdir.GetPath(), dllName) }));
                }
                catch
                {
                    ConstructorInfo con = module.GetConstructor(new Type[] { typeof(File) });
                    return new SCShioriAccessor(con.Invoke(new object[] { rootdir }));
                }
                /*
                catch (NoSuchMethodException)
                {
                    ConstructorInfo con = module.GetConstructor(new Type[] { typeof(DirectoryInfo) });
                    return new SCShioriAccessor((SCShiori)con.Invoke(new object[] { rootdir }));
                }
                */
            }
            /*
            catch (NoSuchMethodException)
            {
                Console.WriteLine($"SCShioriLoader error : Shiori module class \"{module.FullName}\" doesn't have some needed methods.");
            }
            */
            catch (Exception e)
            {
                Console.WriteLine($"SCShioriLoader error : Exception has occured during accessing shiori module class \"{module.FullName}\".");
                Console.WriteLine(e.StackTrace);
            }
            return null;
        }
    }

}
