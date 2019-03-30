using Hardcodet.Wpf.TaskbarNotification;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Aurora.EffectsEngine;
using Aurora.Settings;
using Aurora.Controls;
using Aurora.Profiles.Generic_Application;
using System.IO;
using Aurora.Settings.Keycaps;
using Aurora.Profiles;
using Aurora.Settings.Layers;
using Aurora.Profiles.Aurora_Wrapper;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Data;
using System.Globalization;

namespace Aurora
{
    partial class ConfigUI : Window, INotifyPropertyChanged
    {
        Control_Settings settingsControl = new Control_Settings();
        Control_LayerControlPresenter layerPresenter = new Control_LayerControlPresenter();
        Control_ProfileControlPresenter profilePresenter = new Control_ProfileControlPresenter();

        EffectColor desktop_color_scheme = new EffectColor(0, 0, 0);

        EffectColor transition_color = new EffectColor();
        EffectColor current_color = new EffectColor();

        private float transitionamount = 0.0f;

        private FrameworkElement selected_item = null;
        private FrameworkElement _selectedManager = null;

        private bool settingsloaded = false;
        private bool shownHiddenMessage = false;

        private string saved_preview_key = "";

        private Timer virtual_keyboard_timer;
        private Stopwatch recording_stopwatch = new Stopwatch();
        private Grid virtial_kb = new Grid();

        private readonly double virtual_keyboard_width;
        private readonly double virtual_keyboard_height;

        private readonly double width;
        private readonly double height;

        LayerEditor layer_editor = new LayerEditor();


        public ConfigUI()
        {
            InitializeComponent();

            virtual_keyboard_height = this.keyboard_grid.Height;
            virtual_keyboard_width = this.keyboard_grid.Width;

            width = Width;
            height = Height;

            Global.kbLayout.KeyboardLayoutUpdated += KbLayout_KeyboardLayoutUpdated;

            ctrlProfileManager.ProfileSelected += CtrlProfileManager_ProfileSelected;
            
            settingsControl.DataContext = this;

            
        }

        internal void Display()
        {
            if (App.isSilent || Global.Configuration.start_silently)
            {
                this.Visibility = Visibility.Hidden;
                this.WindowStyle = WindowStyle.None;
                this.ShowInTaskbar = false;
                Hide();
            }
            else
            {
                this.Show();
            }
        }

        private void CtrlProfileManager_ProfileSelected(ApplicationProfile profile)
        {
            profilePresenter.Profile = profile;

            if (_selectedManager.Equals(this.ctrlProfileManager))
                SelectedControl = profilePresenter;   
        }

        private void CtrlLayerManager_ProfileOverviewRequest(UserControl profile_control)
        {
            if (SelectedControl != profile_control)
                SelectedControl = profile_control;
        }

        private void Layer_manager_NewLayer(Layer layer)
        {
            layerPresenter.Layer = layer;

            if (_selectedManager.Equals(this.ctrlLayerManager))
                SelectedControl = layerPresenter;
        }

        private void KbLayout_KeyboardLayoutUpdated(object sender)
        {
            virtial_kb = Global.kbLayout.Virtual_keyboard;

            keyboard_grid.Children.Clear();
            keyboard_grid.Children.Add(virtial_kb);
            keyboard_grid.Children.Add(new LayerEditor());

            keyboard_grid.Width = virtial_kb.Width;
            this.Width = width + (virtial_kb.Width - virtual_keyboard_width);

            keyboard_grid.Height = virtial_kb.Height;
            this.Height = height + (virtial_kb.Height - virtual_keyboard_height);

            keyboard_grid.UpdateLayout();

            keyboard_viewbox.MaxWidth = virtial_kb.Width + 50;
            keyboard_viewbox.MaxHeight = virtial_kb.Height + 50;
            keyboard_viewbox.UpdateLayout();

            this.UpdateLayout();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            

            if (!settingsloaded)
            {
                virtual_keyboard_timer = new Timer(100);
                virtual_keyboard_timer.Elapsed += new ElapsedEventHandler(virtual_keyboard_timer_Tick);
                virtual_keyboard_timer.Start();

                settingsloaded = true;
            }

            this.keyboard_record_message.Visibility = Visibility.Hidden;

            current_color = desktop_color_scheme;
            //bg_grid.Background = new SolidColorBrush(Color.FromRgb(desktop_color_scheme.Red, desktop_color_scheme.Green, desktop_color_scheme.Blue));

            virtial_kb = Global.kbLayout.Virtual_keyboard;

            keyboard_grid.Children.Clear();
            keyboard_grid.Children.Add(virtial_kb);
            keyboard_grid.Children.Add(new LayerEditor());

            keyboard_grid.Width = virtial_kb.Width;
            this.Width = width + (virtial_kb.Width - virtual_keyboard_width);

            keyboard_grid.Height = virtial_kb.Height;
            this.Height = height + (virtial_kb.Height - virtual_keyboard_height);

            keyboard_grid.UpdateLayout();

            keyboard_viewbox.MaxWidth = virtial_kb.Width + 50;
            keyboard_viewbox.MaxHeight = virtial_kb.Height + 50;
            keyboard_viewbox.UpdateLayout();

            UpdateManagerStackFocus(ctrlLayerManager);

            this.UpdateLayout();
        }

        public static bool ApplicationIsActivated()
        {
            var activatedHandle = GetForegroundWindow();
            if (activatedHandle == IntPtr.Zero)
                return false;       // No window is currently activated

            var procId = Process.GetCurrentProcess().Id;
            int activeProcId;
            GetWindowThreadProcessId(activatedHandle, out activeProcId);

            return activeProcId == procId;
        }

        [System.Runtime.InteropServices.DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto, ExactSpelling = true)]
        private static extern IntPtr GetForegroundWindow();

        [System.Runtime.InteropServices.DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto, SetLastError = true)]
        private static extern int GetWindowThreadProcessId(IntPtr handle, out int processId);

        private void virtual_keyboard_timer_Tick(object sender, EventArgs e)
        {
            if (!ApplicationIsActivated())
                return;

            Dispatcher.Invoke(() => {
                if (transitionamount <= 1.0f)
                {
                    transition_color.BlendColors(current_color, transitionamount += 0.07f);

                    //bg_grid.Background = new SolidColorBrush(Color.FromRgb(transition_color.Red, transition_color.Green, transition_color.Blue));
                    //bg_grid.UpdateLayout();
                }


                Dictionary<Devices.DeviceKeys, System.Drawing.Color> keylights = new Dictionary<Devices.DeviceKeys, System.Drawing.Color>();

                if (IsActive)
                {
                    keylights = Global.effengine.GetKeyboardLights();
                    Global.kbLayout.SetKeyboardColors(keylights);
                }

                if (Global.key_recorder.IsRecording())
                    this.keyboard_record_message.Visibility = Visibility.Visible;
                else
                    this.keyboard_record_message.Visibility = Visibility.Hidden;

            });
        }

        ////Misc
        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        private void trayicon_menu_quit_Click(object sender, RoutedEventArgs e)
        {
            exitApp();
        }

        private void trayicon_menu_settings_Click(object sender, RoutedEventArgs e)
        {
            ShowInTaskbar = true;
            Show();
        }

        private void Window_Initialized(object sender, EventArgs e)
        {
            
        }

        private void Window_Closing(object sender, CancelEventArgs e) {
            var closeMode = Global.Configuration.close_mode;

            if (closeMode == AppExitMode.Ask)
                closeMode = MessageBox.Show("Would you like to exit Aurora?", "Aurora", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes ? AppExitMode.Exit : AppExitMode.Minimize;

            if (closeMode == AppExitMode.Exit) {
                exitApp();
            } else {
                minimizeApp();
                e.Cancel = true;
            }
        }

        private void exitApp()
        {
            trayicon.Visibility = Visibility.Hidden;
            virtual_keyboard_timer?.Stop();
            System.Windows.Application.Current.Shutdown();
            Environment.Exit(0);
        }

        private void minimizeApp()
        {
            this.FocusedApplication?.SaveAll();

            if (!shownHiddenMessage)
            {
                trayicon.ShowBalloonTip("Aurora", "This program is now hidden in the tray.", BalloonIcon.Info);
                shownHiddenMessage = true;
            }

            Global.LightingStateManager.PreviewProfileKey = string.Empty;

            //Hide Window
            System.Windows.Application.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, (System.Windows.Threading.DispatcherOperationCallback)delegate (object o)
            {
                WindowStyle = WindowStyle.None;
                Hide();
                return null;
            }, null);
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            Global.LightingStateManager.PreviewProfileKey = saved_preview_key;
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            saved_preview_key = Global.LightingStateManager.PreviewProfileKey;
            Global.LightingStateManager.PreviewProfileKey = string.Empty;
        }

        private static void FocusedProfileChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            ConfigUI self = source as ConfigUI;
            Profiles.Application value = e.NewValue as Profiles.Application;
            
            self.gridManagers.Visibility = value == null ? Visibility.Collapsed : Visibility.Visible;

            if (value != null)            
                self.SelectedControl = value.Control;
        }

        private void RemoveProfile_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender != null && sender is Image && (sender as Image).Tag != null && (sender as Image).Tag is string)
            {
                string name = (sender as Image).Tag as string;

                if (Global.LightingStateManager.Events.ContainsKey(name))
                {
                    if (MessageBox.Show("Are you sure you want to delete profile for " + (((Profiles.Application)Global.LightingStateManager.Events[name]).Settings as GenericApplicationSettings).ApplicationName + "?", "Remove Profile", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                    {
                        var eventList = Global.Configuration.ProfileOrder;
                        string prevProfile = eventList[eventList.FindIndex(s => s.Equals(name)) - 1];
                        Global.LightingStateManager.RemoveGenericProfile(name);
                        //ConfigManager.Save(Global.Configuration);
                        //this.GenerateProfileStack(prevProfile);
                    }
                }
            }
        }
        
        public void ShowWindow()
        {
            Global.logger.Info("Show Window called");
            Visibility = Visibility.Visible;
            ShowInTaskbar = true;
            Show();
            Activate();
        }

        private void trayicon_TrayMouseDoubleClick(object sender, RoutedEventArgs e)
        {
            ShowWindow();
        }

        private void UpdateManagerStackFocus(object focusedElement, bool forced = false)
        {
            if(focusedElement != null && focusedElement is FrameworkElement && (!focusedElement.Equals(_selectedManager) || forced))
            {
                _selectedManager = focusedElement as FrameworkElement;
                if(gridManagers.ActualHeight != 0)
                    stackPanelManagers.Height = gridManagers.ActualHeight;
                double totalHeight = stackPanelManagers.Height;

                foreach (FrameworkElement child in stackPanelManagers.Children)
                {
                    if(child.Equals(focusedElement))
                        child.Height = totalHeight - (28.0 * (stackPanelManagers.Children.Count - 1));
                    else
                        child.Height = 25.0;
                }
                _selectedManager.RaiseEvent(new RoutedEventArgs(GotFocusEvent));
            }
        }

        private void ctrlLayerManager_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!sender.Equals(_selectedManager))
                SelectedControl = this.FocusedApplication.Profile.Layers.Count > 0 ? layerPresenter : this.FocusedApplication.Control;
            UpdateManagerStackFocus(sender);
        }

        private void ctrlOverlayLayerManager_PreviewMouseDown(object sender, MouseButtonEventArgs e) {
            if (!sender.Equals(_selectedManager))
                SelectedControl = this.FocusedApplication.Profile.OverlayLayers.Count > 0 ? layerPresenter : this.FocusedApplication.Control;
            UpdateManagerStackFocus(sender);
        }

        private void ctrlProfileManager_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!sender.Equals(_selectedManager))
                SelectedControl = profilePresenter;
            UpdateManagerStackFocus(sender);
        }

        private void brdOverview_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            this._selectedManager = SelectedControl = this.FocusedApplication.Control;

        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e) {
            UpdateManagerStackFocus(_selectedManager, true);
        }



        // This new code for the layer selection has been separated from the existing code so that one day we can sort all
        // the above out and make it more WPF with bindings and other dark magic like that.
        #region PropertyChangedEvent and Helpers
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Call the PropertyChangedEvent for a single property.
        /// </summary>
        private void NotifyChanged(string prop) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));

        /// <summary>
        /// Sets a field and calls <see cref="NotifyChanged(string)"/> with the calling member name and any additional properties.
        /// Designed for setting a field from a property.
        /// </summary>
        private void SetField<T>(ref T var, T value, string[] additional = null, [CallerMemberName] string name = null) {
            var = value;
            NotifyChanged(name);
            if (additional != null)
                foreach (var prop in additional)
                    NotifyChanged(prop);
        }
        #endregion

        #region Properties
        /// <summary>Returns a list of all Applications in the order defined in the profile order configuration.</summary>
        public IEnumerable<Profiles.Application> AllApplications { get; } =
            Global.Configuration.ProfileOrder.Select(profName => (Profiles.Application)Global.LightingStateManager.Events[profName]);

        /// <summary>Returns a list of all Applications that should be visible to the user (depending on the `<see cref="ShowHiddenApplications"/>` property).</summary>
        public IEnumerable<Profiles.Application> VisibleApplications =>
            AllApplications.Where(app => ShowHiddenApplications || !app.Settings.Hidden);

        /// <summary>A reference to the currently selected layer in either the regular or overlay layer list. When set, will update the <see cref="SelectedControl"/> property.</summary>
        public Layer SelectedLayer {
            get => selectedLayer;
            set {
                SetField(ref selectedLayer, value);
                if (value == null)
                    SelectedControl = FocusedApplication?.Control;
                else {
                    layerPresenter.Layer = value;
                    SelectedControl = layerPresenter;
                }
            }
        }
        private Layer selectedLayer;

       /// <summary>The control that is currently displayed underneath the device preview panel. This could be an overview control or a layer presenter etc.</summary>
        public Control SelectedControl { get => selectedControl; set => SetField(ref selectedControl, value); }
        private Control selectedControl;

        private bool showHiddenApplications;
        public bool ShowHiddenApplications { get => showHiddenApplications; set => SetField(ref showHiddenApplications, value, new[] { "VisibleApplications" }); }

        #region FocusedApplication Property
        public Profiles.Application FocusedApplication {
            get => (Profiles.Application)GetValue(FocusedApplicationProperty);
            set {
                SetValue(FocusedApplicationProperty, value);
                Global.LightingStateManager.PreviewProfileKey = value != null ? value.Config.ID : string.Empty;
            }
        }

        public static readonly DependencyProperty FocusedApplicationProperty
            = DependencyProperty.Register("FocusedApplication", typeof(Profiles.Application), typeof(ConfigUI), new PropertyMetadata((Profiles.Application)Global.LightingStateManager.Events["desktop"], new PropertyChangedCallback(FocusedProfileChanged)));
        #endregion
        #endregion

        #region Methods
        private void ToggleWindowState() {
            WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        }

        #region Event Handlers
        private void ApplicationContextHidden_Checked(object sender, RoutedEventArgs e) => NotifyChanged("VisibleApplications");

        private void ApplicationContext_Opened(object sender, RoutedEventArgs e) {
            var cm = (ContextMenu)e.OriginalSource;
            cm.DataContext = (cm.PlacementTarget as ListBox)?.SelectedItem;
        }

        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e) {
            if (e.ClickCount == 2) ToggleWindowState();
            else DragMove();
        }

        private void MinimiseButton_Click(object sender, RoutedEventArgs e) {
            WindowState = WindowState.Minimized;
        }

        private void MaximiseButton_Click(object sender, RoutedEventArgs e) {
            ToggleWindowState();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e) {
            
        }

        private void AddApplicationButton_Click(object sender, RoutedEventArgs e) {
            Window_ProcessSelection dialog = new Window_ProcessSelection { CheckCustomPathExists = true, ButtonLabel = "Add Application", Title = "Add Application" };
            if (dialog.ShowDialog() == true && !string.IsNullOrWhiteSpace(dialog.ChosenExecutablePath)) {

                string filename = Path.GetFileName(dialog.ChosenExecutablePath.ToLowerInvariant());

                if (Global.LightingStateManager.Events.ContainsKey(filename)) {
                    if (Global.LightingStateManager.Events[filename] is GameEvent_Aurora_Wrapper)
                        Global.LightingStateManager.Events.Remove(filename);
                    else {
                        MessageBox.Show("Cannot add this application. It already exists in the application list.", "Cannot register", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                }

                GenericApplication newApplication = new GenericApplication(filename);
                newApplication.Initialize();
                ((GenericApplicationSettings)newApplication.Settings).ApplicationName = Path.GetFileNameWithoutExtension(filename);

                System.Drawing.Icon ico = System.Drawing.Icon.ExtractAssociatedIcon(dialog.ChosenExecutablePath.ToLowerInvariant());

                if (!Directory.Exists(newApplication.GetProfileFolderPath()))
                    Directory.CreateDirectory(newApplication.GetProfileFolderPath());

                using (var icoBmp = ico.ToBitmap())
                    icoBmp.Save(Path.Combine(newApplication.GetProfileFolderPath(), "icon.png"), System.Drawing.Imaging.ImageFormat.Png);
                ico.Dispose();

                Global.LightingStateManager.RegisterEvent(newApplication);
                ConfigManager.Save(Global.Configuration);
            }
        }

        #endregion
        #endregion
    }

    public class ProfileNameResolver : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => value is GenericApplication generic
            ? ((GenericApplicationSettings)generic.Settings).ApplicationName
            : ((Profiles.Application)value).Config.Name;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
