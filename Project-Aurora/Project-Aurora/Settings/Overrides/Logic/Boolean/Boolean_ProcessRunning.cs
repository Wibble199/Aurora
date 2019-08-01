using Aurora.Profiles;
using Aurora.Utils;
using System.Windows.Controls;
using System.Windows.Data;

namespace Aurora.Settings.Overrides.Logic.Boolean {
    [Evaluatable("Process Running", category: OverrideLogicCategory.Misc)]
    public class BooleanProcessRunning : Evaluatable<bool, TextBox> {

        public string ProcessName { get; set; } = "";

        public override TextBox CreateControl() => new TextBox { MinWidth = 80 }
            .WithBinding(TextBox.TextProperty, new Binding("ProcessName") { Source = this, Mode = BindingMode.TwoWay });

        public override bool Evaluate(IGameState gameState)
            => RunningProcessMonitor.Instance.IsProcessRunning(ProcessName);

        public override IEvaluatable<bool> Clone() => new BooleanProcessRunning { ProcessName = ProcessName };
    }
}
