using System;
using System.Windows;
using System.Windows.Controls;

namespace Aurora.Controls {

    /// <summary>
    /// A simple StackPanel-like panel that can apply a uniform spacing between all children.
    /// </summary>
    public class SpacedStackPanel : Panel {

        public double SpacingAmount {
            get => (double)GetValue(SpacingAmountProperty);
            set => SetValue(SpacingAmountProperty, value);
        }
        public static readonly DependencyProperty SpacingAmountProperty =
            DependencyProperty.Register("SpacingAmount", typeof(double), typeof(SpacedStackPanel), new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsParentArrange));
        
        public double MinimumItemSize {
            get => (double)GetValue(MinimumItemSizeProperty);
            set => SetValue(MinimumItemSizeProperty, value);
        }
        public static readonly DependencyProperty MinimumItemSizeProperty =
            DependencyProperty.Register("MinimumItemSize", typeof(double), typeof(SpacedStackPanel), new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsParentArrange));

        public Orientation Orientation {
            get => (Orientation)GetValue(OrientationProperty);
            set => SetValue(OrientationProperty, value);
        }
        public static readonly DependencyProperty OrientationProperty =
            DependencyProperty.Register("Orientation", typeof(Orientation), typeof(SpacedStackPanel), new PropertyMetadata(Orientation.Vertical));
        

        protected override Size MeasureOverride(Size availableSize) {
            var isVert = Orientation == Orientation.Vertical;
            var size = (parallel: 0d, perpendicular: 0d); // Parallel/perpendicular relative to the orientation
            var inf = new Size(double.PositiveInfinity, double.PositiveInfinity);

            foreach (UIElement child in Children) {
                child.Measure(inf);
                size.parallel += Math.Max(isVert ? child.DesiredSize.Height : child.DesiredSize.Width, MinimumItemSize) + SpacingAmount;
                size.perpendicular = Math.Max(size.perpendicular, isVert ? child.DesiredSize.Width : child.DesiredSize.Height);
            }
            if (size.parallel > SpacingAmount)
                size.parallel -= SpacingAmount; // Remove the extra spacing at the end

            return isVert ? new Size(size.perpendicular, size.parallel) : new Size(size.parallel, size.perpendicular);
        }

        protected override Size ArrangeOverride(Size finalSize) {
            var isVert = Orientation == Orientation.Vertical;
            var running = 0d;
            foreach (UIElement child in Children) {
                var offset = Math.Max((MinimumItemSize - (isVert ? child.DesiredSize.Height : child.DesiredSize.Width)) / 2, 0); // Offset to centre children that don't meet minimum height
                child.Arrange(isVert
                    ? new Rect(0, running + offset, finalSize.Width, Math.Max(child.DesiredSize.Height, MinimumItemSize))
                    : new Rect(running + offset, 0, Math.Max(child.DesiredSize.Width, MinimumItemSize), finalSize.Height)
                );
                running += Math.Max(isVert ? child.DesiredSize.Height : child.DesiredSize.Width, MinimumItemSize) + SpacingAmount;
            }
            return finalSize;
        }
    }
}
