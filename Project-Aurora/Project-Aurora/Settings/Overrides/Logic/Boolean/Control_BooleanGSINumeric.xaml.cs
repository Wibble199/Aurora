using Aurora.Profiles;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows.Controls;

namespace Aurora.Settings.Overrides.Logic {
    /// <summary>
    /// Interaction logic for Control_ConditionGSINumeric.xaml
    /// </summary>
    public partial class Control_ConditionGSINumeric : UserControl {

        public Control_ConditionGSINumeric(BooleanGSINumeric context, Application application) {
            InitializeComponent();
            DataContext = context;
            SetApplication(application);
            OperatorCb.ItemsSource = Utils.EnumUtils.GetEnumItemsSource<ComparisonOperator>();
        }

        internal void SetApplication(Application application) {
            operand1Picker.Application = operand2Picker.Application = application;
        }
    }
}
