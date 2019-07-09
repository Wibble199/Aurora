using Aurora.Profiles;
using Aurora.Settings.Overrides.Logic;
using System.ComponentModel;
using System.Windows.Controls;

namespace Aurora.Settings.Overrides.Logic {
    /// <summary>
    /// Interaction logic for Control_ColorFromValues.xaml
    /// </summary>
    public partial class Control_ColorFromValues : UserControl {
        public Control_ColorFromValues(ColorFromValues parent) {
            InitializeComponent();
            DataContext = new Control_ColorFromValues_Context { ParentCondition = parent };
        }

        public void SetApplication(Application app) {
            var dc = ((Control_ColorFromValues_Context)DataContext);
            dc.Application = app;
            dc.NotifyChanged("Application");
        }

        protected class Control_ColorFromValues_Context : INotifyPropertyChanged {
            public event PropertyChangedEventHandler PropertyChanged;
            public void NotifyChanged(string prop) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
            public Application Application { get; set; }
            public ColorFromValues ParentCondition { get; set; }
        }
    }
}
