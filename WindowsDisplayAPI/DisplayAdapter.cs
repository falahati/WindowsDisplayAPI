using System.Collections.Generic;
using System.Linq;
using WindowsDisplayAPI.DisplayConfig;
using WindowsDisplayAPI.Native;
using WindowsDisplayAPI.Native.DeviceContext;

namespace WindowsDisplayAPI
{
    /// <summary>
    ///     Represents a Windows Video Controller Display Adapter Device
    /// </summary>
    public class DisplayAdapter : Device
    {
        /// <summary>
        ///     Creates a new DisplayAdapter
        /// </summary>
        /// <param name="devicePath">The device path</param>
        /// <param name="deviceName">The device name</param>
        /// <param name="deviceKey">The device driver registry key</param>
        protected DisplayAdapter(string devicePath, string deviceName, string deviceKey)
            : base(devicePath, deviceName, deviceKey)
        {
        }

        /// <summary>
        ///     Returns a list of all display adapters on this machine
        /// </summary>
        /// <returns>An enumerable list of DisplayAdapters</returns>
        public static IEnumerable<DisplayAdapter> GetDisplayAdapters()
        {
            var device = Native.DeviceContext.Structures.DisplayDevice.Initialize();
            var deviceIds = new List<string>();
            for (uint i = 0; DeviceContextApi.EnumDisplayDevices(null, i, ref device, 0); i++)
            {
                if (!deviceIds.Contains(device.DeviceId))
                {
                    deviceIds.Add(device.DeviceId);
                    yield return new DisplayAdapter(device.DeviceId, device.DeviceString, device.DeviceKey);
                }
                device = Native.DeviceContext.Structures.DisplayDevice.Initialize();
            }
        }

        /// <summary>
        ///     Returns a list of all display devices connected to this adapter
        /// </summary>
        /// <returns>An enumerable list of DisplayDevices</returns>
        public IEnumerable<DisplayDevice> GetDisplayDevices()
        {
            return GetDisplayDevices(null);
        }

        /// <summary>
        ///     Returns the corresponding PathDisplayAdapter instance
        /// </summary>
        /// <returns>An instance of PathDisplayAdapter, or null</returns>
        public PathDisplayAdapter ToPathDisplayAdapter()
        {
            return
                PathDisplayAdapter.GetAdapters()
                    .FirstOrDefault(adapter => adapter.DevicePath.StartsWith("\\\\?\\" + DevicePath.Replace("\\", "#")));
        }

        internal IEnumerable<DisplayDevice> GetDisplayDevices(bool? filterByValidity)
        {
            var device = Native.DeviceContext.Structures.DisplayDevice.Initialize();
            var returned = new Dictionary<string, string>();
            for (uint i = 0; DeviceContextApi.EnumDisplayDevices(null, i, ref device, 0); i++)
            {
                if (DevicePath.Equals(device.DeviceId))
                {
                    DisplayDevice displayDevice = null;
                    var display = Native.DeviceContext.Structures.DisplayDevice.Initialize();
                    for (uint id = 0; DeviceContextApi.EnumDisplayDevices(device.DeviceName, id, ref display, 1); id++)
                    {
                        var isAttached = display.StateFlags.HasFlag(DisplayDeviceStateFlags.AttachedToDesktop);
                        if (!filterByValidity.HasValue)
                        {
                            yield return DisplayDevice.FromDeviceInformation(this, device, display);
                        }
                        else if (filterByValidity.Value && isAttached)
                        {
                            if (!returned.ContainsKey(display.DeviceId) ||
                                !returned[display.DeviceId].Equals(display.DeviceKey))
                            {
                                returned.Add(display.DeviceId, display.DeviceKey);
                                yield return DisplayDevice.FromDeviceInformation(this, device, display);
                                break;
                            }
                        }
                        else if (!filterByValidity.Value)
                        {
                            if (!isAttached)
                            {
                                if (!returned.ContainsKey(display.DeviceId) ||
                                    !returned[display.DeviceId].Equals(display.DeviceKey))
                                    displayDevice = DisplayDevice.FromDeviceInformation(this, device, display);
                            }
                            else
                            {
                                displayDevice = null;
                                break;
                            }
                        }
                        display = Native.DeviceContext.Structures.DisplayDevice.Initialize();
                    }
                    if (displayDevice != null)
                    {
                        returned.Add(displayDevice.DevicePath, displayDevice.DeviceKey);
                        yield return displayDevice;
                    }
                }
                device = Native.DeviceContext.Structures.DisplayDevice.Initialize();
            }
        }
    }
}