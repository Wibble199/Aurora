using Aurora.Profiles;
using Aurora.Utils;
using SharpDX.RawInput;
using System;
using System.Diagnostics;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using Xceed.Wpf.Toolkit;
using Keys = System.Windows.Forms.Keys;

namespace Aurora.Settings.Overrides.Logic {

    /// <summary>
    /// Condition that is true when a specific keyboard button is held down.
    /// </summary>
    [Evaluatable("Key Held", category: OverrideLogicCategory.Input)]
    public class BooleanKeyDown : IEvaluatable<bool> {

        /// <summary>Creates a new key held condition with the default key (Space) as the trigger key.</summary>
        public BooleanKeyDown() { }
        /// <summary>Creates a new key held condition with the given key as the trigger key.</summary>
        public BooleanKeyDown(Keys target) { TargetKey = target; }

        /// <summary>The key to be checked to see if it is held down.</summary>
        public Keys TargetKey { get; set; } = Keys.Space;

        /// <summary>Create a control where the user can select the key they wish to detect.</summary>
        public Visual GetControl(Application application) {
            var c = new Controls.Control_FieldPresenter { Type = typeof(Keys), Margin = new System.Windows.Thickness(0, 0, 0, 6) };
            c.SetBinding(Controls.Control_FieldPresenter.ValueProperty, new Binding("TargetKey") { Source = this, Mode = BindingMode.TwoWay });
            return c;
        }

        /// <summary>True if the global event bus's pressed key list contains the target key.</summary>
        public bool Evaluate(IGameState gameState) => Global.InputEvents.PressedKeys.Contains(TargetKey);
        object IEvaluatable.Evaluate(IGameState gameState) => Evaluate(gameState);

        /// <summary>Do nothing - this is an application-independent condition.</summary>
        public void SetApplication(Application application) { }

        public IEvaluatable<bool> Clone() => new BooleanKeyDown { TargetKey = TargetKey };
        IEvaluatable IEvaluatable.Clone() => Clone();
    }


    /// <summary>
    /// Condition that is true when a specific keyboard button is held down.
    /// </summary>
    [Evaluatable("Key Press (Retain for duration)", category: OverrideLogicCategory.Input)]
    public class BooleanKeyDownWithTimer : IEvaluatable<bool> {
        private Stopwatch watch = new Stopwatch();

        /// <summary>Creates a new key held condition with the default key (Space) as the trigger key.</summary>
        public BooleanKeyDownWithTimer() { }
        /// <summary>Creates a new key held condition with the given key as the trigger key.</summary>
        public BooleanKeyDownWithTimer(Keys target, float seconds) : this() { TargetKey = target; Seconds = seconds; }

        /// <summary>The key to be checked to see if it is held down.</summary>
        public Keys TargetKey { get; set; } = Keys.Space;
        public float Seconds { get; set; } = 1;

        /// <summary>Create a control where the user can select the key they wish to detect.</summary>
        public Visual GetControl(Application application) {
            StackPanel panel = new StackPanel();

            var c = new Controls.Control_FieldPresenter { Type = typeof(Keys), Margin = new System.Windows.Thickness(0, 0, 0, 6) };
            c.SetBinding(Controls.Control_FieldPresenter.ValueProperty, new Binding("TargetKey") { Source = this, Mode = BindingMode.TwoWay });
            panel.Children.Add(c);

            StackPanel time = new StackPanel();
            time.Orientation = Orientation.Horizontal;
            var text = new TextBlock();
            text.Text = "For";
            time.Children.Add(text);

            c = new Controls.Control_FieldPresenter { Type = typeof(float), Margin = new System.Windows.Thickness(5, 0, 5, 6) };
            c.SetBinding(Controls.Control_FieldPresenter.ValueProperty, new Binding("Seconds") { Source = this, Mode = BindingMode.TwoWay });
            time.Children.Add(c);

            text = new TextBlock();
            text.Text = "Seconds";
            time.Children.Add(text);

            panel.Children.Add(time);
            return panel;
        }
        /// <summary>True if the global event bus's pressed key list contains the target key.</summary>
        public bool Evaluate(IGameState gameState) {
            if (Global.InputEvents.PressedKeys.Contains(TargetKey)) {
                watch.Restart();
                return true;
            } else if (watch.IsRunning && watch.Elapsed.TotalSeconds <= Seconds) {
                return true;
            } else
                watch.Stop();

            return false;
        }
        object IEvaluatable.Evaluate(IGameState gameState) => Evaluate(gameState);

        /// <summary>Do nothing - this is an application-independent condition.</summary>
        public void SetApplication(Application application) { }

        public IEvaluatable<bool> Clone() => new BooleanKeyDown { TargetKey = TargetKey };
        IEvaluatable IEvaluatable.Clone() => Clone();
    }


    /// <summary>
    /// Condition that is true when a specific mouse button is held down.
    /// </summary>
    [Evaluatable("Mouse Button Held", category: OverrideLogicCategory.Input)]
    public class BooleanMouseDown : IEvaluatable<bool> {

        /// <summary>Creates a new key held condition with the default mouse button (Left) as the trigger button.</summary>
        public BooleanMouseDown() { }
        /// <summary>Creates a new key held condition with the given mouse button as the trigger button.</summary>
        public BooleanMouseDown(System.Windows.Forms.MouseButtons target) { TargetButton = target; }

        /// <summary>The mouse button to be checked to see if it is held down.</summary>
        public System.Windows.Forms.MouseButtons TargetButton { get; set; } = System.Windows.Forms.MouseButtons.Left;

        /// <summary>Create a control where the user can select the mouse button they wish to detect.</summary>
        public Visual GetControl(Application application) {
            var c = new Controls.Control_FieldPresenter { Type = typeof(System.Windows.Forms.MouseButtons), Margin = new System.Windows.Thickness(0, 0, 0, 6) };
            c.SetBinding(Controls.Control_FieldPresenter.ValueProperty, new Binding("TargetButton") { Source = this, Mode = BindingMode.TwoWay });
            return c;
        }

        /// <summary>True if the global event bus's pressed mouse button list contains the target button.</summary>
        public bool Evaluate(IGameState gameState) => Global.InputEvents.PressedButtons.Contains(TargetButton);
        object IEvaluatable.Evaluate(IGameState gameState) => Evaluate(gameState);

        /// <summary>Do nothing - this is an application-independent condition.</summary>
        public void SetApplication(Application application) { }

        public IEvaluatable<bool> Clone() => new BooleanMouseDown { TargetButton = TargetButton };
        IEvaluatable IEvaluatable.Clone() => Clone();
    }


    /// <summary>
    /// Condition that is true when the specified lock key (e.g. caps lock) is active.
    /// </summary>
    [Evaluatable("Lock Key Active", category: OverrideLogicCategory.Input)]
    public class BooleanLockKeyActive : IEvaluatable<bool> {

        /// <summary>Creates a new key held condition with the default lock type (CapsLock) as the lock type.</summary>
        public BooleanLockKeyActive() { }
        /// <summary>Creates a new key held condition with the given button as the lock type button.</summary>
        public BooleanLockKeyActive(Keys target) { TargetKey = target; }

        public Keys TargetKey { get; set; } = Keys.CapsLock;

        /// <summary>Create a control allowing the user to specify which lock key to check.</summary>
        public Visual GetControl(Application application) {
            var cb = new ComboBox { ItemsSource = new[] { Keys.CapsLock, Keys.NumLock, Keys.Scroll } };
            cb.SetBinding(ComboBox.SelectedValueProperty, new Binding("TargetKey") { Source = this, Mode = BindingMode.TwoWay });
            return cb;
        }

        /// <summary>Return true if the target lock key is active.</summary>
        public bool Evaluate(IGameState gameState) => System.Windows.Forms.Control.IsKeyLocked(TargetKey);
        object IEvaluatable.Evaluate(IGameState gameState) => Evaluate(gameState);

        /// <summary>Do nothing - this is an application-independent condition.</summary>
        public void SetApplication(Application application) { }

        public IEvaluatable<bool> Clone() => new BooleanLockKeyActive { TargetKey = TargetKey };
        IEvaluatable IEvaluatable.Clone() => Clone();
    }


    /// <summary>
    /// An evaluatable that returns true when the specified time has elapsed without the user pressing a keyboard button or clicking the mouse.
    /// </summary>
    [Evaluatable("Away Timer", category: OverrideLogicCategory.Input)]
    public class BooleanAwayTimer : IEvaluatable<bool> {

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
        public Visual GetControl(Application application) {
            var value = new DoubleUpDown { Minimum = 0, Maximum = double.MaxValue, Margin = new System.Windows.Thickness(0, 0, 8, 0) };
            value.SetBinding(DoubleUpDown.ValueProperty, new Binding("InactiveTime") { Source = this, Mode = BindingMode.TwoWay });

            var unit = new ComboBox { ItemsSource = Utils.EnumToItemsSourceExtension.GetListFor(typeof(TimeUnit)), DisplayMemberPath = "Text", SelectedValuePath = "Value", Width = 100  };
            unit.SetBinding(ComboBox.SelectedValueProperty, new Binding("TimeUnit") { Source = this, Mode = BindingMode.TwoWay });

            var sp = new StackPanel { Orientation = Orientation.Horizontal };
            sp.Children.Add(value);
            sp.Children.Add(unit);
            return sp;
        }

        /// <summary>Checks to see if the duration since the last input is greater than the given inactive time.</summary>
        public bool Evaluate(IGameState gameState) {
            var idleTime = ActiveProcessMonitor.GetLastInput();
            return TimeUnit switch {
                TimeUnit.Seconds => idleTime.TotalSeconds > InactiveTime,
                TimeUnit.Minutes => idleTime.TotalMinutes > InactiveTime,
                TimeUnit.Hours => idleTime.TotalHours > InactiveTime,
                _ => false
            };
        }
        object IEvaluatable.Evaluate(IGameState gameState) => Evaluate(gameState);

        // Do nothing - this is an application-independent condition.
        public void SetApplication(Application application) { }

        public IEvaluatable<bool> Clone() => new BooleanAwayTimer { InactiveTime = InactiveTime };
        IEvaluatable IEvaluatable.Clone() => Clone();
    }
    public enum TimeUnit { Seconds, Minutes, Hours }
}
