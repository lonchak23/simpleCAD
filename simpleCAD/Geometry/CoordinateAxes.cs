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

				// Dont apply scaling on coordinate axes.
				// They must have constant size.
				Point zeroPnt = new Point(0.0, 0.0);
				zeroPnt = m_owner.GetLocalPoint(zeroPnt, false);

				Point xPnt = new Point(m_owner.AxesLength, 0.0);
				xPnt = m_owner.GetLocalPoint(xPnt, false);

				Point yPnt = new Point(0.0, m_owner.AxesLength);
				yPnt = m_owner.GetLocalPoint(yPnt, false);

				thisDC.DrawLine(_pen, zeroPnt, xPnt);
				thisDC.DrawLine(_pen, zeroPnt, yPnt);

				FontFamily ff = new FontFamily("Arial");
				Typeface tf = new Typeface(ff, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);
				thisDC.DrawText(new FormattedText("X", CultureInfo.CurrentCulture, FlowDirection.LeftToRight, tf, m_owner.AxesTextSize, br), xPnt - 1.3*m_owner.AxesTextSize*new Vector(0.0, 1.0));
				thisDC.DrawText(new FormattedText("Y", CultureInfo.CurrentCulture, FlowDirection.LeftToRight, tf, m_owner.AxesTextSize, br), yPnt - 1.3*m_owner.AxesTextSize * new Vector(1.0, 0.0));
			}
		}
	}
}
