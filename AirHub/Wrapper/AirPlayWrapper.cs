using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace AirHub.Wrapper
{
    [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
    public delegate void AirplayConnect(string session, string name, string address, int state, IntPtr customParam);

    [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
    public delegate void AirplayDisConnect(string session, IntPtr customParam);

    internal class AirPlayWrapper
    {
        private const string CppDLL = @"AirPlay1.dll";

        [DllImport(CppDLL, EntryPoint = "#1", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool Init();

        [DllImport(CppDLL, EntryPoint = "#2", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool Start(string srvName, string pinCode, AirplayConnect connectFun, AirplayDisConnect disConnectFun, IntPtr customParam);

        [DllImport(CppDLL, EntryPoint = "#3", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool Stop();

        [DllImport(CppDLL, EntryPoint = "#6", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool SetPinCode(string pinCode);

        [DllImport(CppDLL, EntryPoint = "#7", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool SetLock(bool isLock);

        [DllImport(CppDLL, EntryPoint = "#8", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool Show(string session, bool show);

        [DllImport(CppDLL, EntryPoint = "#9", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool Mute(string session, bool mute);

        [DllImport(CppDLL, EntryPoint = "#10", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool Close(string session);

        [DllImport(CppDLL, EntryPoint = "#11", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool GetSurface9(string session, out IntPtr pSurface, out int width, out int height, out long updateTime);

        [DllImport(CppDLL, EntryPoint = "#12", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public static extern bool IsMute(string session);

        [DllImport(CppDLL, EntryPoint = "#13", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public static extern bool SetVolume(string session, int volume);

        [DllImport(CppDLL, EntryPoint = "#14", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public static extern int GetVolume(string session);
    }
} 
