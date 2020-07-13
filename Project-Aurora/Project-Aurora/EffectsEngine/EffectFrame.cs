using System;
using System.Collections.Generic;
using System.Linq;

namespace Aurora.EffectsEngine
{
    /// <summary>
    /// A class representative of one frame. Contains a list of EffectLayers as layers and overlay layers.
    /// </summary>
    public class EffectFrame : IDisposable
    {
        Queue<EffectLayer> over_layers = new Queue<EffectLayer>();
        Queue<EffectLayer> layers = new Queue<EffectLayer>();

        /// <summary>
        /// A default constructor for EffectFrame class
        /// </summary>
        public EffectFrame()
        {

        }

        /// <summary>
        /// Adds layers into the frame
        /// </summary>
        /// <param name="effectLayers">Array of layers to be added</param>
        public void AddLayers(EffectLayer[] effectLayers)
        {
            foreach (EffectLayer layer in effectLayers)
                layers.Enqueue(layer);
        }

        /// <summary>
        /// Add overlay layers into the frame
        /// </summary>
        /// <param name="effectLayers">Array of layers to be added</param>
        public void AddOverlayLayers(EffectLayer[] effectLayers)
        {
            foreach(EffectLayer layer in effectLayers)
                over_layers.Enqueue(layer);
        }

        /// <summary>
        /// Renders the layers stored on this frame to an effect layer.
        /// </summary>
        public void RenderLayers(EffectLayer target) => RenderLayers(target, layers.ToList());

        /// <summary>
        /// Renders the overlay layers stored on this frame to an effect layer.
        /// </summary>
        public void RenderOverlayLayers(EffectLayer target) => RenderLayers(target, over_layers.ToList());


        private static void RenderLayers(EffectLayer target, IList<EffectLayer> layers) {
            for (var i = 0; i < layers.Count;)
                i += RenderLayer(target, i, layers);
        }

        /// <summary>
        /// Renders the layer at the given index to the target effect layer. If the layer above this one has a
        /// blending mode, that layer will be rendered first and the result used to render this layer.
        /// Note that if the layer above the above layer also has a blending mode, that will be rendered first etc.
        /// </summary>
        /// <returns>a number indicating how many layers were consumed by the render process.</returns>
        private static int RenderLayer(EffectLayer target, int idx, IList<EffectLayer> layers) {
            // Check if a layer is above with a blending mode
            if (idx < layers.Count - 1 && layers[idx + 1].blendingMode != BlendingMode.None) {
                var temp = new EffectLayer();
                var offset = RenderLayer(temp, idx + 1, layers); // Recursively render the above layer - will handle any blending that layer needs
                target.Add(LayerBlending.ApplyBlend(layers[idx + 1].blendingMode, layers[idx], temp));
                return offset + 1;
            }

            // If there is no blending to do, simply append the layer
            target.Add(layers[idx]);
            return 1;
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    over_layers.Clear();
                    layers.Clear();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~EffectFrame() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
