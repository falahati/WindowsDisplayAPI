using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using WindowsDisplayAPI.Exceptions;

namespace WindowsDisplayAPI
{
    /// <summary>
    ///     Represents a Windows Attached Display Device
    /// </summary>
    public class Display : DisplayDevice
    {
        /// <summary>
        ///     Creates a new Display
        /// </summary>
        /// <param name="device">The DisplayDevice instance to copy information from</param>
        protected Display(DisplayDevice device)
            : base(device.DevicePath, device.DeviceName, device.DeviceKey, device.Adapter, device.IsAvailable, false)
        {
        }

        /// <summary>
        ///     Gets a DisplaySetting object representing the display current settings
        /// </summary>
        public DisplaySetting CurrentSetting => new DisplaySetting(this, true);

        /// <inheritdoc />
        public override string DisplayFullName
        {
            get
            {
                if (IsValid)
                    return
                        DisplayAdapter.GetDisplayAdapters()
                            .SelectMany(adapter => adapter.GetDisplayDevices(base.IsAvailable))
                            .FirstOrDefault(
                                device => device.DevicePath.Equals(DevicePath) && device.DeviceKey.Equals(DeviceKey))?
                            .DisplayFullName;
                return ToUnAttachedDisplay()?.DisplayFullName;
            }
        }

        /// <inheritdoc />
        public override string DisplayName
        {
            get
            {
                if (IsValid)
                    return
                        DisplayAdapter.GetDisplayAdapters()
                            .SelectMany(adapter => adapter.GetDisplayDevices(base.IsAvailable))
                            .FirstOrDefault(
                                device => device.DevicePath.Equals(DevicePath) && device.DeviceKey.Equals(DeviceKey))?
                            .DisplayName;
                return ToUnAttachedDisplay()?.DisplayName;
            }
        }

        /// <inheritdoc />
        public override bool IsAvailable => base.IsAvailable && IsValid;

        /// <summary>
        ///     Gets a boolean value indicating if this display device is the Windows GDI primary device
        /// </summary>
        public bool IsGDIPrimary
            =>
            CurrentSetting.IsEnable && (CurrentSetting.Position.X == Point.Empty.X) &&
            (CurrentSetting.Position.Y == Point.Empty.Y);


        /// <inheritdoc />
        public override bool IsValid
        {
            get
            {
                return
                    DisplayAdapter.GetDisplayAdapters()
                        .SelectMany(adapter => adapter.GetDisplayDevices(base.IsAvailable))
                        .Any(
                            device =>
                                device.DevicePath.Equals(DevicePath) && device.DeviceKey.Equals(DeviceKey) &&
                                device.IsAvailable);
            }
        }

        /// <summary>
        ///     Gets a DisplaySettings object representing this display saved settings
        /// </summary>
        public DisplaySetting SavedSetting => new DisplaySetting(this, false);

        /// <summary>
        ///     Returns a list of all attached displays on this machine
        /// </summary>
        /// <returns>An enumerable list of Displays</returns>
        public static IEnumerable<Display> GetDisplays()
        {
            return
                DisplayAdapter.GetDisplayAdapters()
                    .SelectMany(adapter => adapter.GetDisplayDevices(true))
                    .Where(device => device.IsAvailable)
                    .Select(device => new Display(device));
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return IsValid ? $"{GetType().Name}: {DisplayFullName} ({DeviceName})" : $"{GetType().Name}: Invalid";
        }

        /// <summary>
        ///     Disables and detaches this display device
        /// </summary>
        /// <param name="apply">Indicating if the changes should be applied immediately, recommended value is false</param>
        public void Disable(bool apply)
        {
            SetSettings(new DisplaySetting(), apply);
        }

        /// <summary>
        ///     Returns the corresponding Screen instance for this display device
        /// </summary>
        /// <returns>A Screen object</returns>
        public Screen GetScreen()
        {
            if (!IsValid)
                throw new InvalidDisplayException(DevicePath);
            try
            {
                return Screen.AllScreens.FirstOrDefault(screen => screen.DeviceName.Equals(DisplayName));
            }
            catch
            {
                // ignored
            }
            return null;
        }

        /// <summary>
        ///     Changes the display device settings to a new DisplaySettings object
        /// </summary>
        /// <param name="displaySetting">The display settings that should be applied</param>
        /// <param name="apply">Indicating if the changes should be applied immediately, recommended value is false</param>
        public void SetSettings(DisplaySetting displaySetting, bool apply = false)
        {
            if (!IsValid)
                throw new InvalidDisplayException(DevicePath);
            displaySetting.Save(this, apply);
        }

        /// <summary>
        ///     Returns the corresponding UnAttachedDisplay device for this display. Only valid when this instance is invalidated
        ///     due to display detachment.
        /// </summary>
        /// <returns></returns>
        public UnAttachedDisplay ToUnAttachedDisplay()
        {
            if (IsValid)
                return null;
            return
                UnAttachedDisplay.GetUnAttachedDisplays()
                    .FirstOrDefault(
                        display => display.DevicePath.Equals(DevicePath) && display.DeviceKey.Equals(DeviceKey));
        }
    }
}