using System.Runtime.InteropServices;
using WindowsDisplayAPI.Native.Structures;

namespace WindowsDisplayAPI.Native.DisplayConfig.Structures
{
    // Internal undocumented structure
    [StructLayout(LayoutKind.Sequential)]
    internal struct DisplayConfigGetSourceDPIScale
    {
        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        [MarshalAs(UnmanagedType.Struct)] private readonly DisplayConfigDeviceInfoHeader _Header;
        [MarshalAs(UnmanagedType.U4)] private readonly int _MinimumScaleSteps;
        [MarshalAs(UnmanagedType.U4)] private readonly int _CurrentScaleSteps;
        [MarshalAs(UnmanagedType.U4)] private readonly int _MaximumScaleSteps;

        public int MinimumScaleSteps => _MinimumScaleSteps;
        public int CurrentScaleSteps => _CurrentScaleSteps;
        public int MaximumScaleSteps => _MaximumScaleSteps;

        public DisplayConfigGetSourceDPIScale(LUID adapter, uint sourceId) : this()
        {
            _Header = new DisplayConfigDeviceInfoHeader(adapter, sourceId, GetType());
        }
    }
}