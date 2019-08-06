using Aurora.Controls;
using Aurora.Profiles;
using Aurora.Utils;
using Newtonsoft.Json;
using System.Windows.Data;

namespace Aurora.Settings.Overrides.Logic {

    /// <summary>
    /// A constant KeySequence value that allows the user to select with a control.
    /// </summary>
    [Evaluatable("Key Sequence (From picker)", category: OverrideLogicCategory.Input)]
    public class KeySequenceConstant : Evaluatable<KeySequence, Controls.KeySequence> {

        public KeySequence Sequence { get; set; }

        /// <summary>Creates a new KeySequence constant with an empty sequence.</summary>
        public KeySequenceConstant() { Sequence = new KeySequence(); }

        /// <summary>Creates a new KeySequence constant with the given sequence.</summary>
        public KeySequenceConstant(KeySequence sequence) { Sequence = sequence; }

        public override Controls.KeySequence CreateControl() => new Controls.KeySequence { RecordingTag = "Key Sequence Constant", Title = "Key Sequence Constant" }
                .WithBinding(Controls.KeySequence.SequenceProperty, new Binding("Sequence") { Source = this, Mode = BindingMode.TwoWay });

        /// <summary>Evaluate conditions and return the appropriate evaluation.</summary>
        public override KeySequence Evaluate(IGameState gameState) => Sequence;

        /// <summary>Clones this KeySequenceConstant.</summary>
        public override IEvaluatable<KeySequence> Clone() => new KeySequenceConstant((KeySequence)Sequence.Clone());
    }


    /// <summary>
    /// A KeySequence that is set from given property values.
    /// </summary>
    [Evaluatable("Key Sequence (Freeform from values)", category: OverrideLogicCategory.Input)]
    public class KeySequenceFromValues : Evaluatable<KeySequence, Control_KeySequenceFromValues> {

        // Evaluatables used to build the freeform object
        public IEvaluatable<double> X { get; set; }
        public IEvaluatable<double> Y { get; set; }
        public IEvaluatable<double> Width { get; set; }
        public IEvaluatable<double> Height { get; set; }
        public IEvaluatable<double> Angle { get; set; }

        /// <summary>Creates a new KeySequence constant with the default sized and positioned freeform.</summary>
        public KeySequenceFromValues() : this(new NumberConstant(0), new NumberConstant(0)) { }

        /// <summary>Creates a new KeySequence with the given evaluatables.</summary>
        public KeySequenceFromValues(IEvaluatable<double> x, IEvaluatable<double> y, IEvaluatable<double> width = null, IEvaluatable<double> height = null, IEvaluatable<double> angle = null) {
            X = x ?? new NumberConstant(0);
            Y = y ?? new NumberConstant(0);
            Width = width ?? new NumberConstant(30);
            Height = height ?? new NumberConstant(30);
            Angle = angle ?? new NumberConstant(0);
        }

        /// <summary>Creates a new KeySequence with the given constant values.</summary>
        public KeySequenceFromValues(double x, double y, double width = 30, double height = 30, double angle = 0) {
            X = new NumberConstant(x);
            Y = new NumberConstant(y);
            Width = new NumberConstant(width);
            Height = new NumberConstant(height);
            Angle = new NumberConstant(angle);
        }

        public override Control_KeySequenceFromValues CreateControl() => new Control_KeySequenceFromValues(this);

        /// <summary>Evaluate conditions and return the appropriate evaluation.</summary>
        public override KeySequence Evaluate(IGameState gameState) => new KeySequence(new FreeFormObject(
            (float)X.Evaluate(gameState), (float)Y.Evaluate(gameState), // Position
            (float)Width.Evaluate(gameState), (float)Height.Evaluate(gameState), // Size
            (float)Angle.Evaluate(gameState) // Angle
        ));

        public override void SetApplication(Application app) {
            X.SetApplication(app);
            Y.SetApplication(app);
            Width.SetApplication(app);
            Height.SetApplication(app);
            Angle.SetApplication(app);
            Control.SetApplication(app);
        }

        /// <summary>Clones this KeySequenceConstant.</summary>
        public override IEvaluatable<KeySequence> Clone() => new KeySequenceFromValues();
    }
}
