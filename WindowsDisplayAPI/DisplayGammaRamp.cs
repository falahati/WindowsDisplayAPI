using System;
using WindowsDisplayAPI.Native.DeviceContext.Structures;

namespace WindowsDisplayAPI
{
    public class DisplayGammaRamp
    {
        public DisplayGammaRamp(ushort[] red, ushort[] green, ushort[] blue)
        {
            if (red?.Length != GammaRamp.DataPoints)
            {
                throw new ArgumentOutOfRangeException(nameof(red));
            }

            if (green?.Length != GammaRamp.DataPoints)
            {
                throw new ArgumentOutOfRangeException(nameof(green));
            }

            if (blue?.Length != GammaRamp.DataPoints)
            {
                throw new ArgumentOutOfRangeException(nameof(blue));
            }

            Red = red;
            Green = green;
            Blue = blue;
        }

        public DisplayGammaRamp(double brightness = 0.5, double contrast = 0.5, double gamma = 1)
            : this(
                CalculateLUT(brightness, contrast, gamma),
                CalculateLUT(brightness, contrast, gamma),
                CalculateLUT(brightness, contrast, gamma)
            )
        {
        }

        public DisplayGammaRamp(
            double redBrightness,
            double redContrast,
            double redGamma,
            double greenBrightness,
            double greenContrast,
            double greenGamma,
            double blueBrightness,
            double blueContrast,
            double blueGamma
        )
            : this(
                CalculateLUT(redBrightness, redContrast, redGamma),
                CalculateLUT(greenBrightness, greenContrast, greenGamma),
                CalculateLUT(blueBrightness, blueContrast, blueGamma)
            )
        {
        }

        internal DisplayGammaRamp(GammaRamp ramp) :
            this(ramp.Red, ramp.Green, ramp.Blue)
        {
        }

        public ushort[] Blue { get; }
        public ushort[] Green { get; }
        public ushort[] Red { get; }

        private static ushort[] CalculateLUT(double brightness, double contrast, double gamma)
        {
            // Limit gamma in range [0.4-2.8]
            gamma = Math.Min(Math.Max(gamma, 0.4), 2.8);

            // Normalize contrast in range [-1,1]
            contrast = (Math.Min(Math.Max(contrast, 0), 1) - 0.5) * 2;

            // Normalize brightness in range [-1,1]
            brightness = (Math.Min(Math.Max(brightness, 0), 1) - 0.5) * 2;

            // Calculate curve offset resulted from contrast
            var offset = contrast > 0 ? contrast * -25.4 : contrast * -32;

            // Calculate the total range of curve
            var range = GammaRamp.DataPoints - 1 + offset * 2;

            // Add brightness to the curve offset
            offset += brightness * (range / 5);

            // Fill the gamma curve
            var result = new ushort[GammaRamp.DataPoints];

            for (var i = 0; i < result.Length; i++)
            {
                var factor = (i + offset) / range;

                factor = Math.Pow(factor, 1 / gamma);

                factor = Math.Min(Math.Max(factor, 0), 1);

                result[i] = (ushort) Math.Round(factor * ushort.MaxValue);
            }

            return result;
        }

        internal GammaRamp AsRamp()
        {
            return new GammaRamp(Red, Green, Blue);
        }
    }
}