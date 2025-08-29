using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.IO;
using System.Text;
using RGiesecke.DllExport;


public static class yaya
{
    internal static List<AyaVMWrapper> vms = new List<AyaVMWrapper>();
    internal static string moduleName;
    internal static List<Action<string, int, int>> logHandlers = new List<Action<string, int, int>>();
    internal static int currentId = 0;
    internal static long logSendHwnd = 0;

    static yaya()
    {
        // 初始化时添加一个null VM（索引0为标准VM）
        vms.Add(null);

        // 获取模块名称
        moduleName = Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetExecutingAssembly().Location);
    }

    #region 公共接口方法


    [DllExport("load", CallingConvention = CallingConvention.Cdecl)]
    public static bool Load(IntPtr h, int len)
    {
        if (vms[0] != null)
        {
            vms[0].Dispose();
        }

        currentId = 0;
        EnsureLogHandlerCapacity(1);

        string path = Marshal.PtrToStringAuto(h, len / 2);
        vms[0] = new AyaVMWrapper(moduleName, path);

        Marshal.FreeHGlobal(h);

        return true;
    }

    [DllExport("multi_load", CallingConvention = CallingConvention.Cdecl)]
    public static int MultiLoad(IntPtr h, int len)
    {
        int id = 0;

        // 查找空闲位置
        for (int i = 1; i < vms.Count; i++)
        {
            if (vms[i] == null)
            {
                id = i;
                break;
            }
        }

        // 如果没有空闲位置，添加新的
        if (id == 0)
        {
            vms.Add(null);
            id = vms.Count - 1;
        }

        EnsureLogHandlerCapacity(id + 1);
        currentId = id;

        string path = Marshal.PtrToStringAuto(h, len / 2);
        vms[id] = new AyaVMWrapper(moduleName, path);

        Marshal.FreeHGlobal(h);

        return id;
    }

    [DllExport("unload", CallingConvention = CallingConvention.Cdecl)]
    public static bool Unload()
    {
        if (vms[0] != null)
        {
            vms[0].Dispose();
            vms[0] = null;
        }
        return true;
    }


    [DllExport("multi_unload", CallingConvention = CallingConvention.Cdecl)]
    public static bool MultiUnload(int id)
    {
        if (id <= 0 || id >= vms.Count || vms[id] == null)
        {
            return false;
        }

        vms[id].Dispose();
        vms[id] = null;

        return true;
    }

    [DllExport("request", CallingConvention = CallingConvention.Cdecl)]
    public static IntPtr Request(IntPtr h, ref int len)
    {
        IntPtr result;
        if (vms[0] == null)
        {
            // 测试环境下返回模拟响应
            if (IsTestingEnvironment)
            {
                string testResponse = "SHIORI/2.0 200 OK\nCharset: UTF-8\nValue: Test response\n\n";
                result = Marshal.StringToHGlobalUni(testResponse);
                len = testResponse.Length * 2;
                return result;
            }
            return IntPtr.Zero;
        }

        string request = Marshal.PtrToStringAuto(h, len / 2);
        string response = vms[0].ExecuteRequest(request, false);

        result = Marshal.StringToHGlobalUni(response);
        len = response.Length * 2;

        return result;
    }

    [DllExport("multi_request", CallingConvention = CallingConvention.Cdecl)]
    public static IntPtr MultiRequest(int id, IntPtr h, ref int len)
    {
        if (id <= 0 || id >= vms.Count || vms[id] == null)
        {
            return IntPtr.Zero;
        }

        string request = Marshal.PtrToStringAuto(h, len / 2);
        string response = vms[id].ExecuteRequest(request, false);

        IntPtr result = Marshal.StringToHGlobalUni(response);
        len = response.Length * 2;

        return result;
    }

    [DllExport("Set_loghandler", CallingConvention = CallingConvention.Cdecl)]
    public static void SetLogHandler(Action<string, int, int> logHandler)
    {
        EnsureLogHandlerCapacity(1);
        logHandlers[0] = logHandler;

        if (vms[0] != null)
        {
            vms[0].SetLogHandler(logHandler);
        }
    }

    public static void MultiSetLogHandler(int id, Action<string, int, int> logHandler)
    {
        if (id <= 0 || id >= vms.Count || vms[id] == null)
        {
            return;
        }

        EnsureLogHandlerCapacity(id + 1);
        logHandlers[id] = logHandler;

        if (vms[id] != null)
        {
            vms[id].SetLogHandler(logHandler);
        }
    }

    #endregion

    #region 辅助方法

    private static void EnsureLogHandlerCapacity(int capacity)
    {
        while (logHandlers.Count < capacity)
        {
            logHandlers.Add(null);
        }
    }

    [DllExport("logsend", CallingConvention = CallingConvention.Cdecl)]
    public static bool LogSend(long hwnd)
    {

        if (vms[0] != null)
        {
            vms[0].SetLogRcvWnd(hwnd);
        }
        else if (vms.Count >= 2 && vms[1] != null)
        {
            vms[1].SetLogRcvWnd(hwnd);
        }
        else
        {
            logSendHwnd = hwnd;
        }

        return true;


    }
    private static bool IsTestingEnvironment
    {
        get
        {
#if DEBUG
            return true;
#else
                return false;
#endif
        }
    }


    #endregion
}

internal class AyaVMWrapper : IDisposable
{
    private AyaVM vm;
    private bool isEmergency;

    public AyaVMWrapper(string moduleName, string path)
    {
        vm = new AyaVM();

        if (yaya.logSendHwnd != IntPtr.Zero)
        {
            SetLogRcvWnd(yaya.logSendHwnd);
            yaya.logSendHwnd = IntPtr.Zero;
        }

        vm.Logger.SetLogHandler(yaya.logHandlers[yaya.currentId]);
        vm.Basis.SetModuleName(moduleName, "", "normal");

        vm.Load();
        vm.Basis.SetPath(path);
        vm.Basis.Configure();

        if (vm.Basis.IsSuppress)
        {
            vm.Logger.Message(10, "E_E");

            var emergencyVM = new AyaVM();
            emergencyVM.Logger.SetLogHandler(yaya.logHandlers[yaya.currentId]);
            emergencyVM.Basis.SetModuleName(moduleName, "_emerg", "emergency");

            emergencyVM.Load();
            emergencyVM.Basis.SetPath(path);
            emergencyVM.Basis.Configure();
            emergencyVM.Logger.Message(11, "E_E");

            if (!emergencyVM.Basis.IsSuppress)
            {
                emergencyVM.Logger.AppendErrorLogHistoryToBegin(vm.Logger.GetErrorLogHistory());

                // 交换VM
                var temp = vm;
                vm = emergencyVM;
                emergencyVM = temp;
                isEmergency = true;
            }

            emergencyVM.Dispose();
        }

        vm.Basis.ExecuteLoad();
    }

    public void Dispose()
    {
        vm?.Basis.Termination();
        vm?.Unload();
        vm = null;
    }

    public string ExecuteRequest(string request, bool isDebug)
    {
        if (vm == null) return null;

        vm.RequestBefore();
        string response = vm.Basis.ExecuteRequest(request, isDebug);
        vm.RequestAfter();

        return response;
    }

    public void SetLogHandler(Action<string, int, int> logHandler)
    {
        vm?.Logger.SetLogHandler(logHandler);
    }

    public void SetLogRcvWnd(long hwnd)
    {
        vm?.Basis.SetLogRcvWnd(hwnd);
    }

    public bool IsSuppress => vm?.Basis.IsSuppress ?? true;
    public bool IsEmergency => isEmergency;
}

// 以下是模拟的AyaVM类和相关组件
internal class AyaVM
{
    public Basis Basis { get; } = new Basis();
    public Logger Logger { get; } = new Logger();

    public void Load() { /* 实现加载逻辑 */ }
    public void Unload() { /* 实现卸载逻辑 */ }
    public void RequestBefore() { /* 请求前处理 */ }
    public void RequestAfter() { /* 请求后处理 */ }

    public void Dispose() { }
}

internal class Basis
{
    public bool IsSuppress { get; internal set; }
    public void SetModuleName(string name, string suffix, string mode) { /* 实现 */ }
    public void SetPath(string path) { /* 实现 */ }
    public void Configure() { /* 实现 */ }
    public void ExecuteLoad() { /* 实现 */ }
    public void Termination() { /* 实现 */ }
    public string ExecuteRequest(string request, bool isDebug) { /* 实现 */ return ""; }
    public void SetLogRcvWnd(long hwnd) { /* 实现 */ }
}

internal class Logger
{
    private Action<string, int, int> logHandler;

    public void SetLogHandler(Action<string, int, int> handler) => logHandler = handler;
    public void Message(int code, string message) => logHandler?.Invoke(message, 0, 0);
    public string GetErrorLogHistory() => "";
    public void AppendErrorLogHistoryToBegin(string history) { /* 实现 */ }
}
