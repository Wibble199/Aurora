using System.Diagnostics;
using System.Windows.Controls;

namespace Aurora.Profiles.SkyrimSE {

    public partial class Control_SkyrimSE : UserControl {

        private Application profile;

        public Control_SkyrimSE(Application profile) {
            this.profile = profile;
            InitializeComponent();
            SetSettings();
            profile.ProfileChanged += (sender, e) => SetSettings();
        }

        private void SetSettings() {
            GameEnabled.IsChecked = profile.Settings.IsEnabled;
        }

        private void GameEnabled_Checked(object sender, System.Windows.RoutedEventArgs e) {
            profile.Settings.IsEnabled = GameEnabled.IsChecked == true;
            profile.SaveProfiles();
        }

        private void GoToNexusMods_Click(object sender, System.Windows.RoutedEventArgs e) {
            Process.Start("https://www.nexusmods.com/skyrimspecialedition/mods/34209");
        }

        private void GoToGitLab_Click(object sender, System.Windows.RoutedEventArgs e) {
            Process.Start("https://gitlab.com/wibble199/aurora-gsi-skryim-se");
        }
    }
}
