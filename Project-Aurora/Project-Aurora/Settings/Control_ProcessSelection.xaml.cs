using Aurora.Controls;
using Aurora.Settings.Localization;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using static Aurora.Controls.AlertBox;
using Icon = System.Drawing.Icon;

namespace Aurora.Settings {
    public partial class Control_ProcessSelection : UserControl {

        public Control_ProcessSelection() {
            InitializeComponent();

            // Scan running processes (in another Task, so it doesn't cause the delay to the user) and add them to a list
            Task.Run(() => {
                var processList = new List<RunningProcess>();
                foreach (var p in Process.GetProcesses())
                    try {
                        // Get the exe name
                        string name = System.IO.Path.GetFileName(p.MainModule.FileName);
                        // Check if we've already got an exe by that name, if not add it
                        if (!processList.Any(x => x.Name == name))
                            processList.Add(new RunningProcess {
                                Name = name,
                                Path = p.MainModule.FileName,
                                Icon = Icon.ExtractAssociatedIcon(p.MainModule.FileName)
                            });
                    } catch { }

                Dispatcher.Invoke(() => {
                    // Sort the list, set the ListBox control to use that list
                    RunningProcessList.ItemsSource = processList.OrderBy(p => p.Name);
                    RunningProcessList.SelectedIndex = 0;

                    // CollectionViewSorce to provide search/filter feature
                    CollectionViewSource.GetDefaultView(RunningProcessList.ItemsSource).Filter = RunningProcessFilterPredicate;
                    RunningProcessListFilterText.Focus();

                    // Hide the loading overlay
                    LoadingOverlay.Visibility = Visibility.Collapsed;
                });
            });
        }

        /// <summary>
        /// Handler for the browse button on the custom exe path tab. Sets
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BrowseButton_Click(object sender, RoutedEventArgs e) {
            OpenFileDialog dialog = new OpenFileDialog() {
                AddExtension = true,
                Filter = "Executable files (*.exe)|*.exe|All files (*.*)|*.*",
                Multiselect = false
            };
            if (dialog.ShowDialog() == true) // requires "== true" because ShowDialog is a bool?, so doing "if (dialog.ShowDialog())" is invalid
                ProcessBrowseResult.Text = dialog.FileName;
        }

        /// <summary>
        /// Updates the running process filter when the textbox is changed.
        /// </summary>
        private void RunningListFilter_TextChanged(object sender, TextChangedEventArgs e) {
            CollectionViewSource.GetDefaultView(RunningProcessList.ItemsSource).Refresh();
            if (RunningProcessList.SelectedIndex == -1)
                RunningProcessList.SelectedIndex = 0;
        }

        /// <summary>
        /// Method that makes Up/Down arrow keys when focussed on the RunningListFilter change the selection of the running list element.
        /// This means you don't have to click on the item when you are typing in a filter.
        /// We do not need to handle Enter key here as it is done by setting the OK button "IsDefault" to true.
        /// </summary>
        private void RunningProcessListFilterText_KeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Up)
                RunningProcessList.SelectedIndex = Math.Max(RunningProcessList.SelectedIndex - 1, 0);
            else if (e.Key == Key.Down)
                RunningProcessList.SelectedIndex = RunningProcessList.SelectedIndex + 1; // Automatically clamped
        }

        /// <summary>
        /// Filter that is run on each item in the running process list (List&lt;RunningProcess&gt;) and returns a bool
        /// indicating whether it should appear on the list.
        /// </summary>
        private bool RunningProcessFilterPredicate(object item) {
            return ((RunningProcess)item).Name.IndexOf(RunningProcessListFilterText.Text, StringComparison.InvariantCultureIgnoreCase) >= 0;
        }
        


        /// <summary>
        /// Shows an <see cref="AlertBox"/> with the process picker as the content. Returns a Task that will complete when the dialog is
        /// closed. If nothing was selected (i.e. the operation was cancelled), the result of the Task will be null. If the user selected a
        /// process, that process's details will be returned.
        /// </summary>
        public static async Task<(string name, string path)?> ShowDialog(DependencyObject container, string title) {
            var processPicker = new Control_ProcessSelection();

            // Function that validates the click event for the picker
            bool validatePP(int _) =>
                (processPicker.MainTabControl.SelectedIndex == 0 && processPicker.RunningProcessList.SelectedItem != null) // If the user is on the active process list, ensure that they have actually selected a process
                || (processPicker.MainTabControl.SelectedIndex == 1 && !string.IsNullOrWhiteSpace(processPicker.ProcessBrowseResult.Text) && File.Exists(processPicker.ProcessBrowseResult.Text)); // Else if the user is on the exe browse page, ensure the file exists

            // Show the dialog
            var result = await Show(container, processPicker, title, new[] { new ChoiceButton(TranslationSource.Instance["cancel"], "FlatButton"), new ChoiceButton(TranslationSource.Instance["select_process"], validate: validatePP) });

            // Handle the output (thanks to the button's Validate method, we don't need to check if this is valid since for this to be triggered it must have passed)
            if (result != 1) return null;
            if (processPicker.MainTabControl.SelectedIndex == 0) {
                var exe = (RunningProcess)processPicker.RunningProcessList.SelectedItem;
                return (exe.Name, exe.Path);
            } else {
                var exe = processPicker.ProcessBrowseResult.Text;
                return (Path.GetFileNameWithoutExtension(exe), exe);
            }
        }
    }


    /// <summary>
    /// Converts an Icon into a WPF-compatible BitmapSource.
    /// </summary>
    class IconToImageConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            Icon ico = (Icon)value;
            // Taken from https://stackoverflow.com/a/51438725/1305670
            return System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(ico.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => null;
    }

    /// <summary>
    /// Container for a Running Process definition.
    /// </summary>
    struct RunningProcess {
        public string Name { get; set; }
        public string Path { get; set; }
        public Icon Icon { get; set; }
    }
}
