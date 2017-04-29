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
    }
}