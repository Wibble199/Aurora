using Aurora.Profiles;
using Aurora.Settings.Overrides.Logic;
using System.ComponentModel;
using System.Windows.Controls;

namespace Aurora.Settings.Overrides.Logic {
    /// <summary>
    /// Interaction logic for Control_ColorFromValues.xaml
    /// </summary>
    public partial class Control_KeySequenceFromValues : UserControl {
        public Control_KeySequenceFromValues(KeySequenceFromValues parent) {
            InitializeComponent();
            DataContext = new Control_KeySequenceFromValues_Context { ParentCondition = parent };
        }

        public void SetApplication(Application app) {
            var dc = ((Control_KeySequenceFromValues_Context)DataContext);
            dc.Application = app;
            dc.NotifyChanged("Application");
        }

        protected class Control_KeySequenceFromValues_Context : INotifyPropertyChanged {
            public event PropertyChangedEventHandler PropertyChanged;
            public void NotifyChanged(string prop) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
            public Application Application { get; set; }
            public KeySequenceFromValues ParentCondition { get; set; }
        }
    }
}
