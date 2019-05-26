using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Markup;
using System.Xml;

namespace Aurora.Settings {

    /// <summary>
    /// Singleton class that handles dealing with the theme currently in use by Aurora.
    /// </summary>
    public class ThemeManager : INotifyPropertyChanged {

        /// <summary>Directory where the themes will be stored/read from.</summary>
        private static string ThemePath { get; } = Path.Combine(Global.ExecutingDirectory, "Themes");

        /// <summary>Gets the current instance of the ThemeManager.</summary>
        public static ThemeManager Instance { get; } = new ThemeManager();

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>A list of all possible themes the user can choose from. Includes any files in the "Themes" directory as well as a "Default" theme.
        /// <para>Returns a KeyValuePair enumberable where the key is the user-friendly name and the value is the raw file name.</para></summary>
        public IEnumerable<KeyValuePair<string, string>> ThemeList { get; private set; }

        private ThemeManager() {
            // Do an initial theme scan
            UpdateThemeList();

            // Do an initial theme application
            ApplyTheme();

            // Setup a watcher that will update the theme when it is changed and refresh the list when files
            // are added or removed to the themes directory.
            var watcher = new FileSystemWatcher {
                Path = ThemePath,
                Filter = "*.xaml",
                EnableRaisingEvents = true
            };
            watcher.Created += (sender, e) => UpdateThemeList();
            watcher.Deleted += (sender, e) => UpdateThemeList();
        }

        /// <summary>Refreshes the <see cref="ThemeList"/> property with all available themes in the theme directory.</summary>
        private void UpdateThemeList() {
            ThemeList = new[] { new KeyValuePair<string, string>("Default", "") }
                .Concat(
                    Directory.EnumerateFiles(ThemePath, "*.xaml")
                    .Select(s => Path.GetFileNameWithoutExtension(s))
                    .Select(s => new KeyValuePair<string, string>(s.Replace('_', ' '), s))
                );
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ThemeList"));
        }

        /// <summary>Unloads any existing theme and loads the theme specified in the global configuration.</summary>
        public static void ApplyTheme() {
            string themeName = Global.Configuration.ThemeName;

            var res = Application.Current.Resources.MergedDictionaries;
            res.Clear(); // Remove the existing theme
            res.Add(new ResourceDictionary { Source = new Uri("Theme/BaseThemeVariables.xaml", UriKind.Relative) }); // Add the fallback (default) values
            res.Add(new ResourceDictionary { Source = new Uri("Theme/AuroraAppTheme.xaml", UriKind.Relative) }); // Add the styles (e.g. buttons etc.)
            if (!string.IsNullOrEmpty(themeName) && File.Exists($"Themes/{themeName}.xaml")) // If the file exists, load the custom theme
                try { res.Add((ResourceDictionary)XamlReader.Load(new XmlTextReader($"Themes/{themeName}.xaml"))); } catch { }
        }
    }
}
