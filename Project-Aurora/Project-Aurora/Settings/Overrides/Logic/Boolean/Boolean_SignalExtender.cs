using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Data;
using Aurora.Profiles;
using Aurora.Utils;

namespace Aurora.Settings.Overrides.Logic {
    [Evaluatable("Signal Extender", category: OverrideLogicCategory.Logic)]
    public class BooleanExtender : Evaluatable<bool, StackPanel> {

        private Stopwatch sw = new Stopwatch();

        public BooleanExtender() { }
        public BooleanExtender(IEvaluatable<bool> evaluatable) { Evaluatable = evaluatable; }
        public BooleanExtender(IEvaluatable<bool> evaluatable, double time, TimeUnit timeUnit = TimeUnit.Seconds) : this(evaluatable) { ExtensionTime = time; TimeUnit = timeUnit; }

        public IEvaluatable<bool> Evaluatable { get; set; } = EvaluatableDefaults.Get<bool>();
        public double ExtensionTime { get; set; } = 5;
        public TimeUnit TimeUnit { get; set; } = TimeUnit.Seconds;

        private Control_EvaluatablePresenter ep;
        public override StackPanel CreateControl() {
            var sp = new StackPanel();
            sp.Children.Add(ep = new Control_EvaluatablePresenter { EvalType = typeof(bool) }
                .WithBinding(Control_EvaluatablePresenter.ExpressionProperty, new Binding("Evaluatable") { Source = this, Mode = BindingMode.TwoWay }));
            sp.Children.Add(new Control_TimeAndUnit()
                .WithBinding(Control_TimeAndUnit.TimeProperty, new Binding("ExtensionTime") { Source = this, Mode = BindingMode.TwoWay })
                .WithBinding(Control_TimeAndUnit.UnitProperty, new Binding("TimeUnit") { Source = this, Mode = BindingMode.TwoWay })
            );
            return sp;
        }

        public override bool Evaluate(IGameState gameState) {
            var res = Evaluatable.Evaluate(gameState);
            if (res) sw.Restart();
            return sw.IsRunning && (TimeUnit switch {
                TimeUnit.Seconds => sw.Elapsed.TotalSeconds,
                TimeUnit.Minutes => sw.Elapsed.TotalMinutes,
                TimeUnit.Hours => sw.Elapsed.TotalHours,
                _ => double.MaxValue
            } < ExtensionTime);
        }

        public override void SetApplication(Application application) {
            _ = Control; // Force create the control so we have the eval presenter
            ep.Application = application;
            Evaluatable.SetApplication(application);
        }

        public override IEvaluatable<bool> Clone() => new BooleanExtender { Evaluatable = Evaluatable.Clone(), ExtensionTime = ExtensionTime, TimeUnit = TimeUnit };
    }
}
