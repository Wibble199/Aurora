using Aurora.Settings;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace Aurora.Controls
{
    /// <summary>
    /// Interaction logic for LayerEditor.xaml
    /// </summary>
    public partial class LayerEditor : UserControl
    {
        /// <summary>Contains a map of all FreeFormObjects with their assigned controls.</summary>
        private static Dictionary<FreeFormObject, ContentControl> subcontrols = new Dictionary<FreeFormObject, ContentControl>();

        /// <summary>Colours that can be used and a count of the times they are used.</summary>
        private static Dictionary<(SolidColorBrush brush, int preference), int> colors;

        private static Canvas static_canvas;
        private static Style style;

        public LayerEditor()
        {
            InitializeComponent();

            static_canvas = editor_canvas;
            style = FindResource("DesignerItemStyle") as Style;

            colors = (FindResource("FreeformEditorColors") as SolidColorBrush[]).Select((brush, i) => (brush, i)).ToDictionary(x => x, _ => 0);
        }

        /// <summary>
        /// Creates a new control for the given <see cref="FreeFormObject"/> in the editor if one does not already exist.
        /// </summary>
        /// <param name="element">The freeform object to provide an editor control for.</param>
        /// <param name="caption">The label given to the freeform control.</param>
        public static void AddKeySequenceElement(FreeFormObject element, string caption) {
            // If this element already exists, do nothing
            if (subcontrols.ContainsKey(element)) return;

            var newcontrol = new ContentControl { Width = element.Width, Height = element.Height, Style = style, Tag = element, Foreground = GetNextColor() };
            newcontrol.SetValue(Selector.IsSelectedProperty, true);
            newcontrol.SetValue(Canvas.TopProperty, (double)(element.Y + Effects.grid_baseline_y));
            newcontrol.SetValue(Canvas.LeftProperty, (double)(element.X + Effects.grid_baseline_x));

            var transform = new RotateTransform { Angle = element.Angle };
            newcontrol.SetValue(RenderTransformProperty, transform);
            newcontrol.SizeChanged += Newcontrol_SizeChanged;

            DependencyPropertyDescriptor.FromProperty(Canvas.LeftProperty, typeof(ContentControl)).AddValueChanged(newcontrol, OnCanvasLeftChanged);
            DependencyPropertyDescriptor.FromProperty(Canvas.TopProperty, typeof(ContentControl)).AddValueChanged(newcontrol, OnCanvasTopChanged);
            DependencyPropertyDescriptor.FromProperty(RenderTransformProperty, typeof(ContentControl)).AddValueChanged(newcontrol, OnAngleChanged);

            newcontrol.Content = new TextBlock { Text = caption, IsHitTestVisible = false };

            static_canvas.Children.Add(newcontrol);
            subcontrols.Add(element, newcontrol);
        }

        /// <summary>
        /// Removes the control for the given <see cref="FreeFormObject"/> from the editor.
        /// </summary>
        public static void RemoveKeySequenceElement(FreeFormObject element) {
            if (subcontrols.TryGetValue(element, out var existingControl)) {
                ReleaseColor(existingControl.Foreground);
                static_canvas.Children.Remove(existingControl);
                subcontrols.Remove(element);
            }
        }

        /// <summary>Gets the next color for the editor based on the colors from the theme and increments the use count.</summary>
        private static SolidColorBrush GetNextColor() {
            var next = colors.OrderBy(x => x.Value).ThenBy(x => x.Key.preference).First();
            colors[next.Key]++;
            return next.Key.brush;
        }

        /// <summary>Decrements the use count for the given color.</summary>
        private static void ReleaseColor(Brush brush) {
            colors[colors.Where(x => x.Key.brush == brush).First().Key]--;
        }


        private static void OnAngleChanged(object sender, EventArgs e) {
            var item = (sender as ContentControl).GetValue(RenderTransformProperty) as RotateTransform;

            if (sender is ContentControl cc && cc.Tag is FreeFormObject ffo)
                ffo.Angle = (float)item.Angle;
        }

        private static void OnCanvasTopChanged(object sender, EventArgs e) {
            if (sender is ContentControl cc && cc.Tag is FreeFormObject ffo) {
                var top = (double)cc.GetValue(Canvas.TopProperty);
                ffo.Y = (float)top - Effects.grid_baseline_y;
            }
        }

        private static void OnCanvasLeftChanged(object sender, EventArgs e) {
            if (sender is ContentControl cc && cc.Tag is FreeFormObject ffo) {
                var left = (double)cc.GetValue(Canvas.LeftProperty);
                ffo.X = (float)left - Effects.grid_baseline_x;
            }
        }

        private static void Newcontrol_SizeChanged(object sender, SizeChangedEventArgs e) {
            if (sender is ContentControl cc && cc.Tag is FreeFormObject ffo) {
                ffo.Width = (float)cc.ActualWidth;
                ffo.Height = (float)cc.ActualHeight;
            }
        }
    }
}
