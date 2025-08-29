using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Utils
{
    public class NativeMemory
    {
        private static readonly ConcurrentBag<IntPtr> _allocatedPointers = new();
        private static readonly object _initLock = new();
        private static bool _hooksRegistered;

        public static IntPtr Alloc(int size)
        {
            RegisterCleanupHooks();

            var ptr = Marshal.AllocHGlobal(size);
            _allocatedPointers.Add(ptr);
            return ptr;
        }

        public static void Free(IntPtr ptr)
        {
            if (ptr != IntPtr.Zero && _allocatedPointers.TryTake(out _))
            {
                Marshal.FreeHGlobal(ptr);
            }
        }

        private static void RegisterCleanupHooks()
        {
            lock (_initLock)
            {
                if (_hooksRegistered) return;

                AppDomain.CurrentDomain.ProcessExit += (s, e) => Cleanup();
                AppDomain.CurrentDomain.DomainUnload += (s, e) => Cleanup();
                _hooksRegistered = true;
            }
        }

        private static void Cleanup()
        {
            foreach (var ptr in _allocatedPointers)
            {
                Marshal.FreeHGlobal(ptr);
            }
            _allocatedPointers.Clear();
        }
    }
}
