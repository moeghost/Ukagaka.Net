using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace aya
{
    internal class AyaShioriInterop
    {

        // AYA 的导出函数签名（需与 aya.h 一致）
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr AyaRequestDelegate(IntPtr h, out long len);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate bool Aya5LoadDelegate(IntPtr h, long len);


        private IntPtr _ayaDllHandle;
        private AyaRequestDelegate _requestFunc;
        private Aya5LoadDelegate _loadFunc;
        private bool _loaded;


        public AyaShioriInterop(string dllPath)
        {

            _ayaDllHandle = NativeLibrary.Load(dllPath);
            if (_ayaDllHandle == IntPtr.Zero) return;



            IntPtr loadPtr = NativeLibrary.GetExport(_ayaDllHandle, "load");
            _loadFunc = Marshal.GetDelegateForFunctionPointer<Aya5LoadDelegate>(loadPtr);


            IntPtr requestPtr = NativeLibrary.GetExport(_ayaDllHandle, "request");
            _requestFunc = Marshal.GetDelegateForFunctionPointer<AyaRequestDelegate>(requestPtr);

        }





        // 加载 AYA.dll
        public bool Load()
        {
            if (_loaded) return true;

            string initData = "AYA5_INIT_DATA";
            byte[] initBytes = Encoding.Default.GetBytes(initData);


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

        // 发送 SHIORI 请求
        public string Request(string shioriRequest)
        {
            byte[] requestBytes = Encoding.Default.GetBytes(shioriRequest);
            IntPtr requestPtr = Marshal.AllocHGlobal(requestBytes.Length + 1);
            Marshal.Copy(requestBytes, 0, requestPtr, requestBytes.Length);
            Marshal.WriteByte(requestPtr, requestBytes.Length, 0); // Null-terminator

            try
            {
                long responseLen;
                IntPtr responsePtr = _requestFunc(requestPtr, out responseLen);
                if (responsePtr == IntPtr.Zero || responseLen == 0)
                    return null;

                string response = Marshal.PtrToStringUTF8(responsePtr, (int)responseLen);
                Marshal.FreeHGlobal(responsePtr); // 假设 AYA 要求调用者释放内存
                return response;
            }
            finally
            {
               // Marshal.FreeHGlobal(requestPtr);
            }
        }

        // 卸载
        public void Unload()
        {
            if (_ayaDllHandle != IntPtr.Zero)
            {
                NativeLibrary.Free(_ayaDllHandle);
                _ayaDllHandle = IntPtr.Zero;
            }
        }
    }
}
