using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace aya
{
    internal class Aya5Native
    {
        // 委托类型
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate bool LoadDelegate(IntPtr h, long len);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate IntPtr RequestDelegate(IntPtr h, out long len);

        // 函数指针（通过 NativeLibrary 动态加载）
        public static LoadDelegate? LoadFunc;
        public static RequestDelegate? RequestFunc;
    }
}
