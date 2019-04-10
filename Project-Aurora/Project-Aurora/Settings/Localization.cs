using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Resources;
using System.Windows.Data;
using System.Windows.Markup;

namespace Aurora.Settings.Localization {

    /// <summary>
    /// The class that stores the current culture information and fetches translations for the Localization resource mananager.
    /// </summary>
    /// <remarks>
    /// This code has been slightly adapted and commented from Jakub Fijałkowski's code.
    /// Available here: https://codinginfinity.me/post/2015-05-10/localization_of_a_wpf_app_the_simple_approach
    /// </remarks>
    public class TranslationSource : INotifyPropertyChanged {

        /// <summary>Event called when the culture information is changed, so that XAML elements with the Loc binding know they need to update their values.</summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>Get the singleton instance of <see cref="TranslationSource" />.</summary>
        public static TranslationSource Instance { get; } = new TranslationSource();

        private readonly ResourceManager resManager = Aurora.Localization.Resources.ResourceManager;
        private CultureInfo currentCulture = new CultureInfo(Global.Configuration.LanguageIETF);

        /// <summary>Fetches the string stored in the resource file under the given key in the <see cref="CurrentCulture" /> culture.</summary>
        public string this[string key] => resManager.GetString(key, currentCulture);

        /// <summary>Gets or sets the current culture which is used to find the correct translation.
        /// Will update all XAML elements that make use of the Loc binding when the culture is set.</summary>
        public CultureInfo CurrentCulture {
            get => currentCulture;
            set {
                if (currentCulture != value) {
                    currentCulture = value;
                    Global.Configuration.LanguageIETF = value.IetfLanguageTag;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("")); // Empty string means all properties have changed (including all values with indexers)
                }
            }
        }

        /// <summary>
        /// Gets or sets the IETF Language Tag for the current culture.
        /// </summary>
        public string CurrentCultureIetfTag {
            get => CurrentCulture.IetfLanguageTag;
            set => CurrentCulture = new CultureInfo(value);
        }
    }


    /// <summary>
    /// Binding extension class that provides a method of fetching the translation in the <see cref="TranslationSource.CurrentCulture" /> culture for a particular string key.
    /// </summary>
    public class LocExtension : Binding {

        private string prefix = "";
        private string suffix = "";
        private string[] insertValues = null;

        public LocExtension(string name) : base("[" + name + "]") {
            // One way binding since you can't set back to the resource dictionary... obviously.
            Mode = BindingMode.OneWay;

            // Set the object source of the binding to be the TranslationSource (which provides the strings)
            Source = TranslationSource.Instance;

            // E.G. if the `name` was "Test", the result of the binding would be TranslationSource.Instance[Test],
            // which is Localization.Resources.ResourceManager.GetString("Test", CurrentCulture);
        }

        /// <summary>A substring that is prepended to the start of the localized string.</summary>
        public string Prefix {
            get => prefix;
            set { prefix = value; SetStringFormat(); }
        }

        /// <summary>A substring that is appended to the end of the localized string.</summary>
        public string Suffix {
            get => suffix;
            set { suffix = value; SetStringFormat(); }
        }

        /// <summary>One or more substrings that are passed to the converter to insert them into the translated string.
        /// <para>E.G. if the result from localisation was "Enable {0} profile", and Values was ["Minecraft"], the result
        /// would be "Enable Minecraft profile".</para></summary>
        public string[] InsertValues {
            get => insertValues;
            set { insertValues = value; Converter = value == null ? null : new StringFormatterConverter(value); }
        }

        /// <summary> A string that will be inserted into the localized string. This is shorthand for adding an InsertValues of one element.</summary>
        public string InsertValue { set => InsertValues = new[] { value }; }

        /// <summary>Update the BindingBase's StringFormat property with the specified prefix and suffix.</summary>
        /// <remarks>We use the StringFormat system to take the resulting value ("{0}") and add the suffix/prefix The value
        /// passed to this is the value AFTER running though the converter.</remarks>
        private void SetStringFormat()
            => StringFormat = Prefix + "{0}" + Suffix;


        /// <summary>
        /// Value converter that takes a string value and formats it with the given values.
        /// <para>E.G. Converting the string "{0} and {1}", with insert values "a", "b" we get "a and b".</para>
        /// </summary>
        /// <seealso cref="string.Format(string, object[])"/>
        private class StringFormatterConverter : IValueConverter {

            private string[] insertValues;

            /// <summary>Creates a new formatter converter with the given substitution values which will be formatted into the string.</summary>
            public StringFormatterConverter(string[] insertValues) { this.insertValues = insertValues; }

            public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => string.Format(value.ToString(), insertValues);
            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
        }
    }


    /// <summary>
    /// Some utility functions for helping with localization and various cultures.
    /// </summary>
    public static class CultureUtils {
        /// <summary>Gets a list of all available languages in Aurora.</summary>
        public static IEnumerable<CultureInfo> AvailableCultures { get; } = GetAvailableCultures();

        /// <summary>Gets <see cref="CultureInfo"/>s of all available localization resource files which have been embedded in Aurora.</summary>
        /// <remarks>Adapted from: https://stackoverflow.com/a/3227549/1305670 </remarks>
        private static IEnumerable<CultureInfo> GetAvailableCultures() {
            var programLocation = Process.GetCurrentProcess().MainModule.FileName;
            var resourceFileName = Path.GetFileNameWithoutExtension(programLocation) + ".resources.dll";
            var rootDir = new DirectoryInfo(Path.GetDirectoryName(programLocation));
            var available = from c in CultureInfo.GetCultures(CultureTypes.AllCultures)
                            join d in rootDir.EnumerateDirectories() on c.IetfLanguageTag equals d.Name
                            where d.EnumerateFiles(resourceFileName).Any()
                            select c;

            // Have to add en-US manually since it's the default and thus does not have a folder and won't appear in the available list
            return new List<CultureInfo> { new CultureInfo("en-US") }.Concat(available);
        }

        /// <summary>Returns the user's current culture IETF tag, or defaults to "en-US" if their language is not supported.</summary>
        public static string GetDefaultUserCulture() =>
            AvailableCultures.Contains(CultureInfo.CurrentCulture) ? CultureInfo.CurrentCulture.IetfLanguageTag : "en-US";
    }
}