using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using WindowsDisplayAPI.Native;
using WindowsDisplayAPI.Native.DisplayConfig.Structures;
using WindowsDisplayAPI.Native.Structures;

namespace WindowsDisplayAPI.DisplayConfig
{
    /// <summary>
    ///     Represents a display path source
    /// </summary>
    [Serializable]
    public class PathDisplaySource : IEquatable<PathDisplaySource>
    {
        public PathDisplaySource() { }
        /// <summary>
        ///     Creates a new PathDisplaySource
        /// </summary>
        /// <param name="adapter">Display adapter</param>
        /// <param name="sourceId">Display source identification</param>
        public PathDisplaySource(PathDisplayAdapter adapter, uint sourceId)
        {
            Adapter = adapter;
            SourceId = sourceId;
        }

        /// <summary>
        ///     Gets the path display adapter
        /// </summary>
        public PathDisplayAdapter Adapter { get; }

        /// <summary>
        ///     Gets the display name
        /// </summary>
        /// <exception cref="Win32Exception">Error code can be retrieved from Win32Exception.NativeErrorCode property</exception>
        public string DisplayName
        {
            get
            {
                var sourceName = new DisplayConfigSourceDeviceName(Adapter.AdapterId, SourceId);
                var result = DisplayConfigApi.DisplayConfigGetDeviceInfo(ref sourceName);
                if (result == Win32Status.Success)
                    return sourceName.DeviceName;
                throw new Win32Exception((int) result);
            }
        }

        /// <summary>
        ///     Gets the zero based display identification
        /// </summary>
        public uint SourceId { get; }

        /// <inheritdoc />
        public bool Equals(PathDisplaySource other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Adapter == other.Adapter && SourceId == other.SourceId;
        }

        /// <summary>
        ///     Retrieving a list of all display sources from the currently active and inactive paths
        /// </summary>
        /// <returns>An array of PathDisplaySource instances</returns>
        public static PathDisplaySource[] GetDisplaySources()
        {
            var sources = new Dictionary<Tuple<LUID, uint>, PathDisplaySource>();
            foreach (var pathInfo in PathInfo.GetAllPaths())
            {
                var key = new Tuple<LUID, uint>(pathInfo.DisplaySource.Adapter.AdapterId,
                    pathInfo.DisplaySource.SourceId);
                if (!pathInfo.DisplaySource.Adapter.IsInvalid && !sources.ContainsKey(key))
                    sources.Add(key, pathInfo.DisplaySource);
            }
            return sources.Values.ToArray();
        }

        /// <summary>
        ///     Checks for equality of two PathDisplaySource instances
        /// </summary>
        /// <param name="left">The first instance</param>
        /// <param name="right">The second instance</param>
        /// <returns>true if both instaces are equal, otherwise false</returns>
        public static bool operator ==(PathDisplaySource left, PathDisplaySource right)
        {
            return Equals(left, right) || left?.Equals(right) == true;
        }

        /// <summary>
        ///     Checks for inequality of two PathDisplaySource instances
        /// </summary>
        /// <param name="left">The first instance</param>
        /// <param name="right">The second instance</param>
        /// <returns>true if both instaces are not equal, otherwise false</returns>
        public static bool operator !=(PathDisplaySource left, PathDisplaySource right)
        {
            return !(left == right);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((PathDisplaySource) obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                return ((Adapter != null ? Adapter.GetHashCode() : 0)*397) ^ (int) SourceId;
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return DisplayName;
        }

        /// <summary>
        ///     Returns the corresponding DisplayDevice instances
        /// </summary>
        /// <returns>An enumerable list of DisplayDevices</returns>
        public IEnumerable<DisplayDevice> ToDisplayDevices()
        {
            return
                DisplayAdapter.GetDisplayAdapters()
                    .SelectMany(adapter => adapter.GetDisplayDevices())
                    .Where(device => device.DisplayName.Equals(DisplayName));
        }
    }
}