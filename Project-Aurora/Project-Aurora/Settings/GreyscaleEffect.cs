using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace Aurora.Settings {

    /// <summary>
    /// Greyscale effect shader that can reduce the saturation of UI elements.
    /// </summary>
    /// <remarks>From http://bursjootech.blogspot.com/2008/06/grayscale-effect-pixel-shader-effect-in.html </remarks>
    public class GreyscaleEffect : ShaderEffect {

        // Load the shader file from the resources
        private static PixelShader pixelShader = new PixelShader {
            UriSource = new System.Uri(@"pack://application:,,,/Resources/GreyscaleShader.ps")
        };

        public GreyscaleEffect() {
            PixelShader = pixelShader;
            UpdateShaderValue(InputProperty);
            UpdateShaderValue(DesaturationFactorProperty);
        }

        public static readonly DependencyProperty InputProperty = RegisterPixelShaderSamplerProperty("Input", typeof(GreyscaleEffect), 0);
        public Brush Input {
            get => (Brush)GetValue(InputProperty);
            set => SetValue(InputProperty, value);
        }

        public static readonly DependencyProperty DesaturationFactorProperty = DependencyProperty.Register("DesaturationFactor", typeof(double), typeof(GreyscaleEffect), new UIPropertyMetadata(0.0, PixelShaderConstantCallback(0), CoerceDesaturationFactor));
        public double DesaturationFactor {
            get => (double)GetValue(DesaturationFactorProperty);
            set => SetValue(DesaturationFactorProperty, value);
        }

        private static object CoerceDesaturationFactor(DependencyObject d, object value) {
            GreyscaleEffect effect = (GreyscaleEffect)d;
            double newFactor = (double)value;
            return (newFactor < 0.0 || newFactor > 1.0) ? effect.DesaturationFactor : newFactor;
        }
    }
}
