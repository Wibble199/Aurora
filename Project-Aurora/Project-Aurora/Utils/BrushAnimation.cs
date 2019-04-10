using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Aurora.Utils {

    // If animate to transparent, get the base color of the from color and make it transparent? This will prevent flickering
    // Also try animate non-solid brushes. Maybe just fade into the other

    public class BrushAnimation : AnimationTimeline {
  
        /// <summary>The brush value that the animation will begin from.</summary>
        public Brush From {  get => (Brush)GetValue(FromProperty); set => SetValue(FromProperty, value); }
        public static readonly DependencyProperty FromProperty = DependencyProperty.Register("From", typeof(Brush), typeof(BrushAnimation));

        /// <summary>The brush value that the animation will tend towards.</summary>
        public Brush To { get => (Brush)GetValue(ToProperty); set => SetValue(ToProperty, value); }
        public static readonly DependencyProperty ToProperty = DependencyProperty.Register("To", typeof(Brush), typeof(BrushAnimation));


        /// <summary>Creates a new instance of the <see cref="BrushAnimation" /> Freezable.</summary>
        protected override Freezable CreateInstanceCore() => new BrushAnimation();

        /// <summary>The type that this animation timeline is for. In this case <see cref="Brush"/> types.</summary>
        public override Type TargetPropertyType => typeof(Brush);

        /// <summary> Calculates a new brush that is a result of the blended value between the To and From properties (or their default values if
        /// not set). The blend amount is determined by the given <see cref="AnimationClock"/> value. </summary>
        public override object GetCurrentValue(object defaultOriginValue, object defaultDestinationValue, AnimationClock animationClock) =>
            BrushUtils.BlendBrushes(From ?? (Brush)defaultOriginValue, To ?? (Brush)defaultDestinationValue, animationClock.CurrentProgress.Value);
    }
}
