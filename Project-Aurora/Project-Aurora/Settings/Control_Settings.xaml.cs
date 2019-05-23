using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Xceed.Wpf.Toolkit;
using Microsoft.Win32;
using System.Diagnostics;
using Microsoft.Win32.TaskScheduler;
using System.Windows.Data;

namespace Aurora.Settings {
    /// <summary>
    /// Interaction logic for Control_Settings.xaml
    /// </summary>
    public partial class Control_Settings : UserControl {
        private RegistryKey runRegistryPath = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
        private const string StartupTaskID = "AuroraStartup";

        private Window winBitmapView = null;
        private Image imgBitmap = new Image();
        private static bool bitmapViewOpen;

        public Control_Settings() {
            InitializeComponent();
            settingsScroller.Background = Utils.BrushUtils.BlendBrushes((Brush)FindResource("Panel1BackgroundBrush"), (Brush)FindResource("BaseBackgroundBrush"), .7);

            if (runRegistryPath.GetValue("Aurora") != null)
                runRegistryPath.DeleteValue("Aurora");

            try {
                using (TaskService service = new TaskService()) {
                    Microsoft.Win32.TaskScheduler.Task task = service.FindTask(StartupTaskID);
                    if (task != null) {
                        TaskDefinition definition = task.Definition;
                        //Update path of startup task
                        string exePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                        definition.Actions.Clear();
                        definition.Actions.Add(new ExecAction(exePath, "-silent", Path.GetDirectoryName(exePath)));
                        service.RootFolder.RegisterTaskDefinition(StartupTaskID, definition);
                        RunAtWinStartup.IsChecked = task.Enabled;
                        startDelayAmount.Value = task.Definition.Triggers.FirstOrDefault(t => t.TriggerType == TaskTriggerType.Logon) is LogonTrigger trigger ? (int)trigger.Delay.TotalSeconds : 0;
                    } else {
                        TaskDefinition td = service.NewTask();
                        td.RegistrationInfo.Description = "Start Aurora on Startup";

                        td.Triggers.Add(new LogonTrigger { Enabled = true });

                        string exePath = System.Reflection.Assembly.GetExecutingAssembly().Location;

                        td.Actions.Add(new ExecAction(exePath, "-silent", Path.GetDirectoryName(exePath)));

                        td.Principal.RunLevel = TaskRunLevel.Highest;
                        td.Settings.DisallowStartIfOnBatteries = false;
                        td.Settings.DisallowStartOnRemoteAppSession = false;
                        td.Settings.ExecutionTimeLimit = TimeSpan.Zero;

                        service.RootFolder.RegisterTaskDefinition(StartupTaskID, td);
                        RunAtWinStartup.IsChecked = true;
                    }
                }
            } catch (Exception exc) {
                Global.logger.Error("Error caught when updating startup task. Error: " + exc.ToString());
            }

            string v = FileVersionInfo.GetVersionInfo(System.Reflection.Assembly.GetExecutingAssembly().Location).FileVersion;

            this.lblVersion.Content = $"v{v}" + ((int.Parse(v[0].ToString()) > 0) ? "" : " (beta)");

            LangCb.ItemsSource = Localization.CultureUtils.AvailableCultures // Fill the language selection combobox with all detected available languages
                .OrderBy(culture => culture.NativeName) // Sorted by name
                .Select(culture => new { culture.NativeName, culture.IetfLanguageTag, Icon = Localization.CultureUtils.GetIcon(culture.IetfLanguageTag) });


        }

        private void OnLayerRendered(System.Drawing.Bitmap map) {
            try {
                Dispatcher.Invoke(() => {
                    using (MemoryStream memory = new MemoryStream()) {
                        //Fix conflict with AtomOrb due to async
                        lock (map) {
                            map.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
                        }
                        memory.Position = 0;
                        BitmapImage bitmapimage = new BitmapImage();
                        bitmapimage.BeginInit();
                        bitmapimage.StreamSource = memory;
                        bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                        bitmapimage.EndInit();

                        this.debug_bitmap_preview.Width = 4 * bitmapimage.Width;
                        this.debug_bitmap_preview.Height = 4 * bitmapimage.Height;
                        this.debug_bitmap_preview.Source = bitmapimage;
                    }
                });
            } catch (Exception ex) {
                Global.logger.Warn(ex.ToString());
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e) {
            Global.effengine.NewLayerRender += OnLayerRendered;
            this.ctrlPluginManager.Host = Global.PluginManager;
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e) {
            Global.effengine.NewLayerRender -= OnLayerRendered;
        }

        //// Misc

        private void RecordKeySequence(string whoisrecording, Button button, ListBox sequence_listbox) {
            if (Global.key_recorder.IsRecording()) {
                if (Global.key_recorder.GetRecordingType().Equals(whoisrecording)) {
                    Global.key_recorder.StopRecording();

                    button.Content = "Assign Keys";

                    Devices.DeviceKeys[] recorded_keys = Global.key_recorder.GetKeys();

                    if (sequence_listbox.SelectedIndex > 0 && sequence_listbox.SelectedIndex < (sequence_listbox.Items.Count - 1)) {
                        int insertpos = sequence_listbox.SelectedIndex;
                        foreach (var key in recorded_keys) {
                            sequence_listbox.Items.Insert(insertpos, key);
                            insertpos++;
                        }
                    } else {
                        foreach (var key in recorded_keys)
                            sequence_listbox.Items.Add(key);
                    }

                    Global.key_recorder.Reset();
                } else {
                    System.Windows.MessageBox.Show("You are already recording a key sequence for " + Global.key_recorder.GetRecordingType());
                }
            } else {
                Global.key_recorder.StartRecording(whoisrecording);
                button.Content = "Stop Assigning";
            }
        }

        private void ExcludedAdd_Click(object sender, RoutedEventArgs e) {
            var dialog = new Window_ProcessSelection { ButtonLabel = "Exclude Process" };
            if (dialog.ShowDialog() == true && !string.IsNullOrWhiteSpace(dialog.ChosenExecutableName)) // do not need to check if dialog is already in excluded_programs since it is a Set and only contains unique items by definition
                Global.Configuration.ExcludedPrograms.Add(dialog.ChosenExecutableName);
        }

        private void ExcludedRemove_Click(object sender, RoutedEventArgs e) {
            if (excludedListbox.SelectedItem != null)
                Global.Configuration.ExcludedPrograms.Remove(excludedListbox.SelectedItem.ToString());
        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            Global.effengine.ToggleRecord();

            if (Global.effengine.isrecording)
                (sender as Button).Content = "Stop Recording";
            else
                (sender as Button).Content = "Record";
        }

        private void RunAtWinStartup_Checked(object sender, RoutedEventArgs e) {
            try {
                using (TaskService ts = new TaskService()) {
                    //Find existing task
                    var task = ts.FindTask(StartupTaskID);
                    task.Enabled = (sender as CheckBox).IsChecked.Value;
                }
            } catch (Exception exc) {
                Global.logger.Error("Exception trying to toggle run at startup: " + exc);
            }
        }

        private void devices_retry_Click(object sender, RoutedEventArgs e) {
            Global.dev_manager.Initialize();
        }

        private void devices_view_first_time_logitech_Click(object sender, RoutedEventArgs e) {
            new Devices.Logitech.LogitechInstallInstructions().ShowDialog();
        }

        private void devices_view_first_time_corsair_Click(object sender, RoutedEventArgs e) {
            new Devices.Corsair.CorsairInstallInstructions().ShowDialog();
        }

        private void devices_view_first_time_razer_Click(object sender, RoutedEventArgs e) {
            new Devices.Razer.RazerInstallInstructions().ShowDialog();
        }
        private void devices_view_first_time_steelseries_Click(object sender, RoutedEventArgs e) {
            new Devices.SteelSeries.SteelSeriesInstallInstructions().ShowDialog();
        }

        private void devices_view_first_time_dualshock_Click(object sender, RoutedEventArgs e) {
            new Devices.Dualshock.DualshockInstallInstructions().ShowDialog();
        }

        private void devices_view_first_time_roccat_Click(object sender, RoutedEventArgs e) {
            new Devices.Roccat.RoccatInstallInstructions().ShowDialog();
        }

        private void updates_check_Click(object sender, RoutedEventArgs e) {
            if (IsLoaded) {
                string updater_path = Path.Combine(Global.ExecutingDirectory, "Aurora-Updater.exe");

                if (File.Exists(updater_path)) {
                    ProcessStartInfo startInfo = new ProcessStartInfo();
                    startInfo.FileName = updater_path;
                    Process.Start(startInfo);
                } else {
                    System.Windows.MessageBox.Show("Updater is missing!");
                }
            }
        }

        private void wrapper_install_logitech_Click(object sender, RoutedEventArgs e) {
            try {
                App.InstallLogitech();
            } catch (Exception exc) {
                Global.logger.Error("Exception during Logitech Wrapper install. Exception: " + exc);
                System.Windows.MessageBox.Show("Aurora Wrapper Patch for Logitech could not be applied.\r\nException: " + exc.Message);
            }
        }

        private void wrapper_install_razer_Click(object sender, RoutedEventArgs e) {
            try {
                var dialog = new System.Windows.Forms.FolderBrowserDialog();
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();

                if (result == System.Windows.Forms.DialogResult.OK) {
                    using (BinaryWriter razer_wrapper_86 = new BinaryWriter(new FileStream(Path.Combine(dialog.SelectedPath, "RzChromaSDK.dll"), FileMode.Create))) {
                        razer_wrapper_86.Write(Properties.Resources.Aurora_RazerLEDWrapper86);
                    }

                    using (BinaryWriter razer_wrapper_64 = new BinaryWriter(new FileStream(Path.Combine(dialog.SelectedPath, "RzChromaSDK64.dll"), FileMode.Create))) {
                        razer_wrapper_64.Write(Properties.Resources.Aurora_RazerLEDWrapper64);
                    }

                    System.Windows.MessageBox.Show("Aurora Wrapper Patch for Razer applied to\r\n" + dialog.SelectedPath);
                }
            } catch (Exception exc) {
                Global.logger.Error("Exception during Razer Wrapper install. Exception: " + exc);
                System.Windows.MessageBox.Show("Aurora Wrapper Patch for Razer could not be applied.\r\nException: " + exc.Message);
            }
        }

        private void wrapper_install_lightfx_32_Click(object sender, RoutedEventArgs e) {
            try {
                var dialog = new System.Windows.Forms.FolderBrowserDialog();
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();

                if (result == System.Windows.Forms.DialogResult.OK) {
                    using (BinaryWriter lightfx_wrapper_86 = new BinaryWriter(new FileStream(Path.Combine(dialog.SelectedPath, "LightFX.dll"), FileMode.Create))) {
                        lightfx_wrapper_86.Write(Properties.Resources.Aurora_LightFXWrapper86);
                    }

                    System.Windows.MessageBox.Show("Aurora Wrapper Patch for LightFX (32 bit) applied to\r\n" + dialog.SelectedPath);
                }
            } catch (Exception exc) {
                Global.logger.Error("Exception during LightFX (32 bit) Wrapper install. Exception: " + exc);
                System.Windows.MessageBox.Show("Aurora Wrapper Patch for LightFX (32 bit) could not be applied.\r\nException: " + exc.Message);
            }
        }

        private void wrapper_install_lightfx_64_Click(object sender, RoutedEventArgs e) {
            try {
                var dialog = new System.Windows.Forms.FolderBrowserDialog();
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();

                if (result == System.Windows.Forms.DialogResult.OK) {
                    using (BinaryWriter lightfx_wrapper_64 = new BinaryWriter(new FileStream(Path.Combine(dialog.SelectedPath, "LightFX.dll"), FileMode.Create))) {
                        lightfx_wrapper_64.Write(Properties.Resources.Aurora_LightFXWrapper64);
                    }

                    System.Windows.MessageBox.Show("Aurora Wrapper Patch for LightFX (64 bit) applied to\r\n" + dialog.SelectedPath);
                }
            } catch (Exception exc) {
                Global.logger.Error("Exception during LightFX (64 bit) Wrapper install. Exception: " + exc);
                System.Windows.MessageBox.Show("Aurora Wrapper Patch for LightFX (64 bit) could not be applied.\r\nException: " + exc.Message);
            }
        }

        private void btnShowBitmapWindow_Click(object sender, RoutedEventArgs e) {
            if (winBitmapView == null) {
                if (bitmapViewOpen == true) {
                    System.Windows.MessageBox.Show("Keyboard Bitmap View already open.\r\nPlease close it.");
                    return;
                }

                winBitmapView = new Window();
                winBitmapView.Closed += WinBitmapView_Closed;
                winBitmapView.ResizeMode = ResizeMode.CanResize;

                winBitmapView.SetBinding(Window.TopmostProperty, new Binding("BitmapDebugTopMost") { Source = Global.Configuration });

                //winBitmapView.SizeToContent = SizeToContent.WidthAndHeight;

                winBitmapView.Title = "Keyboard Bitmap View";
                winBitmapView.Background = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0));
                Global.effengine.NewLayerRender += Effengine_NewLayerRender;

                imgBitmap.SnapsToDevicePixels = true;
                imgBitmap.HorizontalAlignment = HorizontalAlignment.Stretch;
                imgBitmap.VerticalAlignment = VerticalAlignment.Stretch;
                /*imgBitmap.MinWidth = 0;
                imgBitmap.MinHeight = 0;*/
                imgBitmap.MinWidth = Effects.canvas_width;
                imgBitmap.MinHeight = Effects.canvas_height;

                winBitmapView.Content = imgBitmap;

                winBitmapView.UpdateLayout();
                winBitmapView.Show();
            } else {
                winBitmapView.BringIntoView();
            }
        }

        private void Effengine_NewLayerRender(System.Drawing.Bitmap bitmap) {
            try {
                Dispatcher.Invoke(() => {
                    lock (bitmap) {
                        using (MemoryStream memory = new MemoryStream()) {
                            bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
                            memory.Position = 0;
                            BitmapImage bitmapimage = new BitmapImage();
                            bitmapimage.BeginInit();
                            bitmapimage.StreamSource = memory;
                            bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                            bitmapimage.EndInit();

                            imgBitmap.Source = bitmapimage;
                        }
                    }
                });
            } catch (Exception ex) {
                Global.logger.Warn(ex.ToString());
            }
        }

        private void WinBitmapView_Closed(object sender, EventArgs e) {
            winBitmapView = null;
            Global.effengine.NewLayerRender -= Effengine_NewLayerRender;
            bitmapViewOpen = false;
        }

        private void btnShowLogsFolder_Click(object sender, RoutedEventArgs e) {
            if (sender is Button)
                Process.Start(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Aurora/Logs/"));
        }

        private void HigherPriority_IsCheckedChanged(object sender, RoutedEventArgs e) {
            Process.GetCurrentProcess().PriorityClass = Global.Configuration.HighPriority ? ProcessPriorityClass.High : ProcessPriorityClass.Normal;
        }

        private void btnShowGSILog_Click(object sender, RoutedEventArgs e) => new Window_GSIHttpDebug().Show();

        private void startDelayAmount_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e) {
            using (TaskService service = new TaskService()) {
                var task = service.FindTask(StartupTaskID);
                if (task != null && task.Definition.Triggers.FirstOrDefault(t => t.TriggerType == TaskTriggerType.Logon) is LogonTrigger trigger) {
                    trigger.Delay = new TimeSpan(0, 0, ((IntegerUpDown)sender).Value ?? 0);
                    task.RegisterChanges();
                }
            }
        }

        private void OpenLink(string link) => Process.Start(new ProcessStartInfo(link));
        private void GithubButton_Click(object sender, RoutedEventArgs e) => OpenLink("https://github.com/antonpup/Aurora/");
        private void DiscordButton_Click(object sender, RoutedEventArgs e) => OpenLink("https://discord.gg/YAuBmg9");
        private void SteamButton_Click(object sender, RoutedEventArgs e) => OpenLink("http://steamcommunity.com/id/SimonWhyte/");
    }
}
