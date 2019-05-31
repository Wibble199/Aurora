using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;


namespace Aurora.Settings {
	
	//          !!!! DO NOT EDIT THIS !!!!
	//
	// Auto-generated Configuration model. Edit Configuration.tt instead and re-generate.
	//
	public partial class Configuration : Settings {

		private AppExitMode closeMode = AppExitMode.Ask;
		[JsonProperty(PropertyName = "close_mode")]
		public AppExitMode CloseMode { get => closeMode; set { closeMode = value; InvokePropertyChanged(); } }

		private bool startSilently = false;
		[JsonProperty(PropertyName = "start_silently")]
		public bool StartSilently { get => startSilently; set { startSilently = value; InvokePropertyChanged(); } }

		private bool updatesCheckOnStartup = true;
		[JsonProperty(PropertyName = "updates_check_on_start_up")]
		public bool UpdatesCheckOnStartup { get => updatesCheckOnStartup; set { updatesCheckOnStartup = value; InvokePropertyChanged(); } }

		private string languageIETF = Localization.CultureUtils.GetDefaultUserCulture();
		public string LanguageIETF { get => languageIETF; set { languageIETF = value; InvokePropertyChanged(); } }

		private string themeName = "";
		public string ThemeName { get => themeName; set { themeName = value; InvokePropertyChanged(); } }

		private bool allowPeripheralDevices = true;
		[JsonProperty(PropertyName = "allow_peripheral_devices")]
		public bool AllowPeripheralDevices { get => allowPeripheralDevices; set { allowPeripheralDevices = value; InvokePropertyChanged(); } }

		private bool allowWrappersInBackground = true;
		[JsonProperty(PropertyName = "allow_wrappers_in_background")]
		public bool AllowWrappersInBackground { get => allowWrappersInBackground; set { allowWrappersInBackground = value; InvokePropertyChanged(); } }

		private bool allowAllLogitechBitmaps = true;
		[JsonProperty(PropertyName = "allow_all_logitech_bitmaps")]
		public bool AllowAllLogitechBitmaps { get => allowAllLogitechBitmaps; set { allowAllLogitechBitmaps = value; InvokePropertyChanged(); } }

		private bool useVolumeAsBrightness = false;
		[JsonProperty(PropertyName = "use_volume_as_brightness")]
		public bool UseVolumeAsBrightness { get => useVolumeAsBrightness; set { useVolumeAsBrightness = value; InvokePropertyChanged(); } }

		private float globalBrightness = 1;
		[JsonProperty(PropertyName = "global_brightness")]
		public float GlobalBrightness { get => globalBrightness; set { globalBrightness = value; InvokePropertyChanged(); } }

		private float keyboardBrightness = 1;
		[JsonProperty(PropertyName = "keyboard_brightness_modifier")]
		public float KeyboardBrightness { get => keyboardBrightness; set { keyboardBrightness = value; InvokePropertyChanged(); } }

		private float peripheralBrightness = 1;
		[JsonProperty(PropertyName = "peripheral_brightness_modifier")]
		public float PeripheralBrightness { get => peripheralBrightness; set { peripheralBrightness = value; InvokePropertyChanged(); } }

		private bool getDevReleases;
		public bool GetDevReleases { get => getDevReleases; set { getDevReleases = value; InvokePropertyChanged(); } }

		private bool getPointerUpdates;
		public bool GetPointerUpdates { get => getPointerUpdates; set { getPointerUpdates = value; InvokePropertyChanged(); } }

		private bool highPriority;
		public bool HighPriority { get => highPriority; set { highPriority = value; InvokePropertyChanged(); } }

		private BitmapAccuracy bitmapAccuracy = BitmapAccuracy.Okay;
		public BitmapAccuracy BitmapAccuracy { get => bitmapAccuracy; set { bitmapAccuracy = value; InvokePropertyChanged(); } }

		private ApplicationDetectionMode detectionMode = ApplicationDetectionMode.WindowsEvents;
		[JsonProperty(PropertyName = "detection_mode")]
		public ApplicationDetectionMode DetectionMode { get => detectionMode; set { detectionMode = value; InvokePropertyChanged(); } }

		private ObservableCollection<string> excludedPrograms = new ObservableCollection<string>();
		[JsonProperty(PropertyName = "excluded_programs")]
		public ObservableCollection<string> ExcludedPrograms {
			get => excludedPrograms;
			set {
				excludedPrograms = value;
				if (value != null)
					value.CollectionChanged += (sender, e) => InvokePropertyChanged("ExcludedPrograms");
			}
		}

		private bool overlaysInPreview = false;
		public bool OverlaysInPreview { get => overlaysInPreview; set { overlaysInPreview = value; InvokePropertyChanged(); } }

		private List<string> profileOrder = new List<string>();
		public List<string> ProfileOrder { get => profileOrder; set { profileOrder = value; InvokePropertyChanged(); } }

		private MouseOrientationType mouseOrientation = MouseOrientationType.RightHanded;
		[JsonProperty(PropertyName = "mouse_orientation")]
		public MouseOrientationType MouseOrientation { get => mouseOrientation; set { mouseOrientation = value; InvokePropertyChanged(); } }

		private PreferredKeyboard keyboardBrand = PreferredKeyboard.None;
		[JsonProperty(PropertyName = "keyboard_brand")]
		public PreferredKeyboard KeyboardBrand { get => keyboardBrand; set { keyboardBrand = value; InvokePropertyChanged(); } }

		private PreferredKeyboardLocalization keyboardLocalization = PreferredKeyboardLocalization.None;
		[JsonProperty(PropertyName = "keyboard_localization")]
		public PreferredKeyboardLocalization KeyboardLocalization { get => keyboardLocalization; set { keyboardLocalization = value; InvokePropertyChanged(); } }

		private PreferredMouse mousePreference = PreferredMouse.None;
		[JsonProperty(PropertyName = "mouse_preference")]
		public PreferredMouse MousePreference { get => mousePreference; set { mousePreference = value; InvokePropertyChanged(); } }

		private KeycapType virtualKeyboardKeycapType = KeycapType.Default;
		[JsonProperty(PropertyName = "virtualkeyboard_keycap_type")]
		public KeycapType VirtualKeyboardKeycapType { get => virtualKeyboardKeycapType; set { virtualKeyboardKeycapType = value; InvokePropertyChanged(); } }

		private bool devicesDisableKeyboard = false;
		[JsonProperty(PropertyName = "devices_disable_keyboard")]
		public bool DevicesDisableKeyboard { get => devicesDisableKeyboard; set { devicesDisableKeyboard = value; InvokePropertyChanged(); } }

		private bool devicesDisableMouse = false;
		[JsonProperty(PropertyName = "devices_disable_mouse")]
		public bool DevicesDisableMouse { get => devicesDisableMouse; set { devicesDisableMouse = value; InvokePropertyChanged(); } }

		private bool devicesDisableHeadset = false;
		[JsonProperty(PropertyName = "devices_disable_headset")]
		public bool DevicesDisableHeadset { get => devicesDisableHeadset; set { devicesDisableHeadset = value; InvokePropertyChanged(); } }

		private bool unifiedHidDisabled = false;
		[JsonProperty(PropertyName = "unified_hid_disabled")]
		public bool UnifiedHidDisabled { get => unifiedHidDisabled; set { unifiedHidDisabled = value; InvokePropertyChanged(); } }

		private HashSet<Type> devicesDisabled = new HashSet<Type>();
		[JsonProperty(PropertyName = "devices_disabled")]
		public HashSet<Type> DevicesDisabled { get => devicesDisabled; set { devicesDisabled = value; InvokePropertyChanged(); } }

		private bool redistFirstTime = true;
		[JsonProperty(PropertyName = "redist_first_time")]
		public bool RedistFirstTime { get => redistFirstTime; set { redistFirstTime = value; InvokePropertyChanged(); } }

		private bool logitechFirstTime = true;
		[JsonProperty(PropertyName = "logitech_first_time")]
		public bool LogitechFirstTime { get => logitechFirstTime; set { logitechFirstTime = value; InvokePropertyChanged(); } }

		private bool corsairFirstTime = true;
		[JsonProperty(PropertyName = "corsair_first_time")]
		public bool CorsairFirstTime { get => corsairFirstTime; set { corsairFirstTime = value; InvokePropertyChanged(); } }

		private bool razerFirstTime = true;
		[JsonProperty(PropertyName = "razer_first_time")]
		public bool RazerFirstTime { get => razerFirstTime; set { razerFirstTime = value; InvokePropertyChanged(); } }

		private bool steelSeriesFirstTime = true;
		[JsonProperty(PropertyName = "steelseries_first_time")]
		public bool SteelSeriesFirstTime { get => steelSeriesFirstTime; set { steelSeriesFirstTime = value; InvokePropertyChanged(); } }

		private bool dualShockFirstTime = true;
		[JsonProperty(PropertyName = "dualshock_first_time")]
		public bool DualShockFirstTime { get => dualShockFirstTime; set { dualShockFirstTime = value; InvokePropertyChanged(); } }

		private bool roccatFirstTime = true;
		[JsonProperty(PropertyName = "roccat_first_time")]
		public bool RoccatFirstTime { get => roccatFirstTime; set { roccatFirstTime = value; InvokePropertyChanged(); } }

		private bool timeBasedDimmingEnabled = false;
		[JsonProperty(PropertyName = "time_based_dimming_enabled")]
		public bool TimeBasedDimmingEnabled { get => timeBasedDimmingEnabled; set { timeBasedDimmingEnabled = value; InvokePropertyChanged(); } }

		private bool timeBasedDimmingAffectGames = false;
		[JsonProperty(PropertyName = "time_based_dimming_affect_games")]
		public bool TimeBasedDimmingAffectGames { get => timeBasedDimmingAffectGames; set { timeBasedDimmingAffectGames = value; InvokePropertyChanged(); } }

		private int timeBasedDimmingStartHour = 21;
		[JsonProperty(PropertyName = "time_based_dimming_start_hour")]
		public int TimeBasedDimmingStartHour { get => timeBasedDimmingStartHour; set { timeBasedDimmingStartHour = value; InvokePropertyChanged(); } }

		private int timeBasedDimmingStartMinute = 0;
		[JsonProperty(PropertyName = "time_based_dimming_start_minute")]
		public int TimeBasedDimmingStartMinute { get => timeBasedDimmingStartMinute; set { timeBasedDimmingStartMinute = value; InvokePropertyChanged(); } }

		private int timeBasedDimmingEndHour = 8;
		[JsonProperty(PropertyName = "time_based_dimming_end_hour")]
		public int TimeBasedDimmingEndHour { get => timeBasedDimmingEndHour; set { timeBasedDimmingEndHour = value; InvokePropertyChanged(); } }

		private int timeBasedDimmingEndMinute = 0;
		[JsonProperty(PropertyName = "time_based_dimming_end_minute")]
		public int TimeBasedDimmingEndMinute { get => timeBasedDimmingEndMinute; set { timeBasedDimmingEndMinute = value; InvokePropertyChanged(); } }

		private bool nightTimeEnabled = false;
		[JsonProperty(PropertyName = "nighttime_enabled")]
		public bool NightTimeEnabled { get => nightTimeEnabled; set { nightTimeEnabled = value; InvokePropertyChanged(); } }

		private int nightTimeStartHour = 20;
		[JsonProperty(PropertyName = "nighttime_start_hour")]
		public int NightTimeStartHour { get => nightTimeStartHour; set { nightTimeStartHour = value; InvokePropertyChanged(); } }

		private int nightTimeStartMinute = 0;
		[JsonProperty(PropertyName = "nighttime_start_minute")]
		public int NightTimeStartMinute { get => nightTimeStartMinute; set { nightTimeStartMinute = value; InvokePropertyChanged(); } }

		private int nightTimeEndHour = 7;
		[JsonProperty(PropertyName = "nighttime_end_hour")]
		public int NightTimeEndHour { get => nightTimeEndHour; set { nightTimeEndHour = value; InvokePropertyChanged(); } }

		private int nightTimeEndMinute = 0;
		[JsonProperty(PropertyName = "nighttime_end_minute")]
		public int NightTimeEndMinute { get => nightTimeEndMinute; set { nightTimeEndMinute = value; InvokePropertyChanged(); } }

		private IdleEffects idleType = IdleEffects.None;
		[JsonProperty(PropertyName = "idle_type")]
		public IdleEffects IdleType { get => idleType; set { idleType = value; InvokePropertyChanged(); } }

		private int idleDelay = 5;
		[JsonProperty(PropertyName = "idle_delay")]
		public int IdleDelay { get => idleDelay; set { idleDelay = value; InvokePropertyChanged(); } }

		private float idleSpeed = 1;
		[JsonProperty(PropertyName = "idle_speed")]
		public float IdleSpeed { get => idleSpeed; set { idleSpeed = value; InvokePropertyChanged(); } }

		private Color idlePrimaryColor = Color.FromArgb(0, 255, 0);
		[JsonProperty(PropertyName = "idle_effect_primary_color")]
		public Color IdlePrimaryColor { get => idlePrimaryColor; set { idlePrimaryColor = value; InvokePropertyChanged(); } }

		private Color idleSecondaryColor = Color.FromArgb(0, 0, 0);
		[JsonProperty(PropertyName = "idle_effect_secondary_color")]
		public Color IdleSecondaryColor { get => idleSecondaryColor; set { idleSecondaryColor = value; InvokePropertyChanged(); } }

		private int idleAmount = 5;
		[JsonProperty(PropertyName = "idle_amount")]
		public int IdleAmount { get => idleAmount; set { idleAmount = value; InvokePropertyChanged(); } }

		private float idleFrequency = 2.5f;
		[JsonProperty(PropertyName = "idle_frequency")]
		public float IdleFrequency { get => idleFrequency; set { idleFrequency = value; InvokePropertyChanged(); } }

		private bool bitmapDebugTopMost;
		public bool BitmapDebugTopMost { get => bitmapDebugTopMost; set { bitmapDebugTopMost = value; InvokePropertyChanged(); } }

		private bool httpDebugTopMost;
		public bool HttpDebugTopMost { get => httpDebugTopMost; set { httpDebugTopMost = value; InvokePropertyChanged(); } }

		private VariableRegistry varRegistry = new VariableRegistry();
		public VariableRegistry VarRegistry { get => varRegistry; set { varRegistry = value; InvokePropertyChanged(); } }


	}
}
