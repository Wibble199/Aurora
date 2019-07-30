using System;
using System.Windows;
using System.Windows.Controls;

namespace Aurora.Controls {

    public partial class StringPropertyField : UserControl {

        public StringPropertyField() {
            InitializeComponent();
            ((ContentPresenter)Content).Content = this;
        }

        #region Type Property
        /// <summary>The type of the field to edit.</summary>
        public Type Type {
            get => (Type)GetValue(TypeProperty);
            set => SetValue(TypeProperty, value);
        }
        public static readonly DependencyProperty TypeProperty =
            DependencyProperty.Register("Type", typeof(Type), typeof(StringPropertyField), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));
        #endregion

        #region Value Property
        /// <summary>The value of the field to edit.</summary>
        public object Value {
            get => GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(object), typeof(StringPropertyField), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        #endregion
    }

    public class FieldEditorTemplateSelector : DataTemplateSelector {
        public override DataTemplate SelectTemplate(object item, DependencyObject container) {
            if (!(item is StringPropertyField context))
                return new DataTemplate();

            var t = context.Type.IsGenericType ? context.Type.GetGenericTypeDefinition() : context.Type;
            return context.TryFindResource(t) as DataTemplate ?? new DataTemplate();
        }
    }
}
