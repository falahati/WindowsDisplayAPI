using System;
using System.Runtime.InteropServices;
using WindowsDisplayAPI.Native.DeviceContext;
using WindowsDisplayAPI.Native.DeviceContext.Structures;

namespace WindowsDisplayAPI.Native
{
    internal class DeviceContextApi
    {
        [DllImport("user32")]
        public static extern ChangeDisplaySettingsExResults ChangeDisplaySettingsEx(
            string deviceName,
            ref DeviceMode devMode,
            IntPtr handler,
            ChangeDisplaySettingsFlags flags,
            IntPtr param);

        [DllImport("user32")]
        public static extern ChangeDisplaySettingsExResults ChangeDisplaySettingsEx(
            string deviceName,
            IntPtr devModePointer,
            IntPtr handler,
            ChangeDisplaySettingsFlags flags,
            IntPtr param);

        [DllImport("user32")]
        public static extern bool EnumDisplaySettings(
            string deviceName,
            DisplaySettingsMode mode,
            ref DeviceMode devMode);

        [DllImport("user32", CharSet = CharSet.Ansi)]
        internal static extern bool EnumDisplayDevices(
            string deviceName,
            uint deviceNumber,
            ref DeviceContext.Structures.DisplayDevice displayDevice,
            uint flags);
    }
}