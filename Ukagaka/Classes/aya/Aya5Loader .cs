using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace aya
{
    internal class Aya5Loader : IDisposable
    {
        private IntPtr _libraryHandle;

        // 定义函数委托
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate bool LoadDelegate(IntPtr h, long len);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate IntPtr RequestDelegate(IntPtr h, out long len);

        public LoadDelegate? LoadFunc;
        public RequestDelegate? RequestFunc;

        public bool Load(string dllPath)
        {
            try
            {
                // 加载DLL
                _libraryHandle = NativeLibrary.Load(dllPath);
                if (_libraryHandle == IntPtr.Zero)
                    return false;

                // 获取函数指针
                IntPtr loadPtr = NativeLibrary.GetExport(_libraryHandle, "load");
                IntPtr requestPtr = NativeLibrary.GetExport(_libraryHandle, "request");

                // 转换为委托
                LoadFunc = Marshal.GetDelegateForFunctionPointer<LoadDelegate>(loadPtr);
                RequestFunc = Marshal.GetDelegateForFunctionPointer<RequestDelegate>(requestPtr);

                return LoadFunc != null && RequestFunc != null;
            }
            catch
            {
                Unload();
                return false;
            }
        }

        public void Unload()
        {
            if (_libraryHandle != IntPtr.Zero)
            {
                NativeLibrary.Free(_libraryHandle);
                _libraryHandle = IntPtr.Zero;
                LoadFunc = null;
                RequestFunc = null;
            }
        }

        public void Dispose() => Unload();
    }
}
