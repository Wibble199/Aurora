using Aurora.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Aurora.Controls {

    /// <summary>
    /// A customised dropdown-like control that is designed explicitly for allowing a user to select a Game State Parameter.
    /// </summary>
    public partial class Control_GameStateParameterPicker : UserControl, INotifyPropertyChanged {

        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler<SelectedPathChangedEventArgs> SelectedPathChanged;

        public Control_GameStateParameterPicker() {
            InitializeComponent();
            ((FrameworkElement)Content).DataContext = this; // Cannot set to `this` as this causes any bindings on it to break if they use datacontext so we set it on the only child of the user control
        }

        #region IsOpen Property
        /// <summary>Whether or not the picker is open.</summary>
        public bool IsOpen {
            get => (bool)GetValue(IsOpenProperty);
            set => SetValue(IsOpenProperty, value);
        }

        public static readonly DependencyProperty IsOpenProperty =
            DependencyProperty.Register("IsOpen", typeof(bool), typeof(Control_GameStateParameterPicker), new PropertyMetadata(false));
        #endregion

        #region SelectedPath Property
        /// <summary>The path of the GameState variable the user has chosen.</summary>
        public string SelectedPath {
            get => (string)GetValue(SelectedPathProperty);
            set => SetValue(SelectedPathProperty, value);
        }
        public static readonly DependencyProperty SelectedPathProperty =
            DependencyProperty.Register("SelectedPath", typeof(string), typeof(Control_GameStateParameterPicker), new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, SelectedPathChange));

        private static void SelectedPathChange(DependencyObject sender, DependencyPropertyChangedEventArgs e) {
            // Do nothing if the value hasn't actually changed.
            if (e.OldValue == e.NewValue || e.NewValue == null) return;

            var picker = (Control_GameStateParameterPicker)sender;
            if (double.TryParse(e.NewValue.ToString(), out double val)) {
                // If a raw number has been entered, fill in the numeric stepper
                picker.numericEntry.Value = val;
            } else {
                // Else if an actual path has been given, split it up into it's ""directories""
                // For the path to be valid (and to be passed as a param to this method) it will be a path to a variable, not a "directory". We use this assumption.
                picker.WorkingPath = new Stack<string>(e.NewValue.ToString().Split('/'));
                picker.WorkingPath.Pop(); // Remove the last one, since the working path should not include the actual var name
                picker.NotifyChanged("WorkingPath", "WorkingPathStr", "ParameterList", "MainParameterListItems"); // All these things will be different now, so trigger an update of anything requiring them
                picker.mainListBox.SelectedItem = e.NewValue.ToString().Split('/').Last(); // The selected item in the list will be the last part of the path
            }

            // Raise an event informing subscribers
            picker.SelectedPathChanged?.Invoke(picker, new SelectedPathChangedEventArgs { OldPath = e.OldValue.ToString(), NewPath = e.NewValue.ToString() });
        }
        #endregion

        #region PropertyType Property
        /// <summary>The type of game state properties that should be shown in the control.</summary>
        public PropertyType PropertyType {
            get => (PropertyType)GetValue(PropertyTypeProperty);
            set { SetValue(PropertyTypeProperty, value); parameterList = null; NotifyChanged("ParameterList"); }
        }

        /// <summary>This property refers to the type of game state properties that will be shown to the user.</summary>
        public static readonly DependencyProperty PropertyTypeProperty =
            DependencyProperty.Register("PropertyType", typeof(PropertyType), typeof(Control_GameStateParameterPicker), new PropertyMetadata(PropertyType.Number));
        #endregion

        #region Application Property
        /// <summary>The application that will have the available game state parameters looked up on.</summary>
        public Profiles.Application Application {
            get => (Profiles.Application)GetValue(ApplicationProperty);
            set { SetValue(ApplicationProperty, value); parameterList = null; NotifyChanged("ParameterList"); }
        }

        /// <summary>The property that refers to the application that will provide the game state parameters.</summary>
        public static readonly DependencyProperty ApplicationProperty =
            DependencyProperty.Register("Application", typeof(Profiles.Application), typeof(Control_GameStateParameterPicker), new PropertyMetadata(null));
        #endregion

        #region Properties
        // The current path the user is in in the dialog.
        public Stack<string> WorkingPath { get; set; } = new Stack<string>();

        private List<string> parameterList;
        /// <summary>The read-only list of parameters for this application and property type.</summary>
        public List<string> ParameterList => parameterList ?? (parameterList = Application?.ParameterLookup?.GetParameters(PropertyType).ToList());

        /// <summary>The items that should be displayed in the main parameter list.</summary>
        public List<string> MainParameterListItems {
            get {
                // If the application or param lookup is null, we don't know the parameters so do nothing
                if (Application?.ParameterLookup == null) return null;

                // If the given working path is a path to a variable (which it shouldn't be), pop the last item (the variable name) from the path to give just the "directory"
                if (Application.ParameterLookup.IsValidParameter(WorkingPathStr))
                    WorkingPath.Pop();

                // Generate the string version of this working path (and cache it)
                var workingPath = WorkingPathStr;
                return ParameterList // With all properties in the current param lookup that are of a valid type (e.g. numbers)
                    .Where(path => path.StartsWith(workingPath)) // Pick only the ones that start with the same working path
                    .Select(path => path.Substring(workingPath == "" ? 0 : workingPath.Length + 1).Split('/').First()) // Select only the next part of the path
                    .Distinct() // And ensure there are no duplicates (get distinct elements) so that we don't show "directories" multiple times (e.g. only once will "LocalPCInfo" be shown)
                    .OrderBy(p => p.ToLowerInvariant()) // Order the items alphabetically (case insensitive)
                    .ToList();
            }
        }

        /// <summary>Returns the string representation of the current working path.</summary>
        public string WorkingPathStr => string.Join("/", WorkingPath.Reverse());
        #endregion

        #region Methods
        /// <summary>Calls the PropertyChanged event for each of the property names passed as parameters.</summary>
        private void NotifyChanged(params string[] propNames) {
            foreach (var prop in propNames)
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        /// <summary>Animates the list boxes.</summary>
        /// <param name="dx">Direction of animation. -1 for previous, 1 for next.</param>
        private void Animate(int dx) {
            GetScollViewer(auxillaryListbox).ScrollToVerticalOffset(GetScollViewer(mainListBox).VerticalOffset);

            // Move the aux to the centre and move the main to the side of it
            SetTransformRelativeOffset(mainListBox, dx);
            SetTransformRelativeOffset(mainListBox, 0);

            // Animate the aux moving away and the main moving in
            CreateStoryboard(dx, 0, mainListBox).Begin();
            CreateStoryboard(0, -dx, auxillaryListbox).Begin();
        }

        /// <summary>Creates a storyboard animation that changes the TransformRelativeOffsetProperty property from `fromX` to `toX` for the given target.</summary>
        private Storyboard CreateStoryboard(int from, int to, UIElement target) {
            var sb = new Storyboard {
                Children = new TimelineCollection(new[] {
                    new DoubleAnimation(from, to, new Duration(new TimeSpan(0, 0, 0, 0, 300))) {
                        EasingFunction = new PowerEase { EasingMode = EasingMode.EaseInOut }
                    }
                })
            };
            Storyboard.SetTarget(sb, target);
            Storyboard.SetTargetProperty(sb, new PropertyPath(TransformRelativeOffsetProperty));
            return sb;
        }

        /// <summary>For the given listbox, gets its inner ScrollViewer component. Why this isn't a standard property on the ListBox idk.</summary>
        private ScrollViewer GetScollViewer(ListBox trg)
            => (ScrollViewer)VisualTreeHelper.GetChild(VisualTreeHelper.GetChild(trg, 0), 0);
        #endregion

        #region Event Handlers
        private void BackBtn_Click(object sender, RoutedEventArgs e) {
            if (WorkingPath.Count > 0) {
                // Make the aux list box take on the same items as the current one so that when animated (since the aux is moved to the middle first) it looks natural
                auxillaryListbox.ItemsSource = MainParameterListItems;

                Animate(-1);
                WorkingPath.Pop(); // Remove the last "directory" off the working path
                NotifyChanged("MainParameterListItems", "WorkingPathStr"); // These properties will have changed so any UI stuff that relies on it should update
            }
        }
        
        private void MainListBox_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            /* The reason this is a PreviewMouseLeftButtonDown event rather than using a SelectionChanged event is because I was having issues where when the next items
             * were loaded it was immediately selecting the first item and then selecting a path the user didn't click. There is no click event available for the ListBox
             * so that was out of the question. Next I tried double click but it was counter-intuitive as items were being selected by not actually chosen, which could
             * easily confuse users. It also felt unresponsive as it would be a few hundred milliseconds after the double click that it actually called this method.
             * Finally preview left down was chosen as, after a bit of tweaking, it seemed to work properly - felt responsive and didn't repeatedly select new items. The
             * only real downside of this is that the user can't use the up and down arrows to change the selected item but I think that's a small price to pay.
             * Side note: THIS PICKER HAS TAKEN ME SO DAMN LONG TO MAKE. Probably longer than the actual GSI plugin system itself.... But hey, I'm proud of it. */

            // Element selection code is adapted from http://kevin-berridge.blogspot.com/2008/06/wpf-listboxitem-double-click.html
            var el = (UIElement)mainListBox.InputHitTest(e.GetPosition(mainListBox));
            while (el != null && el != mainListBox) {
                if (el is ListBoxItem item) {

                    // Since the user has picked an item on the list, we want to clear the numeric box so it is obvious to the user that the number is having no effect.
                    numericEntry.Value = null;

                    // Copy the current list items to the aux list box incase the list box is animated later. This must be done BEFORE the workingpath.push call.
                    auxillaryListbox.ItemsSource = MainParameterListItems;

                    // Add the clicked item to the working path (even if it is an end variable, not a "directory")
                    WorkingPath.Push(item.Content.ToString());

                    var path = string.Join("/", WorkingPath.Reverse());
                    if (Application?.ParameterLookup?.IsValidParameter(path) ?? false) {
                        // If it turns out the user has selected an end variable, we want to update the DependencyObject for the selected path
                        SelectedPath = path;
                        NotifyChanged("SelectedPath");
                    } else {
                        // If the user has selected a directory instead (i.e. isn't not a valid parameter) then perform the animation since there will now be new properties to choose from
                        Animate(1);
                    }

                    // Regardless of whether it was a variable or a directory, the list and path will have changed
                    NotifyChanged("MainParameterListItems", "WorkingPathStr");
                }
                el = (UIElement)VisualTreeHelper.GetParent(el);
            }
        }

        private void NumericEntry_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e) {
            var v = (sender as Xceed.Wpf.Toolkit.DoubleUpDown).Value;

            // If there is no value, then this will have been set programmatically, so do nothing since we don't want to end up in a change event handler loop
            if (!v.HasValue) return;

            mainListBox.SelectedItem = null; // Clear the selection on the list box (to emphasise to the user it is now irrelevant)
            SelectedPath = v.ToString(); // Set the selectedpath to be the value of this numeric stepper
            NotifyChanged("SelectedPath");
        }
        #endregion

        #region TransformRelativeOffset Property
        public static double GetTransformRelativeOffset(DependencyObject obj) => (double)obj.GetValue(TransformRelativeOffsetProperty);
        public static void SetTransformRelativeOffset(DependencyObject obj, double value) => obj.SetValue(TransformRelativeOffsetProperty, value);

        public static readonly DependencyProperty TransformRelativeOffsetProperty =
            DependencyProperty.RegisterAttached("TransformRelativeOffset", typeof(double), typeof(Control_GameStateParameterPicker), new PropertyMetadata(0d));
        #endregion
    }

    /// <summary>
    /// Event that is raised when the selected path or value changes.
    /// </summary>
    public class SelectedPathChangedEventArgs : EventArgs {
        public string OldPath { get; set; }
        public string NewPath { get; set; }
    }

    /// <summary>
    /// Converter that checks to see if a string is null or whitespace. Returns true if it HAS A VALUE, or false if it IS NULL/WHITESPACE.
    /// </summary>
    public class IsStringNotNullOrWhitespaceConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => !string.IsNullOrWhiteSpace(value?.ToString());
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => new NotImplementedException();
    }

    /// <summary>
    /// Converter that converts a PropertyType enum value to a GridLength. Used for binding onto one of the row definition properties to hide a row when
    /// the property type is anything other than <see cref="PropertyType.Number" />.
    /// </summary>
    public class PropertyTypeToGridLengthConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => new GridLength(0, (PropertyType) value == PropertyType.Number? GridUnitType.Auto : GridUnitType.Pixel);
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => new NotImplementedException();
    }

    /// <summary>
    /// A binding that will create a <see cref="TranslateTransform"/> based on the target's TransformRelativeOffset. The value will be relative to the
    /// size of the element (e.g. if the element's width is 150 and the TransformRelativeOffset is 0.5, then the TranslateTransform's X will be 75).
    /// </summary>
    public class DoubleToRelativeTransformOffset : MultiBinding {

        public DoubleToRelativeTransformOffset() {
            Bindings.Add(new Binding("ActualWidth") { RelativeSource = new RelativeSource(RelativeSourceMode.Self) });
            Bindings.Add(new Binding() { Path = new PropertyPath(Control_GameStateParameterPicker.TransformRelativeOffsetProperty), RelativeSource = new RelativeSource(RelativeSourceMode.Self) });
            Converter = new Conv();
        }

        class Conv : IMultiValueConverter {
            public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) => new TranslateTransform((double)values[0] * (double)values[1], 0);
            public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotImplementedException();
        }
    }
}
