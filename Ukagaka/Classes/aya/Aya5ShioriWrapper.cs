using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Utils;
namespace aya
{



    public class Aya5ShioriWrapper : IDisposable
    {
        //==========================
        // Native Windows API
        //==========================
        [DllImport("kernel32.dll")]
        private static extern IntPtr LoadLibrary(string lpFileName);

        [DllImport("kernel32.dll")]
        private static extern bool FreeLibrary(IntPtr hModule);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GlobalAlloc(uint uFlags, UIntPtr dwBytes);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GlobalLock(IntPtr hMem);

        [DllImport("kernel32.dll")]
        private static extern bool GlobalUnlock(IntPtr hMem);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GlobalFree(IntPtr hMem);

        private const uint GMEM_MOVEABLE = 0x0002;

        //==========================
        // Shiori DLL Function Delegates
        //==========================
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate bool LoadDelegate(IntPtr h, long len);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate bool UnloadDelegate();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr RequestDelegate(IntPtr h, ref long len);

        private IntPtr _dllHandle;
        private LoadDelegate _loadFunc;
        private UnloadDelegate _unloadFunc;
        private RequestDelegate _requestFunc;

        private bool _loaded = false;
        private Encoding _shiftJis = Encoding.GetEncoding("shift_jis");

        //==========================
        // 构造函数：加载 DLL
        //==========================
        public Aya5ShioriWrapper(string dllPath)
        {
            _dllHandle = LoadLibrary(dllPath);
            if (_dllHandle == IntPtr.Zero)
                throw new DllNotFoundException($"无法加载 Shiori DLL: {dllPath}");

            _loadFunc = GetFunction<LoadDelegate>("load");
            _unloadFunc = GetFunction<UnloadDelegate>("unload");
            _requestFunc = GetFunction<RequestDelegate>("request");
        }

        private T GetFunction<T>(string name) where T : Delegate
        {
            IntPtr ptr = GetProcAddress(_dllHandle, name);
            if (ptr == IntPtr.Zero)
                throw new EntryPointNotFoundException($"未找到函数 {name}");
            return Marshal.GetDelegateForFunctionPointer<T>(ptr);
        }

        //==========================
        // Load：带 baseDir 的初始化
        //==========================
        public bool Load(string baseDir)
        {
            if (_loaded) return true;


            baseDir = baseDir.Replace("/", "\\");

            if (!baseDir.EndsWith("\\") && !baseDir.EndsWith("/"))
                baseDir += "\\"; // 确保以 \ 结尾

            IntPtr hGlobal = CreateHGlobal(baseDir, Encoding.UTF8);
            try
            {
                _loaded = _loadFunc(hGlobal, baseDir.Length);
            }
            finally
            {
                GlobalFree(hGlobal);
            }

            Console.WriteLine($"[Load] baseDir: {baseDir}, Result: {_loaded}");
            return _loaded;
        }

        //==========================
        // Request：发送 SHIORI 请求
        //==========================
        public string Request(string id)
        {
            if (!_loaded)
            {
                Console.WriteLine("[Request] 未加载 Shiori");
                return null;
            }

            string req =
                "GET SHIORI/3.0\r\n" +
                "Charset: UTF-8\r\n" +
                "Sender: WPFHost\r\n" +
                $"ID: {id}\r\n" +
                "\r\n\0";

            Console.WriteLine("[Request] 发送内容：\n" + req);

            byte[] bytes = Encoding.UTF8.GetBytes(req);
            Console.WriteLine("[Request] 十六进制：" + BitConverter.ToString(bytes));

            IntPtr hReq = CreateHGlobal(req, Encoding.UTF8);
            long respLen = 0;

            IntPtr hResp = _requestFunc(hReq, ref respLen);
            GlobalFree(hReq);

            Console.WriteLine($"[Request] respLen={respLen}, hResp={hResp}");

            if (hResp == IntPtr.Zero || respLen <= 0)
            {
                Console.WriteLine("[Request] 返回空响应");
                return null;
            }

            IntPtr pResp = GlobalLock(hResp);
            byte[] buf = new byte[respLen];
            Marshal.Copy(pResp, buf, 0, (int)respLen);
            GlobalUnlock(hResp);
            GlobalFree(hResp);

            string response = _shiftJis.GetString(buf).TrimEnd('\0');
            Console.WriteLine($"[Request] Response:\n{response}");
            return response;
        }

        //==========================
        // 卸载
        //==========================
        public void Unload()
        {
            if (_loaded && _unloadFunc != null)
            {
                _unloadFunc();
                _loaded = false;
            }
            if (_dllHandle != IntPtr.Zero)
            {
                FreeLibrary(_dllHandle);
                _dllHandle = IntPtr.Zero;
            }
        }

        public void Dispose() => Unload();

        public void Terminate()
        {
            Dispose();
        }

       



        //==========================
        // 工具：创建 HGLOBAL
        //==========================
        private IntPtr CreateHGlobal(string data, Encoding encoding)
        {
            byte[] bytes = encoding.GetBytes(data);
            IntPtr h = GlobalAlloc(GMEM_MOVEABLE, (UIntPtr)bytes.Length);
            if (h == IntPtr.Zero) throw new Exception("GlobalAlloc 失败");
            IntPtr p = GlobalLock(h);
            Marshal.Copy(bytes, 0, p, bytes.Length);
            GlobalUnlock(h);
            return h;
        }
    }


    // 使用示例

    internal class Aya5ShioriWrapper1: IDisposable
    {
        private IntPtr _buffer;


        // AYA5 的函数委托（与 DLL 导出匹配）
        private delegate IntPtr RequestDelegate(IntPtr h, ref long len);
        private delegate bool LoadDelegate(IntPtr h, long len);
        private delegate bool UnloadDelegate();
        private IntPtr _dllHandle;
        private RequestDelegate _requestFunc;
        private LoadDelegate _loadFunc;
        private UnloadDelegate _unloadFunc;

        private bool _loaded;


        public static bool IsFileLocked(string filePath)
        {
            try
            {
                using (System.IO.File.Open(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                    return false;
            }
            catch (IOException)
            {
                return true;
            }
        }

        private T GetFunction<T>(string functionName) where T : Delegate
        {
            IntPtr funcPtr = NativeMethods.GetProcAddress(_dllHandle, functionName);
            if (funcPtr == IntPtr.Zero)
            {
                throw new Exception($"Function {functionName} not found");
            }
            return Marshal.GetDelegateForFunctionPointer<T>(funcPtr);
        }
        private static class NativeMethods
        {
            [DllImport("kernel32.dll")]
            public static extern IntPtr LoadLibrary(string dllToLoad);

            [DllImport("kernel32.dll")]
            public static extern IntPtr GetProcAddress(IntPtr hModule, string procedureName);

            [DllImport("kernel32.dll")]
            public static extern bool FreeLibrary(IntPtr hModule);
        }


        public Aya5ShioriWrapper1(string dllPath)
        {
            //if (IsFileLocked(dllPath))
            {
               // return;

            }
            // 1. 加载 DLL
            try
            {
                _dllHandle = NativeMethods.LoadLibrary(dllPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"加载失败: {ex.GetType().Name}");
                Console.WriteLine($"错误详情: {ex.Message}");
                Console.WriteLine($"建议: {GetFixAdvice(ex)}");
                throw;
            }
            if (_dllHandle == IntPtr.Zero)
                throw new DllNotFoundException($"Failed to load AYA5 DLL: {dllPath}");

            // 2. 获取函数指针并绑定委托
            IntPtr loadPtr = NativeLibrary.GetExport(_dllHandle, "load");
            IntPtr requestPtr = NativeLibrary.GetExport(_dllHandle, "request");

            

            _requestFunc = GetFunction<RequestDelegate>("request");
            _loadFunc = GetFunction<LoadDelegate>("load");
            _unloadFunc = GetFunction<UnloadDelegate>("unload");


            _buffer = Utils.NativeMemory.Alloc(1024);


        }

        // 根据异常类型返回建议
        private static string GetFixAdvice(Exception ex)
        {
            return ex switch
            {
                DllNotFoundException => "1. 检查 DLL 路径是否正确\n2. 确认依赖项是否完整",
                BadImageFormatException => "DLL 位数不匹配（x86/x64）",
                EntryPointNotFoundException => "DLL 导出函数缺失或依赖未满足",
                _ => "未知错误，请检查日志"
            };
        }

        public string GetModuleName() => "AYA5";

        public bool Load()
        {
            if (_loaded) return true;

            string initData = "AYA5_INIT_DATA";
            byte[] initBytes = Encoding.UTF8.GetBytes(initData);

            // 使用 NativeMemory 分配内存
            IntPtr hGlobal = Utils.NativeMemory.Alloc(initBytes.Length + 1);
            try
            {
                Marshal.Copy(initBytes, 0, hGlobal, initBytes.Length);
                Marshal.WriteByte(hGlobal, initBytes.Length, 0); // Null-terminator

                _loaded = _loadFunc(hGlobal, initBytes.Length);
                return _loaded;
            }
            finally
            {
                // 立即释放临时内存（假设DLL已内部复制数据）
               // Utils.NativeMemory.Free(hGlobal);
            }
        }



        public string Request(string input)
        {
            if (!_loaded || string.IsNullOrEmpty(input))
                return null;

            // 1. 准备输入数据
            byte[] inputBytes = Encoding.UTF8.GetBytes(input);
            IntPtr inputPtr = Utils.NativeMemory.Alloc(inputBytes.Length + 1);
            try
            {
                Marshal.Copy(inputBytes, 0, inputPtr, inputBytes.Length);
                Marshal.WriteByte(inputPtr, inputBytes.Length, 0);

                // 2. 调用 DLL
                long responseLen = 0;
                IntPtr responsePtr = _requestFunc(inputPtr, ref responseLen);

                // 3. 处理响应
                if (responsePtr == IntPtr.Zero || responseLen <= 0)
                {
                    Console.WriteLine("DLL 返回空响应");
                    return null;
                }

                // 4. 复制数据并释放（假设DLL返回新内存）
                string response = Marshal.PtrToStringUTF8(responsePtr, (int)responseLen);
                Utils.NativeMemory.Free(responsePtr);
                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Request 异常: {ex}");
                return null;
            }
            finally
            {
              //  NativeMemory.Free(inputPtr);
            }
        }


         
        private void Cleanup() => Marshal.FreeHGlobal(_buffer);

 

        public void Terminate()
        {
            if (_loaded)
            {
                _loaded = false;
                // 注：AYA5 没有 unload 函数，这里仅释放 DLL
            }
            if (_dllHandle != IntPtr.Zero)
            {
                NativeLibrary.Free(_dllHandle);
            }
        }

        public void Dispose() => Terminate();
    }
}
