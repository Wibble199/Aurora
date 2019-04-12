using Aurora.Profiles;
using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Linq;

namespace Aurora.Settings.Overrides.Logic {

    /// <summary>
    /// Interface that defines a logic operand that can be evaluated into a value. Should also have a Visual control that can
    /// be used to edit the operand. The control will be given the current application that can be used to have contextual
    /// prompts (e.g. a dropdown list with the valid game state variable paths) for that application.
    /// </summary>
    public interface IEvaluatable {
        /// <summary>Should evaluate the operand and return the evaluation result.</summary>
        object Evaluate(IGameState gameState);

        /// <summary>Should return a control that is bound to this logic element.</summary>
        Visual GetControl(Application application);

        /// <summary>Indicates the UserControl should be updated with a new application.</summary>
        void SetApplication(Application application);

        /// <summary>Creates a copy of this IEvaluatable.</summary>
        IEvaluatable Clone();
    }

    /// <summary>
    /// Interface that defines an IEvaluatable that will evaluate into a specific type (e.g. bool).
    /// </summary>
    public interface IEvaluatable<T> : IEvaluatable
    {
        /// <summary>Should evaluate this instance and return the evaluation result.</summary>
        new T Evaluate(IGameState gameState);

        /// <summary>Creates a copy of this IEvaluatable.</summary>
        new IEvaluatable<T> Clone();
    }


    /// <summary>
    /// Class that provides a lookup for the default Evaluatable for a particular type.
    /// </summary>
    public static class EvaluatableDefaults {

        private static Dictionary<Type, Type> defaultsMap = new Dictionary<Type, Type> {
            { typeof(bool), typeof(BooleanConstant) },
            { typeof(int), typeof(NumberConstant) },
            { typeof(long), typeof(NumberConstant) },
            { typeof(float), typeof(NumberConstant) },
            { typeof(double), typeof(NumberConstant) },
            { typeof(string), typeof(StringConstant) },
            { typeof(System.Drawing.Color), typeof(ColorConstant) },
            { typeof(KeySequence), typeof(KeySequenceConstant) }
        };

        public static IEvaluatable<T> Get<T>() => (IEvaluatable<T>)Get(typeof(T));

        public static IEvaluatable Get(Type t) {
            if (!defaultsMap.TryGetValue(t, out Type def))
                throw new ArgumentException($"Type '{t.Name}' does not have a default evaluatable type.");
            return (IEvaluatable)Activator.CreateInstance(def);
        }
    }
}
