using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Resources;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media.Imaging;

namespace Aurora.Settings.Localization {

    /// <summary>
    /// The class that stores the current culture information and fetches translations for the Localization resource mananager.
    /// </summary>
    /// <remarks>
    /// This code has been slightly adapted and commented from Jakub Fijałkowski's code.
    /// Available here: https://codinginfinity.me/post/2015-05-10/localization_of_a_wpf_app_the_simple_approach
    /// </remarks>
    public class TranslationSource : INotifyPropertyChanged {

        /// <summary>The base directory which contains folders for each language.</summary>
        public static string LanguageFileBaseDir { get; } = Path.Combine(Global.ExecutingDirectory, "Localization");

        /// <summary>Event called when the culture information is changed, so that XAML elements with the Loc binding know they need to update their values.</summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>Get the singleton instance of <see cref="TranslationSource" />.</summary>
        public static TranslationSource Instance { get => instance.Value; }
        private static Lazy<TranslationSource> instance = new Lazy<TranslationSource>(() => new TranslationSource());

        private CultureInfo currentCulture = new CultureInfo(Global.Configuration.LanguageIETF);

        private Dictionary<(string package, string key), string> source = new Dictionary<(string package, string key), string>();
        private Dictionary<(string package, string key), string> sourceFallback = new Dictionary<(string package, string locale), string>();

        /// <summary>Constructor that performs the initial language file loading.</summary>
        private TranslationSource() => ReloadLanguageFiles();

        /// <summary>Gets or sets the current culture which is used to find the correct translation.
        /// Will update all XAML elements that make use of the Loc binding when the culture is set.</summary>
        public CultureInfo CurrentCulture {
            get => currentCulture;
            set {
                if (currentCulture != value) {
                    currentCulture = value;
                    source.Clear();
                    LoadLanguageFiles(source, value.IetfLanguageTag);
                    Global.Configuration.LanguageIETF = value.IetfLanguageTag; // Store the new lang
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("")); // Empty string means all properties have changed (including all values with indexers)
                }
            }
        }

        /// <summary>Gets or sets the IETF Language Tag for the current culture.</summary>
        public string CurrentCultureIetfTag {
            get => CurrentCulture.IetfLanguageTag;
            set => CurrentCulture = new CultureInfo(value);
        }

        /// <summary>Gets the translated string for particular key.</summary>
        public string this[string key] => GetString(key);
        /// <summary>Gets the translated string for particular key in a particular package.</summary>
        public string this[string key, string package] => GetString(key, package);

        /// <summary>Returns the translated string with a particular key, optionally in a particular package group, in the current culture.
        /// <para>If the current culture does not contain the key, the en-US language is used as a fallback. If the key doesn't exist there
        /// either, then a warning is shown in the format "#Missing:&lt;package&gt;.&lt;key&gt;".</para></summary>
        /// <param name="key">The key of the string to get, e.g. "PrimaryColor".</param>
        /// <param name="package">An optional package the translation is in.</param>
        public string GetString(string key, string package = "aurora") {
            var keyTuple = (package.ToLower(), key.ToLower());
            if (source.ContainsKey(keyTuple))
                return source[keyTuple];
            else if (sourceFallback.ContainsKey(keyTuple))
                return source[keyTuple];
            return $"#Missing:{package}.{key}";
        }

        /// <summary>Loads all the language files (of all packages) of the provided culture into the provided dictionary.</summary>
        /// <param name="into">The dictionary that all the language translations will be stored in.</param>
        /// <param name="locale">The IETF language code of the culture whose files to load into the dictionary.</param>
        private static void LoadLanguageFiles(Dictionary<(string package, string key), string> into, string locale) {
            // Check the specified locale is valid.
            if (!CultureUtils.IsCultureValid(locale)) {
                Global.logger.Error(new ArgumentException("Invalid IETF Language Code provided (parameter `locale`). This locale is unavailable."));
                return;
            }

            // Check the specified lang files exist
            var langDir = new DirectoryInfo(Path.Combine(LanguageFileBaseDir, locale));
            if (!langDir.Exists) {
                Global.logger.Error(new ArgumentException("The specified locale does not have a language directory assigned. This locale is unavailable."));
                return;
            }

            into.Clear();

            // Look though all of the relevant files in the directory
            foreach (var langFile in langDir.EnumerateFiles("*.lang")) {
                // Package name is given by filename. E.G. "Wibble.Package.lang" -> Package = "wibble.package"
                var packageName = Path.GetFileNameWithoutExtension(langFile.Name).ToLower();

                // Read the entire contents of the file, with each line in format "<key>: <translation>"
                // Add each translation into the target dictionary under the package name given by the filename and the key specified on the line
                foreach (var line in File.ReadAllLines(langFile.FullName)) {
                    if (string.IsNullOrWhiteSpace(line) || line[0] == '~') continue;
                    try {
                        var idx = line.IndexOf(": ");
                        into.Add((packageName, line.Substring(0, idx).ToLower()), line.Substring(idx + 2, line.Length - idx - 2).Trim());
                    } catch (Exception ex) {
                        Global.logger.Warn($"Exception while parsing line in {langFile.FullName}. Exception: {ex.Message}. Tip: Use '~' to mark lines as comments.");
                    }
                }
            }
        }

        /// <summary>Can be called to reload the language files. Useful for when additional ones are installed, to save Aurora from needing to restart.</summary>
        public void ReloadLanguageFiles() {
            LoadLanguageFiles(sourceFallback, "en-US");
            LoadLanguageFiles(source, CurrentCultureIetfTag);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(""));
        }
    }



    /// <summary>
    /// Binding extension class that provides a method of fetching the translation in the <see cref="TranslationSource.CurrentCulture" />
    /// culture for a particular string key or string binding. This allows use of localization with controls such as ComboBoxes and
    /// ItemsControls. Uses a MultiBinding so it can watch for changes notified from the key or from the <see cref="TranslationSource"/>.
    /// </summary>
    public class LocExtension : MultiBinding {

        // Reference to the converter so parameters can be passed to it.
        private LocalizationConverter conv;

        /// <summary>Creates a new binding that will use the given string as the key value for localization.</summary>
        public LocExtension(string key) => Init(new Binding(".") { Source = key });

        /// <summary>Creates a new binding that will use the given string as the key value for localization, targetting the given package.</summary>
        public LocExtension(string key, string package) => Init(new Binding(".") { Source = key }, package);

        /// <summary>Creates a new binding that will use the given binding as the key value for localization.</summary>
        public LocExtension(BindingBase keyBinding) => Init(keyBinding);

        /// <summary>Creates a new binding that will use the given binding as the key value for localization, targetting the given package.</summary>
        public LocExtension(BindingBase keyBinding, string package) => Init(keyBinding, package);

        private void Init(BindingBase keyBinding, string package = "aurora") {
            Bindings.Add(keyBinding);
            Bindings.Add(new Binding("[LocBinding]") { Source = TranslationSource.Instance }); // Binds to a dummy key so that it's updated if lang is changed
            Converter = conv = new LocalizationConverter { Package = package };
        }

        /// <summary>A substring that is prepended to the start of the localized string.</summary>
        public string Prefix { get => conv.Prefix; set => conv.Prefix = value; }

        /// <summary>A substring that is appended to the end of the localized string.</summary>
        public string Suffix { get => conv.Suffix; set => conv.Suffix = value; }

        /// <summary>One or more substrings that are passed to the converter to insert them into the translated string. Multiple substrings
        /// should be separated using the pipe character ('|').
        /// <para>E.G. if the result from localisation was "Enable {0} profile", and Values was ["Minecraft"], the result
        /// would be "Enable Minecraft profile".</para></summary>
        public string InsertValues { set => conv.InsertValues = value.Split('|'); }

        /// <summary>Basic converter to handle the localization.</summary>
        private class LocalizationConverter : IMultiValueConverter {

            public string Package { get; set; }
            public string Prefix { get; set; }
            public string Suffix { get; set; }
            public string[] InsertValues { get; set; }

            public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
                var key = values[0].ToString();
                var str = TranslationSource.Instance.GetString(key, Package ?? "aurora");
                if (InsertValues != null) str = string.Format(str, InsertValues);
                return (Prefix ?? "") + str + (Suffix ?? "");
            }
            public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotImplementedException();
        }
    }



    /// <summary>
    /// A class providing Localization-related attached properties. These properties ("Key" and "Package") can be applied to the
    /// <see cref="TextBlock"/> control to apply localization to the Text property without needing to bind to the text (so that placeholder
    /// text can be set during design-time to assist with the positioning and designing of the UI). These properties can take bindings.
    /// </summary>
    public static class Localization {

        // The main localization key used to lookup the translated phrase in the dictionary.
        public static string GetKey(DependencyObject obj) => (string)obj.GetValue(KeyProperty);
        public static void SetKey(DependencyObject obj, string value) => obj.SetValue(KeyProperty, value);

        public static readonly DependencyProperty KeyProperty =
            DependencyProperty.RegisterAttached("Key", typeof(string), typeof(Localization), new PropertyMetadata("", LocalizationChanged));
        
        // The package that is used as a source for the translation. Defaults to "aurora".
        public static string GetPackage(DependencyObject obj) =>(string)obj.GetValue(PackageProperty);
        public static void SetPackage(DependencyObject obj, string value) => obj.SetValue(PackageProperty, value);

        public static readonly DependencyProperty PackageProperty =
            DependencyProperty.RegisterAttached("Package", typeof(string), typeof(Localization), new PropertyMetadata("aurora", LocalizationChanged));

        // Method that updates the TextBlock.TextProperty property with a new binding pointing to the relevant entry whenever the key or package changes.
        private static void LocalizationChanged(DependencyObject depObj, DependencyPropertyChangedEventArgs e) {
            if (!(depObj is TextBlock textBlock)) return; 
            textBlock.SetBinding(TextBlock.TextProperty, new Binding($"[{GetKey(depObj)}, {GetPackage(depObj)}]") { Source = TranslationSource.Instance });
        }
    }



    /// <summary>
    /// Some utility functions for helping with localization and various cultures.
    /// </summary>
    public static class CultureUtils {
        /// <summary>Gets a list of all available languages in Aurora.
        /// An available language is one that has a directory in the Localization directory and has a flag icon.</summary>
        public static IEnumerable<CultureInfo> AvailableCultures { get; } =
            new DirectoryInfo(TranslationSource.LanguageFileBaseDir).EnumerateDirectories()
                .Where(d => IsCultureValid(d.Name) && File.Exists(Path.Combine(d.FullName, "icon.png")))
                .Select(d => new CultureInfo(d.Name));

        /// <summary>Returns the user's current culture IETF tag, or defaults to "en-US" if their language is not supported.</summary>
        public static string GetDefaultUserCulture() =>
            IsCultureValid(CultureInfo.CurrentCulture.IetfLanguageTag) ? CultureInfo.CurrentCulture.IetfLanguageTag : "en-US";

        /// <summary>Checks to see if the specified IETF language tag is recognised.</summary>
        public static bool IsCultureValid(string tag) =>
            CultureInfo.GetCultures(CultureTypes.AllCultures).FirstOrDefault(c => c.IetfLanguageTag == tag) != null;

        /// <summary>Returns the flag icon for a specific culture, by it's IETF tag (e.g. "en-US").</summary>
        public static BitmapImage GetIcon(string cultureTag) {
            var uri = new Uri(Path.Combine(TranslationSource.LanguageFileBaseDir, cultureTag, "icon.png"));
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = uri;
            bitmap.EndInit();
            return bitmap;
        }
    }
}