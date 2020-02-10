using System.Collections.Generic;
using System.Linq;
using WindowsDisplayAPI.DisplayConfig;
using WindowsDisplayAPI.Native;
using WindowsDisplayAPI.Native.DeviceContext;
using WindowsDisplayAPI.Native.DeviceContext.Structures;

namespace WindowsDisplayAPI
{
    /// <summary>
    ///     Represents a Windows Display Device
    /// </summary>
    public class DisplayDevice : Device
    {
        /// <summary>
        ///     Creates a new DisplayDevice
        /// </summary>
        /// <param name="devicePath">The device path</param>
        /// <param name="deviceName">The device name</param>
        /// <param name="deviceKey">The device driver registry key</param>
        protected DisplayDevice(string devicePath, string deviceName, string deviceKey)
            : base(devicePath, deviceName, deviceKey)
        {
        }

        /// <summary>
        ///     Creates a new DisplayDevice
        /// </summary>
        /// <param name="devicePath">The device path</param>
        /// <param name="deviceName">The device name</param>
        /// <param name="deviceKey">The device driver registry key</param>
        /// <param name="adapter">The device parent DisplayAdapter</param>
        /// <param name="isAvailable">true if the device is attached, otherwise false</param>
        /// <param name="isValid">true if this instance is valid, otherwise false</param>
        protected DisplayDevice(string devicePath, string deviceName, string deviceKey, DisplayAdapter adapter,
            bool isAvailable, bool isValid)
            : this(devicePath, deviceName, deviceKey)
        {
            Adapter = adapter;
            IsAvailable = isAvailable;
            IsValid = isValid;
        }

        /// <summary>
        ///     Creates a new DisplayDevice
        /// </summary>
        /// <param name="devicePath">The device path</param>
        /// <param name="deviceName">The device name</param>
        /// <param name="deviceKey">The device driver registry key</param>
        /// <param name="adapter">The device parent DisplayAdapter</param>
        /// <param name="displayName">The device source display name</param>
        /// <param name="displayFullName">The device target display name</param>
        /// <param name="isAvailable">true if the device is attached, otherwise false</param>
        /// <param name="isValid">true if this instance is valid, otherwise false</param>
        protected DisplayDevice(string devicePath, string deviceName, string deviceKey, DisplayAdapter adapter,
            string displayName, string displayFullName,
            bool isAvailable, bool isValid)
            : this(devicePath, deviceName, deviceKey, adapter, isAvailable, isValid)
        {
            DisplayName = displayName;
            DisplayFullName = displayFullName;
        }

        /// <summary>
        ///     Gets the display device driving display adapter instance
        /// </summary>
        public virtual DisplayAdapter Adapter { get; }

        /// <summary>
        ///     Gets the display device target name
        /// </summary>
        public virtual string DisplayFullName { get; }

        /// <summary>
        ///     Gets the display device source name
        /// </summary>
        public virtual string DisplayName { get; }

        /// <summary>
        ///     Gets a boolean value indicating if this display device is currently attached
        /// </summary>
        public virtual bool IsAvailable { get; }

        /// <summary>
        ///     Gets a boolean value indicating if this instance is no longer valid, this may happen when display device attached
        ///     status changes
        /// </summary>
        public virtual bool IsValid { get; }

        internal static DisplayDevice FromDeviceInformation(DisplayAdapter adapter,
            Native.DeviceContext.Structures.DisplayDevice sourceDevice,
            Native.DeviceContext.Structures.DisplayDevice targetDevice)
        {
            return new DisplayDevice(
                targetDevice.DeviceId,
                targetDevice.DeviceString,
                targetDevice.DeviceKey,
                adapter,
                sourceDevice.DeviceName,
                targetDevice.DeviceName,
                targetDevice.StateFlags.HasFlag(DisplayDeviceStateFlags.AttachedToDesktop),
                true
            );
        }


        /// <inheritdoc />
        public override string ToString()
        {
            return string.IsNullOrWhiteSpace(DeviceName)
                ? $"{GetType().Name}: {DisplayFullName} - IsAvailable: {IsAvailable}"
                : $"{GetType().Name}: {DisplayFullName} ({DeviceName}) - IsAvailable: {IsAvailable}";
        }

        /// <summary>
        ///     Returns a list of possible display setting for this display device
        /// </summary>
        /// <returns>An enumerable list of DisplayPossibleSettings</returns>
        public IEnumerable<DisplayPossibleSetting> GetPossibleSettings()
        {
            if (IsValid)
            {
                var deviceMode = new DeviceMode(DeviceModeFields.None);
                for (var i = 0;
                    DeviceContextApi.EnumDisplaySettings(DisplayName, (DisplaySettingsMode) i, ref deviceMode);
                    i++)
                {
                    yield return new DisplayPossibleSetting(deviceMode);
                    deviceMode = new DeviceMode(DeviceModeFields.None);
                }
            }
        }

        /// <summary>
        ///     Returns the best possible display setting for this display device
        /// </summary>
        /// <returns>A DisplayPossibleSetting instance</returns>
        public DisplayPossibleSetting GetPreferredSetting()
        {
            if (IsValid)
                return
                    GetPossibleSettings()
                        .OrderByDescending(setting => (int) setting.ColorDepth)
                        .ThenByDescending(setting => (ulong) setting.Resolution.Width*(ulong) setting.Resolution.Height)
                        .ThenByDescending(setting => setting.Frequency).FirstOrDefault();
            return null;
        }

        /// <summary>
        ///     Returns the corresponding PathDisplaySource instance
        /// </summary>
        /// <returns>An instance of PathDisplaySource, or null</returns>
        public PathDisplaySource ToPathDisplaySource()
        {
            return PathDisplaySource.GetDisplaySources()
                .FirstOrDefault(source => source.DisplayName.Equals(DisplayName));
        }

        /// <summary>
        ///     Returns the corresponding PathDisplayTarget instance
        /// </summary>
        /// <returns>An instance of PathDisplayTarget, or null</returns>
        public PathDisplayTarget ToPathDisplayTarget()
        {
            return PathDisplayTarget.GetDisplayTargets()
                .FirstOrDefault(target => target.DevicePath.Equals(DevicePath));
        }
    }
}