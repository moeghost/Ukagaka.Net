using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Ukagaka
{
    public class DynamicShioriLoader
    {
        private delegate IntPtr RequestDelegate(IntPtr h, ref long len);
        private delegate bool LoadDelegate(IntPtr h, long len);
        private delegate bool UnloadDelegate();

        private IntPtr _dllHandle;
        private RequestDelegate _requestFunc;
        private LoadDelegate _loadFunc;
        private UnloadDelegate _unloadFunc;

        public void LoadLibrary(string dllPath)
        {
            _dllHandle = NativeMethods.LoadLibrary(dllPath);
            if (_dllHandle == IntPtr.Zero)
            {
                throw new Exception($"Failed to load DLL: {dllPath}");
            }

            _requestFunc = GetFunction<RequestDelegate>("request");
            _loadFunc = GetFunction<LoadDelegate>("load");
            _unloadFunc = GetFunction<UnloadDelegate>("unload");
        }







        public string Request(string request)
        {
            // 1. 使用 ANSI 编码（对应日语 Shift-JIS）
            byte[] requestBytes = Encoding.UTF8.GetBytes(request);
            IntPtr requestPtr = Marshal.AllocHGlobal(requestBytes.Length + 1);

            try
            {
                Marshal.Copy(requestBytes, 0, requestPtr, requestBytes.Length);
                Marshal.WriteByte(requestPtr, requestBytes.Length, 0); // NULL 终止

                // 2. 确保参数类型与 aya.h 完全一致
                long responseLen = 0; // 注意必须是 long 而非 int
                IntPtr responsePtr = _requestFunc(requestPtr, ref responseLen);

                if (responsePtr == IntPtr.Zero || responseLen <= 0)
                {
                    throw new Exception($"AYA 返回空指针，可能原因:\n" +
                                      $"1. 请求格式错误\n" +
                                      $"2. DLL 未初始化\n" +
                                      $"3. 内存不足\n" +
                                      $"原始请求:\n{request}");
                }

                // 3. 按 ANSI 编码读取响应
                string response = Marshal.PtrToStringAnsi(responsePtr, (int)responseLen);
              //  Marshal.FreeHGlobal(responsePtr); // 假设需要手动释放
                return response;
            }
            finally
            {
                //Marshal.FreeHGlobal(requestPtr);
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

        public void UnloadLibrary()
        {
            if (_dllHandle != IntPtr.Zero)
            {
                NativeMethods.FreeLibrary(_dllHandle);
                _dllHandle = IntPtr.Zero;
            }
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
    }
}
