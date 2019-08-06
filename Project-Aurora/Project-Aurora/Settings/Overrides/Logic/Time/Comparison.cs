using Aurora.Profiles;
using System.Windows.Media;

namespace Aurora.Settings.Overrides.Logic {

    /// <summary>
    /// Returns true if the time is between the given time range (uses <see cref="Time.IsBetween(Time, Time, Time)"/>). This
    /// will gracefully wrap the time around midnight.
    /// </summary>
    [Evaluatable("Time between", category: OverrideLogicCategory.Time)]
    public class BooleanTimeBetween : Evaluatable<bool, Control_TimeBetween> {

        /// <summary>Creates a new Time-Between evaluatable that will have the default values.</summary>
        public BooleanTimeBetween() { }
        /// <summary>Creates a new Time-Between evaluatable that will check if the current time is between the given constant time values.</summary>
        public BooleanTimeBetween(Time start, Time end) : this(new TimeNow(), new TimeConstant(start), new TimeConstant(end)) { }
        /// <summary>Creates a new Time-Between evaluatable that will check if the given time evaluatable is between the given constant time values.</summary>
        public BooleanTimeBetween(IEvaluatable<Time> value, Time start, Time end) : this(value, new TimeConstant(start), new TimeConstant(end)) { }
        /// <summary>Creates a new Time-Between evaluatable that will check if the given time evaluatable is between the given evaluatable time values.</summary>
        public BooleanTimeBetween(IEvaluatable<Time> value, IEvaluatable<Time> start, IEvaluatable<Time> end) { Value = value; Start = start; End = end; }

        /// <summary>The value of the time to check if it is between `Start` and `End`.</summary>
        public IEvaluatable<Time> Value { get; set; } = new TimeNow();
        /// <summary>Evaluatable that represents the start of the between time.</summary>
        public IEvaluatable<Time> Start { get; set; } = new TimeConstant(21,00);
        /// <summary>Evaluatable that represents the end of the between time.</summary>
        public IEvaluatable<Time> End { get; set; } = new TimeConstant(08,00);

        public override Control_TimeBetween CreateControl() => new Control_TimeBetween(this);

        /// <summary>Returns whether or not the evaluation of the `Value` time is between the evaluation of the `Start` and `End` times.</summary>
        public override bool Evaluate(IGameState gameState) => Time.IsBetween(Value.Evaluate(gameState), Start.Evaluate(gameState), End.Evaluate(gameState));

        public override void SetApplication(Application application) {
            Value?.SetApplication(application);
            Start?.SetApplication(application);
            End?.SetApplication(application);
            Control.SetApplication(application);
        }

        public override IEvaluatable<bool> Clone() => new BooleanTimeBetween { Value = Value.Clone(), Start = Start.Clone(), End = End.Clone() };
    }
}
