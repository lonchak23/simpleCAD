using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace simpleCAD.Geometry
{
	internal class CoordinateAxes : DrawingVisual
	{
		public CoordinateAxes(SimpleCAD owner)
		{
			m_owner = owner;
		}

		private SimpleCAD m_owner = null;
		public SimpleCAD Owner { get { return m_owner; } }

		public void Draw()
		{
			if (m_owner == null)
				return;

			using (DrawingContext thisDC = this.RenderOpen())
			{
				Brush br = new SolidColorBrush(m_owner.AxesColor);
				Pen _pen = new Pen(br, m_owner.AxesThickness);

				//
				// Draw arrows
				double rArrLen = m_owner.AxesLength * 0.15;
				double rArrWidth = m_owner.AxesLength * 0.03;

				// Dont apply scaling on coordinate axes.
				// They must have constant size.
				Point zeroPnt = new Point(0.0, 0.0);
				zeroPnt = m_owner.GetLocalPoint(zeroPnt, false);

				Point xPnt = new Point(m_owner.AxesLength - rArrLen, 0.0);
				xPnt = m_owner.GetLocalPoint(xPnt, false);

				Point yPnt = new Point(0.0, m_owner.AxesLength - rArrLen);
				yPnt = m_owner.GetLocalPoint(yPnt, false);

				thisDC.DrawLine(_pen, zeroPnt, xPnt);
				thisDC.DrawLine(_pen, zeroPnt, yPnt);

				// X arrow
				PathFigure arrow_X = new PathFigure();
				arrow_X.IsClosed = true;
				Point xPnt0 = new Point(m_owner.AxesLength, 0);
				xPnt0 = m_owner.GetLocalPoint(xPnt0, false);
				arrow_X.StartPoint = xPnt0;
				Point xPnt1 = new Point(m_owner.AxesLength - rArrLen, -rArrWidth);
				xPnt1 = m_owner.GetLocalPoint(xPnt1, false);
				arrow_X.Segments.Add(new LineSegment(xPnt1, true));
				Point xPnt2 = new Point(m_owner.AxesLength - rArrLen, rArrWidth);
				xPnt2 = m_owner.GetLocalPoint(xPnt2, false);
				arrow_X.Segments.Add(new LineSegment(xPnt2, true));

				// Y arrow
				PathFigure arrow_Y = new PathFigure();
				arrow_Y.IsClosed = true;
				Point yPnt0 = new Point(0, m_owner.AxesLength);
				yPnt0 = m_owner.GetLocalPoint(yPnt0, false);
				arrow_Y.StartPoint = yPnt0;
				Point yPnt1 = new Point(-rArrWidth, m_owner.AxesLength - rArrLen);
				yPnt1 = m_owner.GetLocalPoint(yPnt1, false);
				arrow_Y.Segments.Add(new LineSegment(yPnt1, true));
				Point yPnt2 = new Point(rArrWidth, m_owner.AxesLength - rArrLen);
				yPnt2 = m_owner.GetLocalPoint(yPnt2, false);
				arrow_Y.Segments.Add(new LineSegment(yPnt2, true));

				PathGeometry pg = new PathGeometry(new[] { arrow_X, arrow_Y });

				thisDC.DrawGeometry(br, _pen, pg);

				//
				// Draw text
				FontFamily ff = new FontFamily("Arial");
				Typeface tf = new Typeface(ff, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);

				FormattedText text_X = new FormattedText("X", CultureInfo.CurrentCulture, FlowDirection.LeftToRight, tf, m_owner.AxesTextSize, br);
				// Positive Y direction because default WPF axes have Y directed to bottom.
				// Here we draw in default WPF system.
				// Read coom in SimpleCAD.GetLocalPoint().
				thisDC.DrawText(text_X, xPnt + new Vector(0, 3));

				FormattedText text_Y = new FormattedText("Y", CultureInfo.CurrentCulture, FlowDirection.LeftToRight, tf, m_owner.AxesTextSize, br);
				// Negative Y direction because default WPF axes have Y directed to bottom.
				// Here we draw in default WPF system.
				// Read coom in SimpleCAD.GetLocalPoint().
				thisDC.DrawText(text_Y, yPnt + new Vector(-m_owner.AxesTextSize, -m_owner.AxesTextSize));
			}
		}
	}
}
