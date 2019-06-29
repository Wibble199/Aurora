using Aurora.Controls;
using Aurora.Settings.Layers;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Aurora.Settings {

    public partial class Control_LayerList : UserControl, INotifyPropertyChanged {
        
        public Control_LayerList() {
            InitializeComponent();
            ((FrameworkElement)Content).DataContext = this;
        }

        #region PropertyChanged Event (and helpers)
        // When C# 8 comes out with default implementations for interfaces, I will probably move these into an interface to be used anywhere.

        /// <summary>Event that fires when a property changes.</summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Sets a dependency property to a specific value and raises change events for the property and all additional properties if the value is different.
        /// </summary>
        private void SetValueNotify(DependencyProperty dp, object val, string[] additionalProperties = null, [CallerMemberName] string propName = null) {
            if (GetValue(dp) == val) return;
            SetValue(dp, val);
            Notify(additionalProperties, propName);
        }

        /// <summary>
        /// Raises change events for the property and all additional properties.
        /// </summary>
        private void Notify(string[] additionalProperties, string propName) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
            Notify(additionalProperties);
        }

        /// <summary>
        /// Raises change events for the properties.
        /// </summary>
        /// <param name="properties"></param>
        private void Notify(params string[] properties) {
            foreach (var prop in properties)
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        /// <summary>
        /// Helper funtion that creates a PropertyChangedCallback for PropertyMetadata that notifies the provided property names have changed.
        /// </summary>
        private static PropertyChangedCallback NotifyCb(params string[] properties) {
            return (trg, e) => ((Control_LayerList)trg).Notify(properties);
        }
        #endregion

        #region Properties
        #region FocusedApplication Property
        public Profiles.Application FocusedApplication {
            get => (Profiles.Application)GetValue(FocusedApplicationProperty);
            set => SetValueNotify(FocusedApplicationProperty, value);
        }

        public static readonly DependencyProperty FocusedApplicationProperty =
            DependencyProperty.Register("FocusedApplication", typeof(Profiles.Application), typeof(Control_LayerList), new PropertyMetadata(null));
        #endregion

        #region LayerCollection Property
        /// <summary>
        /// This collection is a primary set of layers to display to the user.
        /// </summary>
        public ObservableCollection<Layer> LayerCollection {
            get => (ObservableCollection<Layer>)GetValue(LayerCollectionProperty);
            set => SetValue(LayerCollectionProperty, value);
        }
        
        public static readonly DependencyProperty LayerCollectionProperty =
            DependencyProperty.Register("LayerCollection", typeof(ObservableCollection<Layer>), typeof(Control_LayerList), new PropertyMetadata(null, NotifyCb("ActiveLayerCollection", "DayNightCheckboxesVisiblity")));
        #endregion

        #region SelectedLayer Property
        /// <summary>
        /// The currently selected layer.
        /// </summary>
        public Layer SelectedLayer {
            get => (Layer)GetValue(SelectedLayerProperty);
            set => SetValue(SelectedLayerProperty, value);
        }
        
        public static readonly DependencyProperty SelectedLayerProperty =
            DependencyProperty.Register("SelectedLayer", typeof(Layer), typeof(Control_LayerList), new PropertyMetadata(null, OnSelectedLayerPropertyChanged));

        private static void OnSelectedLayerPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e) {
            if (e.NewValue is Layer layer)
                layer.SetProfile(((Control_LayerList)sender).FocusedApplication);
        }
        #endregion

        #region ListTitle Property
        /// <summary>
        /// The label to display at the top of this layers list.
        /// </summary>
        public string ListTitle {
            get => (string)GetValue(ListTitleProperty);
            set => SetValue(ListTitleProperty, value);
        }
        
        public static readonly DependencyProperty ListTitleProperty =
            DependencyProperty.Register("ListTitle", typeof(string), typeof(Control_LayerList), new PropertyMetadata("Layers"));
        #endregion
        #endregion

        #region Methods
        /// <summary>
        /// Adds a new layer to the currently active collection. Will also setup the event listener to make the profile save and set the layer's application.
        /// </summary>
        private void AddLayer(Layer layer) {
            layer.AnythingChanged += FocusedApplication.SaveProfilesEvent;
            layer.SetProfile(FocusedApplication);
            LayerCollection.Insert(0, layer);
            SelectedLayer = layer;
        }

        #region Event Handlers
        /// <summary>
        /// When the add button is clicked, adds a new default layer.
        /// </summary>
        private void AddButton_Click(object sender, RoutedEventArgs e) {
            AddLayer(new Layer("New layer " + Utils.Time.GetMilliSeconds()));
        }
        
        /// <summary>
        /// Creates a clone of the selected layer and sets <see cref="Global.Clipboard"/> to the clone. The reason for cloning is so that should the layer be
        /// changed after copying, the pasted version will not have this changes (as should be expected).
        /// </summary>
        private void CopyButton_Click(object sender, RoutedEventArgs e) {
            Global.Clipboard = SelectedLayer?.Clone();
        }

        /// <summary>
        /// Checks if the <see cref="Global.Clipboard"/> object is a layer, and if so adds a copy of this layer to the active collection. The reason for taking
        /// a clone of the layer is so that if it was to be pasted again, the two pasted layers don't equal one another (i.e. don't have the same reference).
        /// </summary>
        private void PasteButton_Click(object sender, RoutedEventArgs e) {
            // Check if clipboard is layer and also that either: The layer on the clipboard is available to ALL applications OR the layer is available to the current application type.
            // This check is to avoid being able to copy application specific layers to other applications, e.g. prevent copying Minecraft health layer to CSGO.
            if (Global.Clipboard is Layer clipboardLayer && (Global.LightingStateManager.DefaultLayerHandlers.Contains(clipboardLayer.Handler.ID) || FocusedApplication.Config.ExtraAvailableLayers.Contains(clipboardLayer.Handler.ID))) {
                var newLayer = (Layer)clipboardLayer.Clone();
                newLayer.Name += " - Copy";
                AddLayer(newLayer);
            }
        }

        /// <summary>
        /// Asks the user if they wish to delete the currently selected layer and does so if they press "Yes".
        /// </summary>
        private async void DeleteButton_Click(object sender, RoutedEventArgs e) {
            if (LayerCollection.Contains(SelectedLayer) && await AlertBox.ShowDelete(this, Localization.TranslationSource.Instance["layer"], SelectedLayer.Name)) {
                LayerCollection.Remove(SelectedLayer);
                SelectedLayer = null;
            }
        }

        /// <summary>
        /// Keyboard shortcut listener. Reroutes the call to the relevant button's click handler.
        /// </summary>
        private void ReorderableListBox_KeyDown(object sender, KeyEventArgs e) {
            if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control)) {
                if (e.Key == Key.C)
                    CopyButton_Click(sender, null);
                else if (e.Key == Key.V)
                    PasteButton_Click(sender, null);
            } else if (e.Key == Key.Delete)
                DeleteButton_Click(sender, null);
        }

        /// <summary>
        /// When one of the collection radiobuttons (e.g. nighttime/daytime) changes, then notify that the <see cref="ActiveLayerCollection"/>
        /// property will have changed, thus updating the layer list.
        /// </summary>
        private void CollectionSelection_Checked(object sender, RoutedEventArgs e) {
            Notify("ActiveLayerCollection");
        }
        #endregion
        #endregion
    }
}
