using System.Runtime.InteropServices;
using WindowsDisplayAPI.Native.Structures;

namespace WindowsDisplayAPI.Native.DisplayConfig.Structures
{
    // Internal undocumented structure
    [StructLayout(LayoutKind.Sequential)]
    internal struct DisplayConfigSetSourceDPIScale
    {
        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        [MarshalAs(UnmanagedType.Struct)] private readonly DisplayConfigDeviceInfoHeader _Header;
        [MarshalAs(UnmanagedType.U4)] private readonly int _ScaleSteps;

        public int ScaleSteps => _ScaleSteps;

        public DisplayConfigSetSourceDPIScale(LUID adapter, uint sourceId, int scaleSteps) : this()
        {
            _Header = new DisplayConfigDeviceInfoHeader(adapter, sourceId, GetType());
            _ScaleSteps = scaleSteps;
        }
    }
}