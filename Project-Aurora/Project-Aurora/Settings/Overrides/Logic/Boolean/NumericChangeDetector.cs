using Aurora.Controls;
using Aurora.Profiles;
using Aurora.Utils;
using System.Windows.Data;
using Xceed.Wpf.Toolkit;

namespace Aurora.Settings.Overrides.Logic {

    /// <summary>
    /// Evaluatable that detects when the value of the given numeric evaluatable changes by a particular amount.
    /// <para>Can be used in conjunction with the <see cref="BooleanExtender"/> to make the 'true' last longer than a single eval tick.</para>
    /// </summary>
    [Evaluatable("Number Change Detector", category: OverrideLogicCategory.Maths)]
    public class NumericChangeDetector : Evaluatable<bool, SpacedStackPanel> {

        private double? lastValue;

        public NumericChangeDetector() { }
        public NumericChangeDetector(IEvaluatable<double> eval, bool detectRising = true, bool detectFalling = true, double threshold = 0) {
            Evaluatable = eval;
            DetectRising = detectRising;
            DetectFalling = detectFalling;
            DetectionThreshold = threshold;
        }

        public IEvaluatable<double> Evaluatable { get; set; } = EvaluatableDefaults.Get<double>();
        public bool DetectRising { get; set; } = true;
        public bool DetectFalling { get; set; } = true;
        public double DetectionThreshold { get; set; } = 0;

        private Control_EvaluatablePresenter ep;
        public override SpacedStackPanel CreateControl() => new SpacedStackPanel { SpacingAmount = 6 }
            .WithChild(ep = new Control_EvaluatablePresenter { EvalType = typeof(double) }
                .WithBinding(Control_EvaluatablePresenter.ExpressionProperty, this, "Evaluatable", BindingMode.TwoWay))
            .WithChild(new Switch { Content = "Detect on increase" }
                .WithBinding(Switch.IsCheckedProperty, this, "DetectRising"))
            .WithChild(new Switch { Content = "Detect on decrease" }
                .WithBinding(Switch.IsCheckedProperty, this, "DetectFalling"))
            .WithChild(new DoubleUpDown { Minimum = 0 }
                .WithBinding(DoubleUpDown.ValueProperty, this, "DetectionThreshold"));

        public override bool Evaluate(IGameState gameState) {
            var val = Evaluatable.Evaluate(gameState);
            var @out = false;
            if (lastValue.HasValue) {
                // If threshold is 0, we want it to be true on any change, so we can't use >= (as this will always be true). We also can't use > as this means a
                // threshold of 1 would not trigger on change 5 -> 6. By making it the smallest possible double when it's 0, it means that any change is detected
                // but 0 won't be - exactly what we want.
                var threshold = DetectionThreshold == 0 ? double.Epsilon : DetectionThreshold;
                var delta = lastValue.Value - val;
                @out = (DetectRising && delta <= -threshold) || (DetectFalling && delta >= threshold);
            }
            lastValue = val;
            return @out;
        }

        public override void SetApplication(Application application) {
            _ = Control; // Force control creation
            ep.Application = application;
            Evaluatable?.SetApplication(application);
        }

        public override IEvaluatable<bool> Clone() => new NumericChangeDetector { Evaluatable = Evaluatable.Clone(), DetectRising = DetectRising, DetectFalling = DetectFalling, DetectionThreshold = DetectionThreshold };
    }
}
