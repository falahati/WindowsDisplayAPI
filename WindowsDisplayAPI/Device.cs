using System;
using WindowsDisplayAPI.Exceptions;
using Microsoft.Win32;

namespace WindowsDisplayAPI
{
    /// <summary>
    ///     Represents a Windows Video Device including Display Devices and Video Controllers
    /// </summary>
    public abstract class Device
    {
        /// <summary>
        ///     Creates a new Device
        /// </summary>
        /// <param name="devicePath">The device path</param>
        /// <param name="deviceName">The device name</param>
        /// <param name="deviceKey">The device driver registry key</param>
        protected Device(string devicePath, string deviceName, string deviceKey)
        {
            DevicePath = devicePath;
            DeviceName = deviceName;
            DeviceKey = deviceKey;
        }

        /// <summary>
        ///     Gets the registry address of the device driver and configuration
        /// </summary>
        public virtual string DeviceKey { get; }

        /// <summary>
        ///     Gets the Windows device name
        /// </summary>
        public virtual string DeviceName { get; }

        /// <summary>
        ///     Gets the Windows device path
        /// </summary>
        public virtual string DevicePath { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{GetType().Name}: {DeviceName}";
        }

        /// <summary>
        ///     Opens the registry key at the address specified by the DeviceKey property
        /// </summary>
        /// <returns>A RegistryKey instance for successful call, otherwise null</returns>
        /// <exception cref="InvalidRegistryAddressException">Registry address is invalid or unknown.</exception>
        public RegistryKey OpenDeviceKey()
        {
            if (string.IsNullOrWhiteSpace(DeviceKey))
                return null;
            const string machineRootName = "\\Registry\\Machine\\";
            const string userRootName = "\\Registry\\Machine\\";
            if (DeviceKey.StartsWith(machineRootName, StringComparison.InvariantCultureIgnoreCase))
                return Registry.LocalMachine.OpenSubKey(DeviceKey.Substring(machineRootName.Length),
                    RegistryKeyPermissionCheck.ReadSubTree);
            if (DeviceKey.StartsWith(userRootName, StringComparison.InvariantCultureIgnoreCase))
                return Registry.Users.OpenSubKey(DeviceKey.Substring(machineRootName.Length),
                    RegistryKeyPermissionCheck.ReadSubTree);
            throw new InvalidRegistryAddressException("Registry address is invalid or unknown.");
        }

        /// <summary>
        ///     Opens the registry key of the Windows PnP manager for this device
        /// </summary>
        /// <returns>A RegistryKey instance for successful call, otherwise null</returns>
        public RegistryKey OpenDevicePnPKey()
        {
            if (string.IsNullOrWhiteSpace(DevicePath))
                return null;
            var path = DevicePath;
            if (path.StartsWith("\\\\?\\"))
            {
                path = path.Substring(4).Replace("#", "\\");
                if (path.EndsWith("}"))
                {
                    var guidIndex = path.LastIndexOf("{", StringComparison.InvariantCulture);
                    if (guidIndex > 0)
                        path = path.Substring(0, guidIndex);
                }
            }
            return Registry.LocalMachine.OpenSubKey("SYSTEM\\CurrentControlSet\\Enum\\" + path,
                RegistryKeyPermissionCheck.ReadSubTree);
        }
    }
}