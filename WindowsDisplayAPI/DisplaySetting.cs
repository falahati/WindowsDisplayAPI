using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using WindowsDisplayAPI.Exceptions;
using WindowsDisplayAPI.Native;
using WindowsDisplayAPI.Native.DeviceContext;
using WindowsDisplayAPI.Native.DeviceContext.Structures;
using WindowsDisplayAPI.Native.Structures;

namespace WindowsDisplayAPI
{
    /// <summary>
    ///     Holds configurations of a windows display
    /// </summary>
    public class DisplaySetting : DisplayPossibleSetting
    {
        /// <summary>
        ///     Creates a new DisplaySetting
        /// </summary>
        /// <param name="validSetting">The basic configuration information object</param>
        /// <param name="position">Display position on desktop</param>
        public DisplaySetting(DisplayPossibleSetting validSetting, Point position = default(Point))
            : this(validSetting, position, DisplayOrientation.Identity, DisplayFixedOutput.Default)
        {
        }

        /// <summary>
        ///     Creates a new DisplaySetting
        /// </summary>
        /// <param name="validSetting">The basic configuration information object</param>
        /// <param name="position">Display position on desktop</param>
        /// <param name="orientation">Display orientation and rotation</param>
        /// <param name="outputScalingMode">
        ///     Display output behavior in case of presenting a low-resolution mode on a
        ///     higher-resolution display
        /// </param>
        public DisplaySetting(DisplayPossibleSetting validSetting,
            Point position,
            DisplayOrientation orientation,
            DisplayFixedOutput outputScalingMode)
            : this(
                validSetting.Resolution, position, validSetting.ColorDepth, validSetting.Frequency,
                validSetting.IsInterlaced, orientation, outputScalingMode)
        {
        }

        /// <summary>
        ///     Creates a new DisplaySetting
        /// </summary>
        /// <param name="resolution">Display resolution</param>
        /// <param name="position">Display position on desktop</param>
        /// <param name="frequency">Display frequency</param>
        public DisplaySetting(Size resolution, Point position, int frequency)
            : this(resolution, position, ColorDepth.Depth32Bit, frequency)
        {
        }

        /// <summary>
        ///     Creates a new DisplaySetting
        /// </summary>
        /// <param name="resolution">Display resolution</param>
        /// <param name="frequency">Display frequency</param>
        public DisplaySetting(Size resolution, int frequency)
            : this(resolution, new Point(0, 0), ColorDepth.Depth32Bit, frequency)
        {
        }

        /// <summary>
        ///     Creates a new DisplaySetting
        /// </summary>
        /// <param name="resolution">Display resolution</param>
        /// <param name="position">Display position on desktop</param>
        /// <param name="frequency">Display frequency</param>
        /// <param name="colorDepth">Display color depth</param>
        /// <param name="isInterlaced">Indicating if display is using interlaces scan out</param>
        /// <param name="orientation">Display orientation and rotation</param>
        /// <param name="outputScalingMode">
        ///     Display output behavior in case of presenting a low-resolution mode on a
        ///     higher-resolution display
        /// </param>
        public DisplaySetting(Size resolution, Point position, ColorDepth colorDepth, int frequency,
            bool isInterlaced = false, DisplayOrientation orientation = DisplayOrientation.Identity,
            DisplayFixedOutput outputScalingMode = DisplayFixedOutput.Default
        ) : base(resolution, frequency, colorDepth, isInterlaced)
        {
            Position = position;
            Orientation = orientation;
            OutputScalingMode = outputScalingMode;
        }

        internal DisplaySetting(DisplayDevice display, bool current) : this(GetDeviceMode(display, current))
        {
        }

        internal DisplaySetting() : base(default(DeviceMode))
        {
            IsEnable = false;
        }

        private DisplaySetting(DeviceMode deviceMode) : base(deviceMode)
        {
            Position = new Point(deviceMode.Position.X, deviceMode.Position.Y);
            Orientation = deviceMode.DisplayOrientation;
            OutputScalingMode = deviceMode.DisplayFixedOutput;

            if (Resolution.IsEmpty && Position.IsEmpty)
                IsEnable = false;
        }


        /// <summary>
        ///     Gets the
        /// </summary>
        public bool IsEnable { get; } = true;

        /// <summary>
        ///     Gets or sets the orientation of the display monitor
        /// </summary>
        public DisplayOrientation Orientation { get; }

        /// <summary>
        ///     Gets output behavior in case of presenting a low-resolution mode on a higher-resolution display.
        /// </summary>
        public DisplayFixedOutput OutputScalingMode { get; }

        /// <summary>
        ///     Gets or sets the position of the display monitor
        /// </summary>
        public Point Position { get; }

        /// <summary>
        ///     Applies settings that are saved using SaveDisplaySettings() or other similar methods but not yet applied
        /// </summary>
        public static void ApplySavedSettings()
        {
            var result = DeviceContextApi.ChangeDisplaySettingsEx(
                null,
                IntPtr.Zero,
                IntPtr.Zero,
                ChangeDisplaySettingsFlags.Reset,
                IntPtr.Zero);
            if (result != ChangeDisplaySettingsExResults.Successful)
                throw new ModeChangeException($"[{result}]: Applying saved settings failed.", null, result);
        }

        /// <summary>
        ///     Sets and possibility applies a list of display settings
        /// </summary>
        /// <param name="newDisplaySettings">A key value dictionary of DisplayDevices and DisplaySettings</param>
        /// <param name="applyNow">Indicating if the changes should be applied immediately, recommended value is false</param>
        public static void SaveDisplaySettings(Dictionary<DisplayDevice, DisplaySetting> newDisplaySettings,
            bool applyNow)
        {
            SaveDisplaySettings(newDisplaySettings, applyNow, true);
        }

        private static DeviceMode GetDeviceMode(DisplayDevice display, bool current)
        {
            var deviceMode = new DeviceMode(DeviceModeFields.None);
            return !string.IsNullOrWhiteSpace(display.DisplayName) &&
                   DeviceContextApi.EnumDisplaySettings(display.DisplayName,
                       current ? DisplaySettingsMode.CurrentSettings : DisplaySettingsMode.RegistrySettings,
                       ref deviceMode)
                ? deviceMode
                : default(DeviceMode);
        }

        private static void SaveDisplaySettings(Dictionary<DisplayDevice, DisplaySetting> newDisplaySettings,
            bool applyNow, bool retry)
        {
            var rollBackState =
                Display.GetDisplays()
                    .Where(display => display.IsValid)
                    .ToDictionary(display => (DisplayDevice) display, display => display.CurrentSetting);
            try
            {
                var currentDisplaySettings = rollBackState.ToList();
                foreach (var displaySetting in newDisplaySettings)
                {
                    currentDisplaySettings.Remove(
                        currentDisplaySettings.FirstOrDefault(
                            ex => ex.Key.DevicePath.Equals(displaySetting.Key.DevicePath)));
                    displaySetting.Value.Save(displaySetting.Key, false);
                }

                // Disable missing monitors
                foreach (
                    var displaySetting in
                    currentDisplaySettings.Where(pair => pair.Key is Display)
                        .Select(pair => pair.Key as Display)
                        .Where(display => display?.IsValid == true))
                    displaySetting.Disable(false);

                if (applyNow)
                    ApplySavedSettings();
            }
            catch (ModeChangeException)
            {
                if (retry)
                    SaveDisplaySettings(rollBackState, false, false);
                throw;
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            if (IsEnable)
                return
                    $"{Resolution} {(IsInterlaced ? "Interlaced" : "Progressive")} {Frequency}hz @ {ColorDepth} @ {Position}";
            return "Disabled";
        }

        internal void Save(DisplayDevice display, bool reset)
        {
            var deviceMode = GetDeviceMode(display);
            var flags = ChangeDisplaySettingsFlags.UpdateRegistry | ChangeDisplaySettingsFlags.Global;
            flags |= reset ? ChangeDisplaySettingsFlags.Reset : ChangeDisplaySettingsFlags.NoReset;
            if (IsEnable && (Position.X == 0) && (Position.Y == 0))
                flags |= ChangeDisplaySettingsFlags.SetPrimary;
            var result = DeviceContextApi.ChangeDisplaySettingsEx(display.DisplayName, ref deviceMode, IntPtr.Zero,
                flags,
                IntPtr.Zero);
            if (result != ChangeDisplaySettingsExResults.Successful)
                throw new ModeChangeException($"[{result}]: Applying saved settings failed.", display, result);
        }

        private DeviceMode GetDeviceMode(DisplayDevice display)
        {
            DeviceMode deviceMode;
            if (IsEnable)
            {
                var flags = DisplayFlags.None;
                if (IsInterlaced)
                    flags |= DisplayFlags.Interlaced;
                deviceMode = new DeviceMode(display.DisplayName, new PointL(Position), Orientation, OutputScalingMode,
                    (uint) ColorDepth, (uint) Resolution.Width, (uint) Resolution.Height, flags, (uint) Frequency);
            }
            else
            {
                deviceMode = new DeviceMode(display.DisplayName,
                    DeviceModeFields.PelsWidth | DeviceModeFields.PelsHeight | DeviceModeFields.Position);
            }
            if (string.IsNullOrWhiteSpace(deviceMode.DeviceName))
                throw new MissingDisplayException("Display device is missing or invalid.", display.DevicePath);
            return deviceMode;
        }
    }
}