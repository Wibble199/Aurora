using Hardcodet.Wpf.TaskbarNotification;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using Aurora.Settings;
using Aurora.Controls;
using Aurora.Profiles.Generic_Application;
using System.IO;
using Aurora.Settings.Layers;
using Aurora.Profiles.Aurora_Wrapper;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Data;
using System.Globalization;
using System.Windows.Threading;

namespace Aurora {
    partial class ConfigUI : Window, INotifyPropertyChanged
    {
        Control_Settings settingsControl = new Control_Settings();
        Control_LayerControlPresenter layerPresenter = new Control_LayerControlPresenter();
        Control_ProfileControlPresenter profilePresenter = new Control_ProfileControlPresenter();
        
        private FrameworkElement _selectedManager = null;

        private bool settingsloaded = false;
        private bool shownHiddenMessage = false;

        private Timer virtual_keyboard_timer;
        private Grid virtial_kb = new Grid();

        private readonly double virtual_keyboard_width;
        private readonly double virtual_keyboard_height;

        private readonly double width;
        private readonly double height;


        public ConfigUI()
        {
            InitializeComponent();

            virtual_keyboard_height = keyboard_grid.Height;
            virtual_keyboard_width = keyboard_grid.Width;

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
                Visibility = Visibility.Hidden;
                WindowStyle = WindowStyle.None;
                ShowInTaskbar = false;
                Hide();
            }
            else
            {
                Show();
            }
        }

        private void CtrlProfileManager_ProfileSelected(ApplicationProfile profile)
        {
            profilePresenter.Profile = profile;

            if (_selectedManager.Equals(ctrlProfileManager))
                SelectedControl = profilePresenter;   
        }

        private void KbLayout_KeyboardLayoutUpdated(object sender)
        {
            virtial_kb = Global.kbLayout.Virtual_keyboard;

            keyboard_grid.Children.Clear();
            keyboard_grid.Children.Add(virtial_kb);
            keyboard_grid.Children.Add(new LayerEditor());

            keyboard_grid.Width = virtial_kb.Width;
            Width = width + (virtial_kb.Width - virtual_keyboard_width);

            keyboard_grid.Height = virtial_kb.Height;
            Height = height + (virtial_kb.Height - virtual_keyboard_height);

            keyboard_grid.UpdateLayout();

            keyboard_viewbox.MaxWidth = virtial_kb.Width + 50;
            keyboard_viewbox.MaxHeight = virtial_kb.Height + 50;
            keyboard_viewbox.UpdateLayout();

            UpdateLayout();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (!settingsloaded)
            {
                virtual_keyboard_timer = new Timer(50);
                virtual_keyboard_timer.Elapsed += new ElapsedEventHandler(virtual_keyboard_timer_Tick);
                virtual_keyboard_timer.Start();

                settingsloaded = true;
            }

            keyboard_record_message.Visibility = Visibility.Hidden;
            
            virtial_kb = Global.kbLayout.Virtual_keyboard;

            keyboard_grid.Children.Clear();
            keyboard_grid.Children.Add(virtial_kb);
            keyboard_grid.Children.Add(new LayerEditor());

            keyboard_grid.Width = virtial_kb.Width;
            Width = width + (virtial_kb.Width - virtual_keyboard_width);

            keyboard_grid.Height = virtial_kb.Height;
            Height = height + (virtial_kb.Height - virtual_keyboard_height);

            keyboard_grid.UpdateLayout();

            keyboard_viewbox.MaxWidth = virtial_kb.Width + 50;
            keyboard_viewbox.MaxHeight = virtial_kb.Height + 50;
            keyboard_viewbox.UpdateLayout();

            UpdateManagerStackFocus(ctrlLayerManager);

            UpdateLayout();
        }

        private void virtual_keyboard_timer_Tick(object sender, EventArgs e) {
            Dispatcher.Invoke(() => {
                if (!IsActive) return;

                var keylights = new Dictionary<Devices.DeviceKeys, System.Drawing.Color>();

                keylights = Global.effengine.GetKeyboardLights();
                Global.kbLayout.SetKeyboardColors(keylights);

                keyboard_record_message.Visibility = Global.key_recorder.IsRecording() ? Visibility.Visible : Visibility.Hidden;
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
            FocusedApplication?.SaveAll();

            if (!shownHiddenMessage)
            {
                trayicon.ShowBalloonTip("Aurora", "This program is now hidden in the tray.", BalloonIcon.Info);
                shownHiddenMessage = true;
            }

            //Hide Window
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, (Action)(() => {
                WindowStyle = WindowStyle.None;
                Hide();
            }));
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            Global.LightingStateManager.PreviewProfileKey = FocusedApplication?.Config.ID ?? string.Empty;
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            Global.LightingStateManager.PreviewProfileKey = string.Empty;
        }

        private static void FocusedProfileChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            ConfigUI self = source as ConfigUI;
            Profiles.Application value = e.NewValue as Profiles.Application;
            
            self.gridManagers.Visibility = value == null ? Visibility.Collapsed : Visibility.Visible;

            if (value != null)            
                self.SelectedControl = value.Control;

            Global.LightingStateManager.PreviewProfileKey = value?.Config.ID ?? string.Empty;
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
                SelectedControl = FocusedApplication.Profile.Layers.Count > 0 ? layerPresenter : FocusedApplication.Control;
            UpdateManagerStackFocus(sender);
        }

        private void ctrlOverlayLayerManager_PreviewMouseDown(object sender, MouseButtonEventArgs e) {
            if (!sender.Equals(_selectedManager))
                SelectedControl = FocusedApplication.Profile.OverlayLayers.Count > 0 ? layerPresenter : FocusedApplication.Control;
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
            _selectedManager = SelectedControl = FocusedApplication.Control;
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
            if (e.LeftButton == MouseButtonState.Pressed) {
                if (e.ClickCount == 2) ToggleWindowState();
                else DragMove();
            }
        }

        private void MinimiseButton_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;

        private void MaximiseButton_Click(object sender, RoutedEventArgs e) => ToggleWindowState();

        private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();

        private void AddApplicationButton_Click(object sender, RoutedEventArgs e) {
            var dialog = new Window_ProcessSelection { CheckCustomPathExists = true, ButtonLabel = "Add Application", Title = "Add Application" };
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

                var newApplication = new GenericApplication(filename);
                newApplication.Initialize();
                ((GenericApplicationSettings)newApplication.Settings).ApplicationName = Path.GetFileNameWithoutExtension(filename);

                var ico = System.Drawing.Icon.ExtractAssociatedIcon(dialog.ChosenExecutablePath.ToLowerInvariant());

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

        private void SettingsButton_Click(object sender, RoutedEventArgs e) => SelectedControl = settingsControl;
    }


    /// <summary>
    /// Returns the name from an application, even if the applicationis a generic one.
    /// </summary>
    public class ProfileNameResolver : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => value is GenericApplication generic
            ? ((GenericApplicationSettings)generic.Settings).ApplicationName
            : (value as Profiles.Application)?.Config.Name;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
