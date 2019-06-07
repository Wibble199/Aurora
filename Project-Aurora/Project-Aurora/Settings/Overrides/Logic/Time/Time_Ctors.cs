using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using Aurora.Profiles;
using Xceed.Wpf.Toolkit;

namespace Aurora.Settings.Overrides.Logic {

    /// <summary>
    /// Evaluatable that returns a constant user-defined time.
    /// </summary>
    [Evaluatable("Time constant", category: OverrideLogicCategory.Time)]
    public class TimeConstant : IEvaluatable<Time> {

        /// <summary>Creates a new Time constant representing midnight.</summary>
        public TimeConstant() : this(0, 0) { }

        /// <summary>Creates a new Time constant representing the specified time.</summary>
        public TimeConstant(int hour, int minute) { Time = new Time(hour, minute); }

        /// <summary>Creates a new Time constant representing the specified time.</summary>
        public TimeConstant(Time time) { Time = time; }

        public Time Time { get; set; }

        /// <summary>Creates a control allowing the user to change the defined time.</summary>
        public Visual GetControl(Application application) {
            var hour = new IntegerUpDown { Minimum = 0, Maximum = 23, Width = 100 };
            hour.SetBinding(IntegerUpDown.ValueProperty, new Binding("Hour") { Source = Time, Mode = BindingMode.TwoWay });

            var minute = new IntegerUpDown { Minimum = 0, Maximum = 59, Width = 100 };
            minute.SetBinding(IntegerUpDown.ValueProperty, new Binding("Minute") { Source = Time, Mode = BindingMode.TwoWay });

            var stackPanel = new StackPanel { Orientation = Orientation.Horizontal };
            stackPanel.Children.Add(hour);
            stackPanel.Children.Add(new TextBlock { Text = ":", Margin = new System.Windows.Thickness(8, 0, 8, 0) });
            stackPanel.Children.Add(minute);
            return stackPanel;
        }

        public Time Evaluate(IGameState gameState) => Time;
        object IEvaluatable.Evaluate(IGameState gameState) => Evaluate(gameState);

        public void SetApplication(Application application) { }

        public IEvaluatable<Time> Clone() => new TimeConstant { Time = new Time(Time.Hour, Time.Minute) };
        IEvaluatable IEvaluatable.Clone() => Clone();
    }


    /// <summary>
    /// Evaluatable that returns the current time.
    /// </summary>
    [Evaluatable("Current time", category: OverrideLogicCategory.Time)]
    public class TimeNow : IEvaluatable<Time> {

        public TimeNow() { }

        public Visual GetControl(Application application) => null;

        public Time Evaluate(IGameState gameState) => Time.Now;
        object IEvaluatable.Evaluate(IGameState gameState) => Evaluate(gameState);

        public void SetApplication(Application application) { }

        public IEvaluatable<Time> Clone() => new TimeNow();
        IEvaluatable IEvaluatable.Clone() => Clone();
    }
}
