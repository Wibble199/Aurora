using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using Aurora.Profiles;
using Aurora.Utils;
using Newtonsoft.Json;

namespace Aurora.Settings.Overrides.Logic {

    /// <summary>
    /// Condition that checks a set of subconditions for atleast one of them being true.
    /// </summary>
    [Evaluatable("Or", category: OverrideLogicCategory.Logic)]
    public class BooleanOr : Evaluatable<bool, Control_SubconditionHolder>, IHasSubConditons {

        /// <summary>Creates a new Or evaluatable with no subconditions.</summary>
        public BooleanOr() { }
        /// <summary>Creates a new Or evaluatable with the given subconditions.</summary>
        public BooleanOr(IEnumerable<IEvaluatable<bool>> subconditions) {
            SubConditions = new ObservableCollection<IEvaluatable<bool>>(subconditions);
        }

        [JsonProperty]
        public ObservableCollection<IEvaluatable<bool>> SubConditions { get; set; } = new ObservableCollection<IEvaluatable<bool>>();

        public override Control_SubconditionHolder CreateControl() => new Control_SubconditionHolder(this, "Require atleast one of the following is true...");

        public override bool Evaluate(IGameState gameState) => SubConditions.Any(subcondition => subcondition?.Evaluate(gameState) ?? false);

        public override void SetApplication(Application application) {
            Control.Application = application;
            foreach (var subcondition in SubConditions)
                subcondition.SetApplication(application);
        }

        public override IEvaluatable<bool> Clone() => new BooleanOr { SubConditions = new ObservableCollection<IEvaluatable<bool>>(SubConditions.Select(e => e.Clone())) };
    }


    /// <summary>
    /// Condition that checks a set of subconditions and requires them all to be true.
    /// </summary>
    [Evaluatable("And", category: OverrideLogicCategory.Logic)]
    public class BooleanAnd : Evaluatable<bool, Control_SubconditionHolder>, IHasSubConditons {

        /// <summary>Creates a new And evaluatable with no subconditions.</summary>
        public BooleanAnd() { }
        /// <summary>Creates a new And evaluatable with the given subconditions.</summary>
        public BooleanAnd(IEnumerable<IEvaluatable<bool>> subconditions) {
            SubConditions = new ObservableCollection<IEvaluatable<bool>>(subconditions);
        }

        [JsonProperty]
        public ObservableCollection<IEvaluatable<bool>> SubConditions { get; set; } = new ObservableCollection<IEvaluatable<bool>>();

        public override Control_SubconditionHolder CreateControl() => new Control_SubconditionHolder(this, "Require all of the following are true...");

        public override bool Evaluate(IGameState gameState) => SubConditions.All(subcondition => subcondition?.Evaluate(gameState) ?? false);

        public override void SetApplication(Application application) {
            Control.Application = application;
            foreach (var subcondition in SubConditions)
                subcondition.SetApplication(application);
        }

        public override IEvaluatable<bool> Clone() => new BooleanAnd { SubConditions = new ObservableCollection<IEvaluatable<bool>>(SubConditions.Select(e => { var x = e.Clone(); return x; })) };
    }



    /// <summary>
    /// Condition that inverts another condition.
    /// </summary>
    [Evaluatable("Not", category: OverrideLogicCategory.Logic)]
    public class BooleanNot : Evaluatable<bool, Control_ConditionNot> {

        /// <summary>Creates a new NOT evaluatable with the default BooleanTrue subcondition.</summary>
        public BooleanNot() { }
        /// <summary>Creates a new NOT evaluatable which inverts the given subcondition.</summary>
        public BooleanNot(IEvaluatable<bool> subcondition) {
            SubCondition = subcondition;
        }

        [JsonProperty]
        public IEvaluatable<bool> SubCondition { get; set; } = new BooleanConstant();

        public override Control_ConditionNot CreateControl() => new Control_ConditionNot(this);

        public override bool Evaluate(IGameState gameState) => !SubCondition.Evaluate(gameState);

        public override void SetApplication(Application application) {
            Control.Application = application;
            SubCondition?.SetApplication(application);
        }

        public override IEvaluatable<bool> Clone() => new BooleanNot { SubCondition = SubCondition.Clone() };
    }


    /// <summary>
    /// Condition that always returns true. Useful as a default condition as it means that
    /// the layer will always be visible.
    /// </summary>
    [Evaluatable("Boolean Constant", category: OverrideLogicCategory.Logic)]
    public class BooleanConstant : Evaluatable<bool, CheckBox> {

        /// <summary>Creates a new constant true boolean.</summary>
        public BooleanConstant() { }
        /// <summary>Creates a new constant boolean with the given state.</summary>
        public BooleanConstant(bool state) { }

        /// <summary>The value held by this constant value.</summary>
        public bool State { get; set; }

        // Create a checkbox to use to set the constant value
        public override CheckBox CreateControl() => new CheckBox { Content = "True/False" }
            .WithBinding(CheckBox.IsCheckedProperty, new Binding("State") { Source = this, Mode = BindingMode.TwoWay });

        // Simply return the current state
        public override bool Evaluate(IGameState _) => State;

        // Creates a new BooleanConstant
        public override IEvaluatable<bool> Clone() => new BooleanConstant { State = State };
    }


    /// <summary>
    /// Indicates that the implementing class has a SubCondition collection property.
    /// </summary>
    public interface IHasSubConditons {
        ObservableCollection<IEvaluatable<bool>> SubConditions { get; set; }
    }
}
