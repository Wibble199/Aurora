﻿using Aurora.Profiles;
using System.Collections.ObjectModel;
using System.Drawing;
using UIElement = System.Windows.UIElement;


namespace Aurora.Settings.Overrides.Logic
{
    public class IfElseGeneric<T> : IEvaluatable<T> {
        /// <summary>
        /// A list of all branches of the conditional.
        /// </summary>
        public ObservableCollection<Branch> Cases { get; set; } = CreateDefaultCases(new BooleanConstant(), EvaluatableDefaults.Get<T>(), EvaluatableDefaults.Get<T>());

        /// <summary>Creates a new If-Else evaluatable with default evaluatables.</summary>
        public IfElseGeneric() { }
        /// <summary>Creates a new evaluatable that returns caseTrue if condition evaluates to true and caseFalse otherwise.</summary>
        public IfElseGeneric(IEvaluatable<bool> condition, IEvaluatable<T> caseTrue, IEvaluatable<T> caseFalse) : this() { Cases = CreateDefaultCases(condition, caseTrue, caseFalse); }
        /// <summary>Creates a new evaluatable using the given case tree.</summary>
        public IfElseGeneric(ObservableCollection<Branch> cases) : this() { Cases = cases; }

        Control_Ternary<T> control;
        public UIElement GetControl(Application application) => control ?? (control = new Control_Ternary<T>(this, application));

        /// <summary>Evaluate conditions and return the appropriate evaluation.</summary>
        public T Evaluate(IGameState gameState) {
            foreach (var branch in Cases)
                if (branch.Condition == null || branch.Condition.Evaluate(gameState)) // Find the first with a true condition, or where the condition is null (which indicates 'else')
                    return branch.Value.Evaluate(gameState);
            return default(T);
        }

        object IEvaluatable.Evaluate(IGameState gameState) => Evaluate(gameState);

        /// <summary>Update the applications of the children evaluatables.</summary>
        public void SetApplication(Application application) {
            foreach (var kvp in Cases) {
                kvp.Condition?.SetApplication(application);
                kvp.Value?.SetApplication(application);
            }
        }

        public IEvaluatable<T> Clone() => new IfElseGeneric<T>(new ObservableCollection<Branch>(Cases));
        IEvaluatable IEvaluatable.Clone() => Clone();

        private static ObservableCollection<Branch> CreateDefaultCases(IEvaluatable<bool> condition, IEvaluatable<T> caseTrue, IEvaluatable<T> caseFalse) =>
            new ObservableCollection<Branch> {
                new Branch(condition, caseTrue),
                new Branch(null, caseFalse)
            };

        public class Branch {
            public IEvaluatable<bool> Condition { get; set; }
            public IEvaluatable<T> Value { get; set; }

            public Branch() { }
            public Branch(IEvaluatable<bool> condition, IEvaluatable<T> value) { Condition = condition; Value = value; }
        }
    }


    [Evaluatable("If - Else If - Else", category: OverrideLogicCategory.Logic)]
    public class IfElseBoolean : IfElseGeneric<bool> {
        /// <summary>Creates a new If-Else evaluatable with default evaluatables.</summary>
        public IfElseBoolean() : base() { }

        /// <summary>Creates a new evaluatable that returns caseTrue if condition evaluates to true and caseFalse otherwise.</summary>
        public IfElseBoolean(IEvaluatable<bool> condition, IEvaluatable<bool> caseTrue, IEvaluatable<bool> caseFalse) : base(condition, caseTrue, caseFalse) { }

        /// <summary>Creates a new evaluatable using the given case tree.</summary>
        public IfElseBoolean(ObservableCollection<Branch> cases) : base(cases) { }
    }


    [Evaluatable("If - Else If - Else", category: OverrideLogicCategory.Logic)]
    public class IfElseNumeric : IfElseGeneric<double> {
        /// <summary>Creates a new If-Else evaluatable with default evaluatables.</summary>
        public IfElseNumeric() : base() { }

        /// <summary>Creates a new evaluatable that returns caseTrue if condition evaluates to true and caseFalse otherwise.</summary>
        public IfElseNumeric(IEvaluatable<bool> condition, IEvaluatable<double> caseTrue, IEvaluatable<double> caseFalse) : base(condition, caseTrue, caseFalse) { }

        /// <summary>Creates a new evaluatable using the given case tree.</summary>
        public IfElseNumeric(ObservableCollection<Branch> cases) : base(cases) { }
    }


    [Evaluatable("If - Else If - Else", category: OverrideLogicCategory.Logic)]
    public class IfElseString : IfElseGeneric<string> {
        /// <summary>Creates a new If-Else evaluatable with default evaluatables.</summary>
        public IfElseString() : base() { }

        /// <summary>Creates a new evaluatable that returns caseTrue if condition evaluates to true and caseFalse otherwise.</summary>
        public IfElseString(IEvaluatable<bool> condition, IEvaluatable<string> caseTrue, IEvaluatable<string> caseFalse) : base(condition, caseTrue, caseFalse) { }

        /// <summary>Creates a new evaluatable using the given case tree.</summary>
        public IfElseString(ObservableCollection<Branch> cases) : base(cases) { }
    }


    [Evaluatable("If - Else If - Else", category: OverrideLogicCategory.Logic)]
    public class IfElseColor : IfElseGeneric<Color> {
        /// <summary>Creates a new If-Else evaluatable with default evaluatables.</summary>
        public IfElseColor() : base() { }

        /// <summary>Creates a new evaluatable that returns caseTrue if condition evaluates to true and caseFalse otherwise.</summary>
        public IfElseColor(IEvaluatable<bool> condition, IEvaluatable<Color> caseTrue, IEvaluatable<Color> caseFalse) : base(condition, caseTrue, caseFalse) { }

        /// <summary>Creates a new evaluatable that returns caseTrue if condition evaluates to true and caseFalse otherwise.</summary>
        public IfElseColor(IEvaluatable<bool> condition, Color caseTrue, Color caseFalse) : base(condition, new ColorConstant(caseTrue), new ColorConstant(caseFalse)) { }

        /// <summary>Creates a new evaluatable using the given case tree.</summary>
        public IfElseColor(ObservableCollection<Branch> cases) : base(cases) { }
    }


    [Evaluatable("If - Else If - Else", category: OverrideLogicCategory.Logic)]
    public class IfElseKeySequence : IfElseGeneric<KeySequence> {
        /// <summary>Creates a new If-Else evaluatable with default evaluatables.</summary>
        public IfElseKeySequence() : base() { }

        /// <summary>Creates a new evaluatable that returns caseTrue if condition evaluates to true and caseFalse otherwise.</summary>
        public IfElseKeySequence(IEvaluatable<bool> condition, IEvaluatable<KeySequence> caseTrue, IEvaluatable<KeySequence> caseFalse) : base(condition, caseTrue, caseFalse) { }
        
        /// <summary>Creates a new evaluatable using the given case tree.</summary>
        public IfElseKeySequence(ObservableCollection<Branch> cases) : base(cases) { }
    }
}
