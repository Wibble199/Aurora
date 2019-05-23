using System;
using System.Windows;
using System.Windows.Controls;

namespace Aurora.Controls {

    /// <summary>
    /// A simple (vertical only) StackPanel-like panel that can apply a uniform spacing between all children.
    /// </summary>
    public class SpacedStackPanel : Panel {

        public double SpacingAmount {
            get => (double)GetValue(SpacingAmountProperty);
            set => SetValue(SpacingAmountProperty, value);
        }
        public static readonly DependencyProperty SpacingAmountProperty =
            DependencyProperty.Register("SpacingAmount", typeof(double), typeof(SpacedStackPanel), new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsParentArrange));
        
        public double MinimumItemHeight {
            get => (double)GetValue(MinimumItemHeightProperty);
            set => SetValue(MinimumItemHeightProperty, value);
        }
        public static readonly DependencyProperty MinimumItemHeightProperty =
            DependencyProperty.Register("MinimumItemHeight", typeof(double), typeof(SpacedStackPanel), new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsParentArrange));

        protected override Size MeasureOverride(Size availableSize) {
            var size = new Size();
            var inf = new Size(double.PositiveInfinity, double.PositiveInfinity);
            foreach (UIElement child in Children) {
                child.Measure(inf);
                size.Height += Math.Max(child.DesiredSize.Height, MinimumItemHeight) + SpacingAmount;
                size.Width = Math.Max(size.Width, child.DesiredSize.Width);
            }
            if (size.Height > SpacingAmount)
                size.Height -= SpacingAmount; // Remove the extra spacing at the end
            return size;
        }

        protected override Size ArrangeOverride(Size finalSize) {
            var y = 0d;
            foreach (UIElement child in Children) {
                var yOff = Math.Max((MinimumItemHeight - child.DesiredSize.Height) / 2, 0); // Offset to centre children that don't meet minimum height
                child.Arrange(new Rect(0, y + yOff, finalSize.Width, Math.Max(child.DesiredSize.Height, MinimumItemHeight)));
                y += Math.Max(child.DesiredSize.Height, MinimumItemHeight) + SpacingAmount;
            }
            return finalSize;
        }
    }
}
