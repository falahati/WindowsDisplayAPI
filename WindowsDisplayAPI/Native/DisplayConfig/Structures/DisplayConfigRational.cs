using System;
using System.Diagnostics.Contracts;
using System.Runtime.InteropServices;

namespace WindowsDisplayAPI.Native.DisplayConfig.Structures
{
    // https://msdn.microsoft.com/en-us/library/windows/hardware/ff553968(v=vs.85).aspx
    [StructLayout(LayoutKind.Sequential)]
    internal struct DisplayConfigRational : IEquatable<DisplayConfigRational>
    {
        [MarshalAs(UnmanagedType.U4)] public readonly uint Numerator;
        [MarshalAs(UnmanagedType.U4)] public readonly uint Denominator;

        public DisplayConfigRational(uint numerator, uint denominator, bool simplify)
            : this((ulong) numerator, denominator, simplify)
        {
        }

        public DisplayConfigRational(ulong numerator, ulong denominator, bool simplify)
        {
            var gcm = simplify & (numerator != 0) ? Euclidean(numerator, denominator) : 1;
            Numerator = (uint) (numerator/gcm);
            Denominator = (uint) (denominator/gcm);
        }

        private static ulong Euclidean(ulong a, ulong b)
        {
            while ((a != 0) && (b != 0))
                if (a > b)
                    a %= b;
                else
                    b %= a;

            return a == 0 ? b : a;
        }

        [Pure]
        public ulong ToValue(ulong multiplier = 1)
        {
            if (Numerator == 0)
                return 0;
            return Numerator*multiplier/Denominator;
        }

        public bool Equals(DisplayConfigRational other)
        {
            return Numerator/Denominator == other.Numerator/other.Denominator;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is DisplayConfigRational && Equals((DisplayConfigRational) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((int) Numerator*397) ^ (int) Denominator;
            }
        }

        public static bool operator ==(DisplayConfigRational left, DisplayConfigRational right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(DisplayConfigRational left, DisplayConfigRational right)
        {
            return !left.Equals(right);
        }
    }
}