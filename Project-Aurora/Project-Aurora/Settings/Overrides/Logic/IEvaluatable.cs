using Aurora.Profiles;
using Aurora.Utils;
using System;
using System.Collections.Generic;
using System.Windows;
using Application = Aurora.Profiles.Application;

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
        UIElement GetControl(Application application);

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
    /// Helper class that is built on the <see cref="IEvaluatable{T}"/> and automatically implements some of the <see cref="IEvaluatable"/>
    /// methods. This class also caches the control created with the <see cref="CreateControl"/> method, which is available in the
    /// <see cref="Control"/> property.
    /// </summary>
    /// <remarks>This class is an optional class to extend, if you require more control it is still possible to implement the IEvalutable
    /// instead.</remarks>
    /// <typeparam name="TEvaluatable">The type of value that the Evaluatable will return.</typeparam>
    /// <typeparam name="TControl">The type of element that will be the control used by this evaluatable.</typeparam>
    public abstract class Evaluatable<TEvaluatable, TControl> : IEvaluatable<TEvaluatable>, IDisposable where TControl : UIElement {

        // All the properties that are on this evaluatable type.
        private static IReadOnlyDictionary<string, Member<Evaluatable<TEvaluatable, TControl>>> props
            = StringProperty.GeneratePropertyLookup<Evaluatable<TEvaluatable, TControl>>();

        private TControl control;

        /// <summary>The cached control for this evaluatable. Will create the control if it does not exist..</summary>
        [Newtonsoft.Json.JsonIgnore] protected TControl Control { get => control ?? (control = CreateControl()); }

        /// <summary>Creates a new instance of the control to use for this Evaluatable.</summary>
        public abstract TControl CreateControl();

        /// <summary>Calls the <see cref="SetApplication(Application)"/> for the control and returns it.</summary>
        public UIElement GetControl(Application application) {
            _ = Control; // Create the control if it doesn't exist
            SetApplication(application);
            return Control;
        }

        /// <summary>Updates the control with application-specific logic. May be omitted if not required.</summary>
        public virtual void SetApplication(Application application) { }

        /// <summary>Evaluates this IEvaluatable and returns the result.</summary>
        public abstract TEvaluatable Evaluate(IGameState gameState);
        /// <summary>Evaluates this IEvaluatable and returns the result, boxed as an object.</summary>
        object IEvaluatable.Evaluate(IGameState gameState) => Evaluate(gameState);

        /// <summary>Creates a clone of this IEvaluatable, cloning all properties (if ICloneable) also.</summary>
        public virtual IEvaluatable<TEvaluatable> Clone() {
            var inst = (Evaluatable<TEvaluatable, TControl>)GetType().New();
            foreach (var prop in props.Values) {
                var v = prop.Get(this);
                if (v is ICloneable c) v = c.Clone();
                prop.Set(inst, v);
            }
            return inst;
        }

        /// <summary>Creates a clone of this IEvaluatable, cloning any child evaluatables also.</summary>
        IEvaluatable IEvaluatable.Clone() => Clone();

        /// <summary>Disposes this evaluatable, also disposing any child property values that need disposing (e.g. other evaluatables).</summary>
        public virtual void Dispose() {
            foreach (var kvp in props) {
                var val = kvp.Value.Get(this);
                (val as IDisposable)?.Dispose();
            }
        }
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
            return (IEvaluatable)def.New();
        }
    }
}
