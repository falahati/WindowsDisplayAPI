using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using WindowsDisplayAPI;
using WindowsDisplayAPI.DisplayConfig;
using WindowsDisplayAPI.Native.DisplayConfig;

namespace WindowsDisplaySample
{
    internal class Program
    {
        private static void DeviceContext()
        {
            var navigation = new Dictionary<object, Action>
            {
                {
                    "DeviceContext: Display Adapters",
                    () =>
                        ConsoleNavigation.PrintObject(
                            DisplayAdapter.GetDisplayAdapters().ToArray(),
                            display =>
                                ConsoleNavigation.PrintObject(
                                    display.GetDisplayDevices(),
                                    "Display.GetDisplayDevices()"
                                ),
                            "DisplayAdapter.GetDisplayAdapters()",
                            "Select an adapter to show connected devices."
                        )
                },
                {
                    "DeviceContext: Connected Displays",
                    () =>
                        ConsoleNavigation.PrintObject(
                            Display.GetDisplays().ToArray(),
                            display => ConsoleNavigation.PrintObject(
                                display.DisplayScreen.CurrentSetting,
                                () =>
                                {
                                    ConsoleNavigation.PrintObject(
                                        display.DisplayScreen.GetPossibleSettings()
                                            .OrderByDescending(
                                                setting => (ulong) setting.Resolution.Height *
                                                           (ulong) setting.Resolution.Width)
                                            .Take(10)
                                            .ToArray(),
                                        setting =>
                                        {
                                            display.DisplayScreen.SetSettings(new DisplaySetting(setting), true);
                                        },
                                        "Display.GetValidSettings()",
                                        "Select a display setting to apply and enable."
                                    );
                                },
                                "Display.CurrentSetting",
                                ""),
                            "Display.GetDisplays()",
                            "Select a display to show current settings."
                        )
                },
                {
                    "DeviceContext: Disconnected Displays",
                    () =>
                        ConsoleNavigation.PrintObject(
                            UnAttachedDisplay.GetUnAttachedDisplays().ToArray(),
                            display =>
                            {
                                ConsoleNavigation.PrintObject(
                                    display.DisplayScreen.GetPossibleSettings()
                                        .OrderByDescending(
                                            setting => (ulong) setting.Resolution.Height *
                                                       (ulong) setting.Resolution.Width)
                                        .Take(10)
                                        .ToArray(),
                                    setting =>
                                    {
                                        display.DisplayScreen.Enable(new DisplaySetting(setting));
                                        DisplaySetting.ApplySavedSettings();
                                    },
                                    "Display.GetValidSettings()",
                                    "Select a display setting to apply and enable.");
                            },
                            "UnAttachedDisplay.GetUnAttachedDisplays()",
                            "Select an unattached display to show possible display settings to activate."
                        )
                },
                {
                    "DeviceContext: Disable All Except Primary",
                    () =>
                    {
                        var displays = Display.GetDisplays().ToArray();

                        foreach (var display in displays.Where(display => !display.DisplayScreen.IsPrimary))
                        {
                            display.DisplayScreen.Disable(false);
                        }

                        DisplaySetting.ApplySavedSettings();
                    }
                },
                {
                    "DeviceContext: Enable All",
                    () =>
                    {
                        var startPosition = Display.GetDisplays()
                            .Max(
                                display => display.DisplayScreen.CurrentSetting.Position.X +
                                           display.DisplayScreen.CurrentSetting.Resolution.Width
                            );

                        var displays = UnAttachedDisplay.GetUnAttachedDisplays().ToArray();

                        foreach (var display in displays)
                        {
                            var validSetting = display.DisplayScreen.GetPreferredSetting();
                            var placedSettings = new DisplaySetting(validSetting, new Point(startPosition, 0));
                            startPosition += validSetting.Resolution.Width;
                            display.DisplayScreen.Enable(placedSettings, true);
                        }

                        DisplaySetting.ApplySavedSettings();
                    }
                }
            };
            ConsoleNavigation.PrintNavigation(
                navigation,
                "DeviceContext functions",
                "Select a DeviceContext sample."
            );
        }

        private static void DisplayConfig()
        {
            var navigation = new Dictionary<object, Action>
            {
                {
                    "DisplayConfig: Display Adapters",
                    () =>
                    {
                        ConsoleNavigation.PrintObject(
                            PathDisplayAdapter.GetAdapters(),
                            "PathDisplayAdapter.GetAdapters()"
                        );
                    }
                },
                {
                    "DisplayConfig: Display Sources",
                    () =>
                    {
                        ConsoleNavigation.PrintObject(
                            PathDisplaySource.GetDisplaySources(),
                            "PathDisplaySource.GetDisplaySources()"
                        );
                    }
                },
                {
                    "DisplayConfig: Display Targets",
                    () =>
                    {
                        ConsoleNavigation.PrintObject(
                            PathDisplayTarget.GetDisplayTargets(),
                            "PathDisplayTarget.GetDisplayTargets()"
                        );
                    }
                },
                {
                    "DisplayConfig: Active Paths",
                    () =>
                    {
                        ConsoleNavigation.PrintObject(
                            PathInfo.GetActivePaths(),
                            pathInfo =>
                            {
                                ConsoleNavigation.PrintObject(
                                    pathInfo.TargetsInfo,
                                    targetInfo =>
                                    {
                                        ConsoleNavigation.PrintObject(
                                            targetInfo.SignalInfo,
                                            "PathTargetInfo.SignalInfo"
                                        );
                                    },
                                    "PathInfo.TargetsInfo",
                                    "Select a PathTargetInfo to see target signal information."
                                );
                            },
                            "PathInfo.GetActivePaths()",
                            "Select a PathInfo to see associated targets."
                        );
                    }
                },
                {
                    "DisplayConfig: Go Saved Clone",
                    () =>
                    {
                        PathInfo.ApplyTopology(DisplayConfigTopologyId.Clone, true);
                    }
                },
                {
                    "DisplayConfig: Go Saved Extend",
                    () =>
                    {
                        PathInfo.ApplyTopology(DisplayConfigTopologyId.Extend, true);
                    }
                },
                {
                    "DisplayConfig: Extend All Displays",
                    () =>
                    {
                        var sourceId = 0u;
                        var lastWidth = 0;
                        var pathInfos = new List<PathInfo>();
                        var pathTargets = PathDisplayTarget.GetDisplayTargets()
                            .Select(
                                target => Tuple.Create(
                                    target.PreferredResolution,
                                    new PathTargetInfo(
                                        target,
                                        target.PreferredSignalMode,
                                        DisplayConfigRotation.Identity,
                                        DisplayConfigScaling.Identity
                                    )
                                )
                            )
                            .ToList();

                        foreach (var target in pathTargets)
                        {
                            var source = new PathDisplaySource(target.Item2.DisplayTarget.Adapter, sourceId);
                            pathInfos.Add(
                                new PathInfo(
                                    source,
                                    new Point(lastWidth, 0),
                                    target.Item1,
                                    DisplayConfigPixelFormat.PixelFormat32Bpp,
                                    new[] {target.Item2}
                                )
                            );
                            lastWidth += target.Item1.Width;
                            sourceId++;
                        }

                        PathInfo.ApplyPathInfos(pathInfos.ToArray());
                    }
                },
                {
                    "DisplayConfig: Clone All Compatible Displays Together",
                    () =>
                    {
                        var pathTargets = new Dictionary<Tuple<Size, PathDisplayAdapter>, List<PathTargetInfo>>();

                        foreach (var target in PathDisplayTarget.GetDisplayTargets())
                        {
                            var key = Tuple.Create(target.PreferredResolution, target.Adapter);

                            if (!pathTargets.ContainsKey(key))
                            {
                                pathTargets.Add(key, new List<PathTargetInfo>());
                            }

                            pathTargets[key].Add(new PathTargetInfo(
                                target,
                                target.PreferredSignalMode,
                                DisplayConfigRotation.Identity,
                                DisplayConfigScaling.Identity)
                            );
                        }

                        var pathInfos = new List<PathInfo>();
                        var sourceId = 0u;
                        var lastWidth = 0;

                        foreach (var target in pathTargets.OrderByDescending(pair => pair.Value.Count))
                        {
                            var source = new PathDisplaySource(target.Key.Item2, sourceId);
                            pathInfos.Add(
                                new PathInfo(
                                    source,
                                    new Point(lastWidth, 0),
                                    target.Key.Item1,
                                    DisplayConfigPixelFormat.PixelFormat32Bpp,
                                    target.Value.ToArray()
                                )
                            );
                            lastWidth += target.Key.Item1.Width;
                            sourceId++;
                        }

                        PathInfo.ApplyPathInfos(pathInfos.ToArray());
                    }
                },
                {
                    "DisplayConfig: Disable All Except Primary Path",
                    () =>
                    {
                        var pathInfos = PathInfo.GetActivePaths();
                        PathInfo.ApplyPathInfos(new[] {pathInfos.First(info => info.IsGDIPrimary)});
                    }
                }
            };

            ConsoleNavigation.PrintNavigation(
                navigation,
                "DisplayConfig functions",
                "Select a DisplayConfig sample."
            );
        }

        private static void Main()
        {
            var navigation = new Dictionary<object, Action>
            {
                {
                    "DeviceContext",
                    DeviceContext
                },
                {
                    "DisplayConfig",
                    DisplayConfig
                }
            };

            ConsoleNavigation.PrintNavigation(
                navigation,
                "Execution Lines",
                "Select an execution line to browse WindowsDisplayAPI functionalities."
            );
        }
    }
}