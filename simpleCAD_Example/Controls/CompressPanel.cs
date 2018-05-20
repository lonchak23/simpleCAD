using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace simpleCAD_Example.Controls
{
	// Can compress children in staking direction.
	public class CompressPanel : Panel
	{
		//=============================================================================
		// The direction of stacking
		public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register(
			"Orientation",
			typeof(Orientation),
			typeof(CompressPanel),
			new FrameworkPropertyMetadata(Orientation.Vertical, FrameworkPropertyMetadataOptions.AffectsMeasure));
		//=============================================================================
		public Orientation Orientation
		{
			get { return (Orientation)GetValue(OrientationProperty); }
			set { SetValue(OrientationProperty, value); }
		}

		//=============================================================================
		protected override Size MeasureOverride(Size availableSize)
		{
			Size desiredSize = new Size();

			if (Children.Count == 0)
				return desiredSize;

			double rSizeInStackingDirection = 0.0;
			if (Orientation.Vertical == Orientation)
				rSizeInStackingDirection = availableSize.Height / Children.Count;
			else
				rSizeInStackingDirection = availableSize.Width / Children.Count;

			foreach(UIElement child in Children)
			{
				if (child == null)
					continue;

				Size childSize = availableSize;
				if (Orientation.Vertical == Orientation)
					childSize.Height = rSizeInStackingDirection;
				else
					childSize.Width = rSizeInStackingDirection;

				//
				child.Measure(childSize);

				if (Orientation.Vertical == Orientation)
				{
					desiredSize.Width = Math.Max(desiredSize.Width, child.DesiredSize.Width);
					desiredSize.Height += child.DesiredSize.Height;
				}
				else
				{
					desiredSize.Width += child.DesiredSize.Width;
					desiredSize.Height = Math.Max(desiredSize.Height, child.DesiredSize.Height);
				}
			}

			return desiredSize;
		}

		//=============================================================================
		protected override Size ArrangeOverride(Size finalSize)
		{
			double rOffset = 0.0;

			if (Children.Count == 0)
				return finalSize;

			double rSizeInStackingDirection = 0.0;
			if (Orientation.Vertical == Orientation)
				rSizeInStackingDirection = finalSize.Height / Children.Count;
			else
				rSizeInStackingDirection = finalSize.Width / Children.Count;

			//
			// Arrange children
			foreach (UIElement child in Children)
			{
				if (child == null)
					continue;

				if (Orientation.Vertical == Orientation)
				{
					double childHeight = child.DesiredSize.Height;
					if (rSizeInStackingDirection < childHeight)
						childHeight = rSizeInStackingDirection;

					child.Arrange(new Rect(0, rOffset, finalSize.Width, childHeight));

					rOffset += childHeight;
				}
				else
				{
					double childWidth = child.DesiredSize.Width;
					if (rSizeInStackingDirection < childWidth)
						childWidth = rSizeInStackingDirection;

					child.Arrange(new Rect(rOffset, 0, childWidth, finalSize.Height));

					rOffset += childWidth;
				}
			}

			return finalSize;
		}
	}
}
