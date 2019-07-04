using Newtonsoft.Json;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.Windows.Data;

using MColor = System.Windows.Media.Color;
using DColor = System.Drawing.Color;

namespace Aurora.Utils
{
    public static class ColorExt {
        public static DColor ToDrawingColor(this MColor self) => ColorUtils.MediaColorToDrawingColor(self);
        public static MColor ToMediaColor(this DColor self) => ColorUtils.DrawingColorToMediaColor(self);
        public static MColor Clone(this MColor self) => ColorUtils.CloneMediaColor(self);
        public static DColor Clone(this DColor clr) => ColorUtils.CloneDrawingColor(clr);
    }

    /// <summary>
    /// Various color utilities
    /// </summary>
    public static class ColorUtils
    {
        private static Random randomizer = new Random();

        /// <summary>
        /// Converts from System.Windows.Media.Color to System.Drawing.Color
        /// </summary>
        /// <param name="in_color">A Windows Media Color</param>
        /// <returns>A Drawing Color</returns>
        public static DColor MediaColorToDrawingColor(MColor in_color)
        {
            return DColor.FromArgb(in_color.A, in_color.R, in_color.G, in_color.B);
        }

        /// <summary>
        /// Converts from System.Drawing.Color to System.Windows.Media.Color
        /// </summary>
        /// <param name="in_color">A Drawing Color</param>
        /// <returns>A Windows Media Color</returns>
        public static MColor DrawingColorToMediaColor(DColor in_color)
        {
            return MColor.FromArgb(in_color.A, in_color.R, in_color.G, in_color.B);
        }

        /// <summary>
        /// Multiplies a byte by a specified double balue
        /// </summary>
        /// <param name="color">Part of the color, as a byte</param>
        /// <param name="value">The value to multiply the byte by</param>
        /// <returns>The color byte</returns>
        public static byte ColorByteMultiplication(byte color, double value)
        {
            byte returnbyte = color;

            if ((double)returnbyte * value >= 255.0)
                returnbyte = 255;
            else if ((double)returnbyte * value <= 0.0)
                returnbyte = 0;
            else
                returnbyte = (byte)((double)returnbyte * value);

            return returnbyte;
        }

        /// <summary>
        /// Blends two colors together by a specified amount
        /// </summary>
        /// <param name="background">The background color (When percent is at 0.0D, only this color is shown)</param>
        /// <param name="foreground">The foreground color (When percent is at 1.0D, only this color is shown)</param>
        /// <param name="percent">The blending percent value</param>
        /// <returns>The blended color</returns>
        public static DColor BlendColors(DColor background, DColor foreground, double percent)
        {
            if (percent < 0.0)
                percent = 0.0;
            else if (percent > 1.0)
                percent = 1.0;

            int Red = (byte)Math.Min((Int32)foreground.R * percent + (Int32)background.R * (1.0 - percent), 255);
            int Green = (byte)Math.Min((Int32)foreground.G * percent + (Int32)background.G * (1.0 - percent), 255);
            int Blue = (byte)Math.Min((Int32)foreground.B * percent + (Int32)background.B * (1.0 - percent), 255);
            int Alpha = (byte)Math.Min((Int32)foreground.A * percent + (Int32)background.A * (1.0 - percent), 255);

            return DColor.FromArgb(Alpha, Red, Green, Blue);
        }

        /// <summary>
        /// Blends two colors together by a specified amount
        /// </summary>
        /// <param name="background">The background color (When percent is at 0.0D, only this color is shown)</param>
        /// <param name="foreground">The foreground color (When percent is at 1.0D, only this color is shown)</param>
        /// <param name="percent">The blending percent value</param>
        /// <returns>The blended color</returns>
        public static MColor BlendColors(MColor background, MColor foreground, double percent)
        {
            if (percent < 0.0)
                percent = 0.0;
            else if (percent > 1.0)
                percent = 1.0;

            int Red = (byte)Math.Min((Int32)foreground.R * percent + (Int32)background.R * (1.0 - percent), 255);
            int Green = (byte)Math.Min((Int32)foreground.G * percent + (Int32)background.G * (1.0 - percent), 255);
            int Blue = (byte)Math.Min((Int32)foreground.B * percent + (Int32)background.B * (1.0 - percent), 255);
            int Alpha = (byte)Math.Min((Int32)foreground.A * percent + (Int32)background.A * (1.0 - percent), 255);

            return MColor.FromArgb((byte)Alpha, (byte)Red, (byte)Green, (byte)Blue);
        }

        /// <summary>
        /// Adds two colors together by using the alpha component of the foreground color
        /// </summary>
        /// <param name="background">The background color</param>
        /// <param name="foreground">The foreground color (must have transparency to allow color blending)</param>
        /// <returns>The sum of two colors</returns>
        public static DColor AddColors(DColor background, DColor foreground)
        {
            if ((object)background == null)
                return foreground;

            if ((object)foreground == null)
                return background;

            return BlendColors(background, foreground, foreground.A / 255.0);
        }

        /// <summary>
        /// Multiplies a Drawing Color instance by a scalar value
        /// </summary>
        /// <param name="color">The color to be multiplied</param>
        /// <param name="scalar">The scalar amount for multiplication</param>
        /// <returns>The multiplied Color</returns>
        public static DColor MultiplyColorByScalar(DColor color, double scalar)
        {
            int Red = ColorByteMultiplication(color.R, scalar);
            int Green = ColorByteMultiplication(color.G, scalar);
            int Blue = ColorByteMultiplication(color.B, scalar);
            int Alpha = ColorByteMultiplication(color.A, scalar);

            return DColor.FromArgb(Alpha, Red, Green, Blue);
        }

        /// <summary>
        /// Multiplies a Drawing Color instance by a scalar value
        /// </summary>
        /// <param name="color">The color to be multiplied</param>
        /// <param name="scalar">The scalar amount for multiplication</param>
        /// <returns>The multiplied Color</returns>
        public static MColor MultiplyColorByScalar(MColor color, double scalar)
        {
            int Red = ColorByteMultiplication(color.R, scalar);
            int Green = ColorByteMultiplication(color.G, scalar);
            int Blue = ColorByteMultiplication(color.B, scalar);
            int Alpha = ColorByteMultiplication(color.A, scalar);

            return MColor.FromArgb((byte)Alpha, (byte)Red, (byte)Green, (byte)Blue);
        }

        /// <summary>
        /// Generates a random color
        /// </summary>
        /// <returns>A random color</returns>
        public static DColor GenerateRandomColor()
        {
            return DColor.FromArgb(randomizer.Next(255), randomizer.Next(255), randomizer.Next(255));
        }

        /// <summary>
        /// Generates a random color within a certain base color range
        /// </summary>
        /// <param name="baseColor">A base color range</param>
        /// <returns>A random color within a base range</returns>
        public static DColor GenerateRandomColor(DColor baseColor)
        {
            int red = (randomizer.Next(255) + baseColor.R) / 2;
            int green = (randomizer.Next(255) + baseColor.G) / 2;
            int blue = (randomizer.Next(255) + baseColor.B) / 2;
            int alpha = (255 + baseColor.A) / 2;

            return DColor.FromArgb(alpha, red, green, blue);
        }

        /// <summary>
        /// Returns an average color from a presented Bitmap
        /// </summary>
        /// <param name="bitmap">The bitmap to be evaluated</param>
        /// <returns>An average color from the bitmap</returns>
        public static DColor GetAverageColor(System.Windows.Media.Imaging.BitmapSource bitmap)
        {
            var format = bitmap.Format;

            if (format != System.Windows.Media.PixelFormats.Bgr24 &&
                format != System.Windows.Media.PixelFormats.Bgr32 &&
                format != System.Windows.Media.PixelFormats.Bgra32 &&
                format != System.Windows.Media.PixelFormats.Pbgra32)
            {
                throw new InvalidOperationException("BitmapSource must have Bgr24, Bgr32, Bgra32 or Pbgra32 format");
            }

            var width = bitmap.PixelWidth;
            var height = bitmap.PixelHeight;
            var numPixels = width * height;
            var bytesPerPixel = format.BitsPerPixel / 8;
            var pixelBuffer = new byte[numPixels * bytesPerPixel];

            bitmap.CopyPixels(pixelBuffer, width * bytesPerPixel, 0);

            long blue = 0;
            long green = 0;
            long red = 0;

            for (int i = 0; i < pixelBuffer.Length; i += bytesPerPixel)
            {
                blue += pixelBuffer[i];
                green += pixelBuffer[i + 1];
                red += pixelBuffer[i + 2];
            }

            return DColor.FromArgb((byte)(red / numPixels), (byte)(green / numPixels), (byte)(blue / numPixels));
        }

        /// <summary>
        /// Returns an average color from a presented Bitmap
        /// </summary>
        /// <param name="bitmap">The bitmap to be evaluated</param>
        /// <returns>An average color from the bitmap</returns>
        public static DColor GetAverageColor(Bitmap bitmap)
        {
            long Red = 0;
            long Green = 0;
            long Blue = 0;
            long Alpha = 0;

            int numPixels = bitmap.Width * bitmap.Height;

            var srcData = bitmap.LockBits(
                new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadOnly,
                PixelFormat.Format32bppArgb);

            int stride = srcData.Stride;

            var Scan0 = srcData.Scan0;

            unsafe
            {
                byte* p = (byte*)(void*)Scan0;

                for (int y = 0; y < bitmap.Height; y++)
                {
                    for (int x = 0; x < bitmap.Width; x++)
                    {
                        Blue += p[(y * stride) + x * 4];
                        Green += p[(y * stride) + x * 4 + 1];
                        Red += p[(y * stride) + x * 4 + 2];
                        Alpha += p[(y * stride) + x * 4 + 3];
                    }
                }
            }

            bitmap.UnlockBits(srcData);

            return DColor.FromArgb((int)(Alpha / numPixels), (int)(Red / numPixels), (int)(Green / numPixels), (int)(Blue / numPixels));
        }

        public static DColor GetColorFromInt(int interger)
        {
            if (interger < 0)
                interger = 0;
            else if (interger > 16777215)
                interger = 16777215;

            int R = interger >> 16;
            int G = (interger >> 8) & 255;
            int B = interger & 255;

            return DColor.FromArgb(R, G, B);
        }

        public static int GetIntFromColor(DColor color)
        {
            return (color.R << 16) | (color.G << 8) | (color.B);
        }

        /// <summary>
        /// Returns a Luma coefficient for brightness of a color
        /// </summary>
        /// <param name="color">Color to be evaluated</param>
        /// <returns>The brightness of the color. [0 = Dark, 255 = Bright]</returns>
        public static byte GetColorBrightness(DColor color)
        {
            //Source: http://stackoverflow.com/a/12043228
            return (byte)(0.2126 * color.R + 0.7152 * color.G + 0.0722 * color.B);
        }

        /// <summary>
        /// Returns whether or not a color is considered to be dark, based on Luma coefficient
        /// </summary>
        /// <param name="color">Color to be evaluated</param>
        /// <returns>Whether or not the color is dark</returns>
        public static bool IsColorDark(DColor color)
        {
            //Source: http://stackoverflow.com/a/12043228
            return GetColorBrightness(color) < 40;
        }

        public static MColor CloneMediaColor(MColor clr)
        {
            return MColor.FromArgb(clr.A, clr.R, clr.G, clr.B);
        }

        public static DColor CloneDrawingColor(DColor clr)
        {
            return DColor.FromArgb(clr.ToArgb());
        }
    }

    public class ColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null ? default(MColor) : ColorUtils.DrawingColorToMediaColor((DColor)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null ? default(DColor) : ColorUtils.MediaColorToDrawingColor((MColor)value);
        }
    }

    /// <summary>
    /// Converts between a RealColor and Media color so that the RealColor class can be used with the Xceed Color Picker
    /// </summary>
    public class RealColorConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => ((RealColor)value).GetMediaColor();
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => new RealColor((MColor)value);
    }

    /// <summary>
    /// Class to convert between a <see cref="EffectsEngine.EffectBrush"></see> and a <see cref="System.Windows.Media.Brush"></see> so that it can be
    /// used with the ColorBox gradient editor control.
    /// </summary>
    public class EffectBrushToBrushConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => ((EffectsEngine.EffectBrush)value).GetMediaBrush();
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => new EffectsEngine.EffectBrush((System.Windows.Media.Brush)value);
    }

    public class BoolToColorConverter : IValueConverter
    {
        public static Tuple<DColor, DColor> TextWhiteRed = new Tuple<DColor, DColor>(DColor.FromArgb(255, 186, 186, 186), DColor.Red);

        public static Tuple<DColor, DColor> TextRedWhite = new Tuple<DColor, DColor>(DColor.Red, DColor.FromArgb(255, 186, 186, 186));

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool b = (bool)value;
            var clrs = parameter as Tuple<DColor, DColor> ?? TextWhiteRed;
            var clr = b ? clrs.Item1 : clrs.Item2;

            return new System.Windows.Media.SolidColorBrush(ColorUtils.DrawingColorToMediaColor(clr));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Takes a color and returns it with a modified opacity. The opacity should be specified as the converter parameter.
    /// </summary>
    public class OpacityConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var c = ((System.Windows.Media.SolidColorBrush)value).Color; var a = byte.Parse((string)parameter);
            c.A = a;
            return new System.Windows.Media.SolidColorBrush(c);
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    public class RealColor : ICloneable
    {
        [JsonProperty]
        private DColor Color { get; set; }

        public RealColor()
        {
            Color = DColor.Transparent;
        }

        public RealColor(MColor clr)
        {
            this.SetMediaColor(clr);
        }

        public RealColor(DColor color)
        {
            this.Color = color.Clone();
        }

        public DColor GetDrawingColor()
        {
            return Color.Clone();
        }

        public MColor GetMediaColor()
        {
            return Color.ToMediaColor();
        }

        public void SetDrawingColor(DColor clr)
        {
            this.Color = clr.Clone();
        }

        public void SetMediaColor(MColor clr)
        {
            this.Color = clr.ToDrawingColor();
        }

        public object Clone()
        {
            return new RealColor(this.Color.Clone());
        }

        public static implicit operator DColor(RealColor c) => c.GetDrawingColor();
        public static implicit operator MColor(RealColor c) => c.GetMediaColor();
    }
}
