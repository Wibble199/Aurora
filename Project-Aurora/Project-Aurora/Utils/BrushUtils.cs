using System.Linq;
using System.Windows.Media;

namespace Aurora.Utils {

    /// <summary>
    /// Class that contains various helper functions for dealing with Media Brushes.
    /// </summary>
    public static class BrushUtils {

        #region Brush Blending
        /// <summary>
        /// Blends two brushes together. Currently supports <see cref="SolidColorBrush"/>es and <see cref="LinearGradientBrush"/>es. Will return transparent brush
        /// if the brush types being blended aren't supported.
        /// <para>Can take mixed brush types (e.g. can handle blending between <see cref="SolidColorBrush"/> and <see cref="LinearGradientBrush"/>).</para>
        /// </summary>
        /// <param name="amount">Determines the strength of each brush. At 0, only brush a will be used. At 1, only brush b will be used.</param>
        public static Brush BlendBrushes(Brush a, Brush b, double amount) {
            if (amount <= 0) return a;
            else if (amount >= 1) return b;
            else return (a, b) switch {
                // If both brushes are solid, we simply want to create a new solid brush of their colors combined
                (SolidColorBrush origin, SolidColorBrush destination) => (Brush) new SolidColorBrush(BlendColorsAssert(origin.Color, destination.Color, amount)),

                // If one or both are linear, blend them using my custom-written custom gradient blender
                (SolidColorBrush origin, LinearGradientBrush destination) => BlendLinearGradientBrush(origin, destination, amount),
                (LinearGradientBrush origin, SolidColorBrush destination) => BlendLinearGradientBrush(origin, destination, amount),
                (LinearGradientBrush origin, LinearGradientBrush destination) => BlendLinearGradientBrush(origin, destination, amount),

                // In any other case (e.g. radial brushes), we don't know how to handle them yet, so just return a default brush
                _ => Brushes.Transparent
            };
        }

        /// <summary>
        /// Blends two <see cref="LinearGradientBrush"/>es by creating a new brush that has gradient stops at every place either the 'left' or
        /// 'right' brushes did, and at each of these stops the merge color based on the value of each brush is calculated.
        /// </summary>
        public static LinearGradientBrush BlendLinearGradientBrush(LinearGradientBrush left, LinearGradientBrush right, double amount) {
            // If the amount is at either end of the scale, jsut return the brushes, no need to do calculation for it
            if (amount <= 0) return left.Clone();
            else if (amount >= 1) return right.Clone();

            var stops = left.GradientStops.Select(s => s.Offset) // Get all the offset positions in the left gradient
                .Concat(right.GradientStops.Select(s => s.Offset)) // And merge that list with all the ones in the right gradient
                .Distinct() // But only select each value once

                .Select(off => new GradientStop( // Then, at each of these offsets construct a new stop
                    BlendColorsAssert(left.GradientStops.GetColorAt(off), right.GradientStops.GetColorAt(off), amount), off // By blending the colors from each brush at that location (regardless of whether it was an offset of that brush)
                ));

            // Return a new brush from the new stop collection
            return new LinearGradientBrush(new GradientStopCollection(stops));
        }

        /// <summary>
        /// Blends a <see cref="LinearGradientBrush"/> and a <see cref="SolidColorBrush"/> by blending the color of the solid brush with
        /// the color at each of the linear gradient brush stops. Returns a new brush with the blended stops.
        /// </summary>
        public static LinearGradientBrush BlendLinearGradientBrush(LinearGradientBrush grad, SolidColorBrush solid, double amount) {
            var stops = grad.GradientStops.Clone();
            foreach (var stop in stops)
                stop.Color = BlendColorsAssert(stop.Color, solid.Color, amount);
            return new LinearGradientBrush(stops);
        }

        /// <summary>
        /// Blends a <see cref="LinearGradientBrush"/> and a <see cref="SolidColorBrush"/> by blending the color of the solid brush with
        /// the color at each of the linear gradient brush stops. Returns a new brush with the blended stops.
        /// </summary>
        public static LinearGradientBrush BlendLinearGradientBrush(SolidColorBrush solid, LinearGradientBrush grad, double amount)
            => BlendLinearGradientBrush(grad, solid, 1 - amount);

        /// <summary>
        /// Blends two colors, but ensures that if either of them are <see cref="Colors.Transparent"/>, they are instead replaced with
        /// the other color but with alpha set to 0 instead. This means, for example, that fading Red into Transparent, the color does
        /// not go white as it's alpha increases, but stays red.
        /// </summary>
        /// <seealso cref="AssertTransparency(Color, Color)"/>
        private static Color BlendColorsAssert(Color a, Color b, double amount)
            => ColorUtils.BlendColors(a.AssertTransparency(b), b.AssertTransparency(a), amount);

        /// <summary>Takes two colors. If the first (trg) color is <see cref="Colors.Transparent"/>, returns a new transparent color that has identical RGB values to the
        /// second (base) color except with 0 alpha. This is useful when blending a color with transparent so that it doesn't go whiter as it's alpha decreases.</summary>
        private static Color AssertTransparency(this Color trg, Color @base) =>
            trg.Equals(Colors.Transparent)
                ? Color.FromArgb(0, @base.R, @base.G, @base.B)
                : trg;
        #endregion

        /// <summary>Gets the color at the given point in the given <see cref="GradientStopCollection"/>.</summary>
        public static Color GetColorAt(this GradientStopCollection gsc, double offset) {
            var stops = gsc.OrderBy(s => s.Offset).ToArray();

            // Check if any GradientStops are exactly at the requested offset. If so, return that
            var exact = stops.SingleOrDefault(s => s.Offset == offset);
            if (exact != null) return exact.Color;

            // Check if the requested offset is outside of bounds of the offset range. If so, return the nearest offset
            if (offset <= stops[0].Offset) return stops[0].Color;
            if (offset >= stops[stops.Length - 1].Offset) return stops[stops.Length - 1].Color;

            // Find the two stops either side of the requsted offset
            var left = stops.Last(s => s.Offset < offset);
            var right = stops.First(s => s.Offset > offset);

            // Return the blended color that is the correct ratio between left and right
            return ColorUtils.BlendColors(left.Color, right.Color, (offset - left.Offset) / (right.Offset - left.Offset));
        }
    }
}
