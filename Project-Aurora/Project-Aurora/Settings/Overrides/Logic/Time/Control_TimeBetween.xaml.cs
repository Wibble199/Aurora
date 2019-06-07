using System.Windows.Controls;

namespace Aurora.Settings.Overrides.Logic {

    public partial class Control_TimeBetween : UserControl {
        public Control_TimeBetween(BooleanTimeBetween context) {
            InitializeComponent();
            DataContext = context;
        }

        public void SetApplication(Profiles.Application app) {
            Value.Application = Start.Application = End.Application = app;
        }
    }
}
