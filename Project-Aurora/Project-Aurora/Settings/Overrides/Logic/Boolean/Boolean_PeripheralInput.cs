using Aurora.Controls;
using Aurora.Profiles;
using Aurora.Utils;
using System.Diagnostics;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;
using Xceed.Wpf.Toolkit;
using Keys = System.Windows.Forms.Keys;

namespace Aurora.Settings.Overrides.Logic {

    /// <summary>
    /// Condition that is true when a specific keyboard button is held down.
    /// </summary>
    [Evaluatable("Key Held", category: OverrideLogicCategory.Input)]
    public class BooleanKeyDown : Evaluatable<bool, StringPropertyField> {

        /// <summary>Creates a new key held condition with the default key (Space) as the trigger key.</summary>
        public BooleanKeyDown() { }
        /// <summary>Creates a new key held condition with the given key as the trigger key.</summary>
        public BooleanKeyDown(Keys target) { TargetKey = target; }

        /// <summary>The key to be checked to see if it is held down.</summary>
        public Keys TargetKey { get; set; } = Keys.Space;

        /// <summary>Create a control where the user can select the key they wish to detect.</summary>
        public override StringPropertyField CreateControl() => new StringPropertyField { Type = typeof(Keys) }
            .WithBinding(StringPropertyField.ValueProperty, new Binding("TargetKey") { Source = this, Mode = BindingMode.TwoWay });

        /// <summary>True if the global event bus's pressed key list contains the target key.</summary>
        public override bool Evaluate(IGameState gameState) => Global.InputEvents.PressedKeys.Contains(TargetKey);

        public override IEvaluatable<bool> Clone() => new BooleanKeyDown { TargetKey = TargetKey };
    }


    /// <summary>
    /// Condition that is true when a specific mouse button is held down.
    /// </summary>
    [Evaluatable("Mouse Button Held", category: OverrideLogicCategory.Input)]
    public class BooleanMouseDown : Evaluatable<bool, StringPropertyField> {

        /// <summary>Creates a new key held condition with the default mouse button (Left) as the trigger button.</summary>
        public BooleanMouseDown() { }
        /// <summary>Creates a new key held condition with the given mouse button as the trigger button.</summary>
        public BooleanMouseDown(System.Windows.Forms.MouseButtons target) { TargetButton = target; }

        /// <summary>The mouse button to be checked to see if it is held down.</summary>
        public System.Windows.Forms.MouseButtons TargetButton { get; set; } = System.Windows.Forms.MouseButtons.Left;

        /// <summary>Create a control where the user can select the mouse button they wish to detect.</summary>
        public override StringPropertyField CreateControl() => new StringPropertyField { Type = typeof(System.Windows.Forms.MouseButtons) }
            .WithBinding(StringPropertyField.ValueProperty, new Binding("TargetButton") { Source = this, Mode = BindingMode.TwoWay });

        /// <summary>True if the global event bus's pressed mouse button list contains the target button.</summary>
        public override bool Evaluate(IGameState gameState) => Global.InputEvents.PressedButtons.Contains(TargetButton);

        public override IEvaluatable<bool> Clone() => new BooleanMouseDown { TargetButton = TargetButton };
    }


    /// <summary>
    /// Condition that is true when the specified lock key (e.g. caps lock) is active.
    /// </summary>
    [Evaluatable("Lock Key Active", category: OverrideLogicCategory.Input)]
    public class BooleanLockKeyActive : Evaluatable<bool, ComboBox> {

        /// <summary>Creates a new key held condition with the default lock type (CapsLock) as the lock type.</summary>
        public BooleanLockKeyActive() { }
        /// <summary>Creates a new key held condition with the given button as the lock type button.</summary>
        public BooleanLockKeyActive(Keys target) { TargetKey = target; }

        public Keys TargetKey { get; set; } = Keys.CapsLock;

        /// <summary>Create a control allowing the user to specify which lock key to check.</summary>
        public override ComboBox CreateControl() => new ComboBox { ItemsSource = new[] { Keys.CapsLock, Keys.NumLock, Keys.Scroll } }
            .WithBinding(ComboBox.SelectedValueProperty, new Binding("TargetKey") { Source = this, Mode = BindingMode.TwoWay });

        /// <summary>Return true if the target lock key is active.</summary>
        public override bool Evaluate(IGameState gameState) => System.Windows.Forms.Control.IsKeyLocked(TargetKey);

        public override IEvaluatable<bool> Clone() => new BooleanLockKeyActive { TargetKey = TargetKey };
    }


    /// <summary>
    /// An evaluatable that returns true when the specified time has elapsed without the user pressing a keyboard button or clicking the mouse.
    /// </summary>
    [Evaluatable("Away Timer", category: OverrideLogicCategory.Input)]
    public class BooleanAwayTimer : Evaluatable<bool, Control_TimeAndUnit> {

        /// <summary>Gets or sets the time before this timer starts returning true after there has been no user input.</summary>
        public double InactiveTime { get; set; }
        /// <summary>Gets or sets the time unit that is being used to measure the AFK time.</summary>
        public TimeUnit TimeUnit { get; set; }

        #region ctors
        /// <summary>Create a new away timer condition with the default time (60 seconds).</summary>
        public BooleanAwayTimer() : this(60) { }

        /// <summary>Creates a new away timer with the specified number of seconds before activating.</summary>
        public BooleanAwayTimer(double time) : this(time, TimeUnit.Seconds) { }

        /// <summary>Creates a new away timer with the specified time before activating.</summary>
        public BooleanAwayTimer(double time, TimeUnit unit) {
            InactiveTime = time;
            TimeUnit = unit;
        }
        #endregion

        /// <summary>Creates a control allowing the user to change the inactive time.</summary>
        public override Control_TimeAndUnit CreateControl() => new Control_TimeAndUnit()
            .WithBinding(Control_TimeAndUnit.TimeProperty, new Binding("InactiveTime") { Source = this, Mode = BindingMode.TwoWay })
            .WithBinding(Control_TimeAndUnit.UnitProperty, new Binding("TimeUnit") { Source = this, Mode = BindingMode.TwoWay });

        /// <summary>Checks to see if the duration since the last input is greater than the given inactive time.</summary>
        public override bool Evaluate(IGameState gameState) {
            var idleTime = ActiveProcessMonitor.GetLastInput();
            return TimeUnit switch {
                TimeUnit.Seconds => idleTime.TotalSeconds > InactiveTime,
                TimeUnit.Minutes => idleTime.TotalMinutes > InactiveTime,
                TimeUnit.Hours => idleTime.TotalHours > InactiveTime,
                _ => false
            };
        }

        public override IEvaluatable<bool> Clone() => new BooleanAwayTimer { InactiveTime = InactiveTime };
    }
    public enum TimeUnit { Seconds, Minutes, Hours }
}
