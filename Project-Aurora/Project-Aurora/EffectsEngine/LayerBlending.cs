using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace Aurora.EffectsEngine {

    public static class LayerBlending {

        #region Blend Implementations
        unsafe public static void AlphaBlend(byte* target, byte* input, byte* mask, int x) {
            target[x] = input[x]; // b
            target[x + 1] = input[x + 1]; // g
            target[x + 2] = input[x + 2]; // r
            target[x + 3] = mask[x + 3]; // a
        }
        #endregion


        public static EffectLayer ApplyBlend(BlendingMode mode, EffectLayer input, EffectLayer blend) {
            if (mode == BlendingMode.None)
                return input;

            unsafe {
                // Figure out which method to use based on the BlendingMode
                ForEachPixelDelegate action = mode switch
                {
                    BlendingMode.Alpha => AlphaBlend,
                    _ => throw new ArgumentException($"Unknown BlendingMode type '{mode}'")
                };

                // Run the blend method on all pixels on both the input and blend
                var output = new EffectLayer();
                ForEachPixel(output, input, blend, action);
                return output;
            }
        }

        // Helper function that runs the given action on each pixel for an input and blend
        // Heavily adapted from https://stackoverflow.com/a/3654231
        unsafe private static void ForEachPixel(EffectLayer target, EffectLayer input, EffectLayer blend, ForEachPixelDelegate action) {
            var rect = new Rectangle(0, 0, target.Colormap.Width, target.Colormap.Height);

            var bitsTarget = target.Colormap.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            var bitsInput = input.Colormap.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            var bitsBlend = blend.Colormap.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

            for (var y = 0; y < rect.Height; y++) {
                var ptrTarget = (byte*)bitsTarget.Scan0 + y * bitsTarget.Stride;
                var ptrInput = (byte*)bitsInput.Scan0 + y * bitsInput.Stride;
                var ptrBlend = (byte*)bitsBlend.Scan0 + y * bitsBlend.Stride;

                for (var x = 0; x < rect.Width; x++)
                    action(ptrTarget, ptrInput, ptrBlend, 4 * x);
            }

            target.Colormap.UnlockBits(bitsTarget);
            input.Colormap.UnlockBits(bitsInput);
            blend.Colormap.UnlockBits(bitsBlend);
        }

        /// <summary>
        /// Delegate action that will be run on every pixel 
        /// </summary>
        private unsafe delegate void ForEachPixelDelegate(byte* target, byte* input, byte* blend, int x);
    }

    public enum BlendingMode {
        None,
        Alpha
    }
}
