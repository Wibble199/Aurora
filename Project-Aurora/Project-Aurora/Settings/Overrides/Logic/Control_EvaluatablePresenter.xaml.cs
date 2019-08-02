using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Aurora.Settings.Overrides.Logic {
    /// <summary>
    /// A control that can be used to show <see cref="IEvaluatable{T}"/> controls (and enable evaluatable editing) to the user.
    /// <para>Can set the type of value the presenter accepts using the <see cref="EvalType"/> dependency property.</para>
    /// </summary>
    public partial class Control_EvaluatablePresenter : UserControl {

        /// <summary>Event that fires when a new evaluatable replaces the current one. Note that this only fires when the user replaces
        /// the evaluatable by dropping a new one onto the presenter, not when it is changed by code.</summary>
        public event EventHandler<ExpressionChangeEventArgs> ExpressionChanged;

        #region Ctor
        /// <summary>Creates a new <see cref="Control_EvaluatablePresenter"/>.</summary>
        public Control_EvaluatablePresenter() : base() {
            InitializeComponent();
        }
        #endregion

        #region Properties
        #region Expression Property
        // Expression Property (the expression whose control this presenter should host)        
        public IEvaluatable Expression {
            get => (IEvaluatable)GetValue(ExpressionProperty);
            set => SetValue(ExpressionProperty, value);
        }
        public static readonly DependencyProperty ExpressionProperty =
            DependencyProperty.Register("Expression", typeof(IEvaluatable), typeof(Control_EvaluatablePresenter), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, OnExpressionChange));

        private static void OnExpressionChange(DependencyObject evaluatablePresenter, DependencyPropertyChangedEventArgs eventArgs) {
            if (eventArgs.NewValue == null || eventArgs.NewValue.Equals(eventArgs.OldValue)) return;
            var control = (Control_EvaluatablePresenter)evaluatablePresenter;
            var expr = (IEvaluatable)eventArgs.NewValue;
            control.EvaluatableControl.Content = expr?.GetControl(control.Application);
        }
        #endregion

        #region Application Property
        // Application Property (the application passed to the component's UserControl to allow it do detect GSI variables names and such)
        public Profiles.Application Application {
            get => (Profiles.Application)GetValue(ApplicationProperty);
            set => SetValue(ApplicationProperty, value);
        }
        public static readonly DependencyProperty ApplicationProperty =
            DependencyProperty.Register("Application", typeof(Profiles.Application), typeof(Control_EvaluatablePresenter), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, OnApplicationChange));

        private static void OnApplicationChange(DependencyObject evaluatablePresenter, DependencyPropertyChangedEventArgs eventArgs) {
            var control = (Control_EvaluatablePresenter)evaluatablePresenter;
            control.Expression?.SetApplication((Profiles.Application)eventArgs.NewValue);
        }
        #endregion

        #region EvalType Property
        // The subtype of evaluatable to restrict the user to (e.g. IEvaluatable<bool>)
        public Type EvalType {
            get => (Type)GetValue(EvalTypeProperty);
            set => SetValue(EvalTypeProperty, value);
        }
        public static readonly DependencyProperty EvalTypeProperty =
            DependencyProperty.Register("EvalType", typeof(Type), typeof(Control_EvaluatablePresenter), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, null, CoerceEvalType));

        // Coerce any numeric types into double
        private static Type[] evalTypeNumerics = new[] { typeof(int), typeof(long), typeof(short), typeof(float), typeof(decimal) };
        private static object CoerceEvalType(DependencyObject d, object value) =>
            evalTypeNumerics.Contains(value as Type) ? typeof(double) : value;
        #endregion

        #region Highlighted Property
        /// <summary>Write-only property to set the highlight status of this presenter.</summary>
        protected bool Highlighted {
            set {
                Background = value ? Brushes.Red : Brushes.Transparent;
            }
        }
        #endregion
        #endregion

        #region Methods
        #region Event Handlers
        /// <summary>
        /// Method that handles when the user is dragging something and their mouse enters this presenter. If the dragged data is accepted
        /// by this presenter, it's visual state will change to indicate to the user it will be accepted.
        /// </summary>
        protected override void OnDragEnter(DragEventArgs e) {
            // When the user is dragging something over this presenter, check to see if it can be accepted (if the evaluatable type of the
            // dragged data matches the type specified in the property).
            if (TryGetData(e.Data, out _)) {
                // If so, highlight this presenter to give users visual feedback of where it will land.
                Highlighted = true;

                // Mark the event as handled so when the event bubbles up, other parent presenters will not highlight themselves as if they
                // will accept the drag-drop evaluatable instead.
                e.Handled = true;
            }

            // If the dragged data can't be accepted, don't mark the event as handled which will allow bubbling of the event to parent
            // presenters to allow them to check to see if they can handle the data.
        }

        /// <summary>
        /// Method that handles when the user is dragging an item and their mouse leaves this presenter. Remove any highlighting that is
        /// currently set.
        /// </summary>
        protected override void OnDragLeave(DragEventArgs e) {
            Highlighted = false;
        }

        /// <summary>
        /// Method that fires when the mouse moves while dragging something over this presenter. If the dragged evaluatable data can be
        /// accepted by this presenter, will indicate which effect will occur depending on whether the user is holding the CTRL key.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnDragOver(DragEventArgs e) {
            // If the evaluatable data can be accepted by this presenter...
            if (TryGetData(e.Data, out _)) {
                // If the user is holding the CTRL key, indicate they will copy the evaluatable, otherwise they'll move it.
                e.Effects = (e.KeyStates & DragDropKeyStates.ControlKey) > 0 ? DragDropEffects.Copy : DragDropEffects.Move;
                // Mark the event as handled so any parent presenters don't attempt to override the effects with their own logic.
                e.Handled = true;
            }
        }

        /// <summary>
        /// Method to handle dropping a dragged item on the presenter. If the dragged item is an evaluatable and can be accepted, it will
        /// replaced the current expression (and trigger an event). If the item cannot be accepted, it is left unhandled so as to bubble.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnDrop(DragEventArgs e) {
            if (TryGetData(e.Data, out var eval)) {
                // Before we update this evaluatable, keep a reference to it for the event listener
                var oldExpression = Expression;

                // Update the expression depending on source and ctrl key:
                var source = e.Data.GetData("SourcePresenter") as Control_EvaluatablePresenter;
                var ctrlKey = (e.KeyStates & DragDropKeyStates.ControlKey) > 0;
                if (source != null && ctrlKey) {
                    // If ctrl key is down and we are dragging from another presenter (i.e. not a spawner), clone the existing evaluatable into this
                    Expression = eval.Clone();

                } else if (source != null && !ctrlKey) {
                    // If ctrl key is NOT down, and we are dragging from another presenter (not spawner), swap the evaluatable of that presenter with this one
                    // We also need to call the changed event on the other presenter (since this is not done by the DependencyProperty to prevent a invoker-subscriber loop with WPF updating itself)
                    source.Expression = Expression;
                    source.ExpressionChanged?.Invoke(source, new ExpressionChangeEventArgs { NewExpression = Expression, OldExpression = eval });
                    Expression = eval;

                } else {
                    // Otherwise if we are dragging from a spawner (regardless of whether the ctrl key is down) just set this expression to the data (the spawner
                    // is responsible for instantiating a new evaluatable for us).
                    Expression = eval;
                }

                // Raise an event to indicate the change.
                ExpressionChanged?.Invoke(this, new ExpressionChangeEventArgs { OldExpression = oldExpression, NewExpression = Expression });

                // Reset any highlighting that this presenter has due to dragging an acceptable item over this presenter.
                Highlighted = false;

                // Mark event as handled so parent presenters do not end up also handling the data.
                e.Handled = true;
            }
        }

        /// <summary>Event that is applied to the left drag area that allows the user to pick up this evaluatable.</summary>
        private void DragStartArea_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            var @do = new DataObject(Expression);
            @do.SetData("SourcePresenter", this);
            DragDrop.DoDragDrop(this, @do, DragDropEffects.Move | DragDropEffects.Copy);
        }
        #endregion

        /// <summary>Attempts to get IEvaluatable data from the supplied data object. Will return true/false indicating if data is of correct format
        /// (an <see cref="IEvaluatable{T}"/> where T matches the <see cref="EvalType"/> property.</summary>
        private bool TryGetData(IDataObject @do, out IEvaluatable evaluatable) {
            if (@do.GetData(@do.GetFormats().First(x => x != "SourcePresenter")) is IEvaluatable data && Utils.TypeUtils.ImplementsGenericInterface(data.GetType(), typeof(IEvaluatable<>), EvalType)) {
                evaluatable = data;
                return true;
            }
            evaluatable = null;
            return false;
        }
        #endregion
    }
    

    /// <summary>
    /// Event arguments passed to subscribers when the IEvaluatable expression changes on a <see cref="Control_EvaluatablePresenter"/>.
    /// </summary>
    public class ExpressionChangeEventArgs : EventArgs {
        public IEvaluatable OldExpression { get; set; }
        public IEvaluatable NewExpression { get; set; }
    }
}
