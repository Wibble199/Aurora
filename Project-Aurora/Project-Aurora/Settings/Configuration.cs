using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;

namespace Aurora.Settings {

    public class Configuration : AutoNotifyPropertyChanged<Configuration> {

        public Configuration() {
            // Add any initial event handling here.
            // The proxy will handle adding CollectionChanged and PropertyChanged events to relevant properties when their setters are called
            // (i.e. when the values are replaced), however the proxy is not capable of intercepting the constructor, so in the case of a new
            // config being made (i.e. not loaded from JSON), the events need to be added.
            ExcludedPrograms.CollectionChanged += (sender, e) => NotifyPropertyChanged("ExcludedPrograms");
        }

                
        [JsonProperty(PropertyName = "close_mode")]
        public virtual AppExitMode CloseMode { get; set; } = AppExitMode.Ask;

        [JsonProperty(PropertyName = "start_silently")]
        public virtual bool StartSilently { get; set; } = false;

        [JsonProperty(PropertyName = "updates_check_on_start_up")]
        public virtual bool UpdatesCheckOnStartup { get; set; } = true;

        public virtual string LanguageIETF { get; set; } = Localization.CultureUtils.GetDefaultUserCulture();

        public virtual string ThemeName { get; set; } = "";

        [JsonProperty(PropertyName = "allow_peripheral_devices")]
        public virtual bool AllowPeripheralDevices { get; set; } = true;

        [JsonProperty(PropertyName = "allow_wrappers_in_background")]
        public virtual bool AllowWrappersInBackground { get; set; } = true;

        [JsonProperty(PropertyName = "allow_all_logitech_bitmaps")]
        public virtual bool AllowAllLogitechBitmaps { get; set; } = true;

        [JsonProperty(PropertyName = "use_volume_as_brightness")]
        public virtual bool UseVolumeAsBrightness { get; set; } = false;

        [JsonProperty(PropertyName = "global_brightness")]
        public virtual float GlobalBrightness { get; set; } = 1;

        [JsonProperty(PropertyName = "keyboard_brightness_modifier")]
        public virtual float KeyboardBrightness { get; set; } = 1;

        [JsonProperty(PropertyName = "peripheral_brightness_modifier")]
        public virtual float PeripheralBrightness { get; set; } = 1;

        public virtual bool GetDevReleases { get; set; }

        public virtual bool GetPointerUpdates { get; set; }

        public virtual bool HighPriority { get; set; }

        public virtual BitmapAccuracy BitmapAccuracy { get; set; } = BitmapAccuracy.Okay;

        [JsonProperty(PropertyName = "detection_mode")]
        public virtual ApplicationDetectionMode DetectionMode { get; set; } = ApplicationDetectionMode.WindowsEvents;

        [JsonProperty(PropertyName = "excluded_programs")]
        public virtual ObservableCollection<string> ExcludedPrograms { get; set; } = new ObservableCollection<string>();

        public virtual bool OverlaysInPreview { get; set; } = false;

        public virtual List<string> ProfileOrder { get; set; } = new List<string>();

        [JsonProperty(PropertyName = "mouse_orientation")]
        public virtual MouseOrientationType MouseOrientation { get; set; } = MouseOrientationType.RightHanded;

        [JsonProperty(PropertyName = "keyboard_brand")]
        public virtual PreferredKeyboard KeyboardBrand { get; set; } = PreferredKeyboard.None;

        [JsonProperty(PropertyName = "keyboard_localization")]
        public virtual PreferredKeyboardLocalization KeyboardLocalization { get; set; } = PreferredKeyboardLocalization.None;

        [JsonProperty(PropertyName = "mouse_preference")]
        public virtual PreferredMouse MousePreference { get; set; } = PreferredMouse.None;

        [JsonProperty(PropertyName = "virtualkeyboard_keycap_type")]
        public virtual KeycapType VirtualKeyboardKeycapType { get; set; } = KeycapType.Default;

        [JsonProperty(PropertyName = "devices_disable_keyboard")]
        public virtual bool DevicesDisableKeyboard { get; set; } = false;

        [JsonProperty(PropertyName = "devices_disable_mouse")]
        public virtual bool DevicesDisableMouse { get; set; } = false;

        [JsonProperty(PropertyName = "devices_disable_headset")]
        public virtual bool DevicesDisableHeadset { get; set; } = false;

        [JsonProperty(PropertyName = "unified_hid_disabled")]
        public virtual bool UnifiedHidDisabled { get; set; } = false;

        [JsonProperty(PropertyName = "devices_disabled")]
        public virtual HashSet<Type> DevicesDisabled { get; set; } = new HashSet<Type>();

        [JsonProperty(PropertyName = "redist_first_time")]
        public virtual bool RedistFirstTime { get; set; } = true;

        [JsonProperty(PropertyName = "logitech_first_time")]
        public virtual bool LogitechFirstTime { get; set; } = true;

        [JsonProperty(PropertyName = "corsair_first_time")]
        public virtual bool CorsairFirstTime { get; set; } = true;

        [JsonProperty(PropertyName = "razer_first_time")]
        public virtual bool RazerFirstTime { get; set; } = true;

        [JsonProperty(PropertyName = "steelseries_first_time")]
        public virtual bool SteelSeriesFirstTime { get; set; } = true;

        [JsonProperty(PropertyName = "dualshock_first_time")]
        public virtual bool DualShockFirstTime { get; set; } = true;

        [JsonProperty(PropertyName = "roccat_first_time")]
        public virtual bool RoccatFirstTime { get; set; } = true;

        public virtual bool BitmapDebugTopMost { get; set; }

        public virtual bool HttpDebugTopMost { get; set; }

        public virtual VariableRegistry VarRegistry { get; set; } = new VariableRegistry();

    }
}
