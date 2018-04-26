using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace simpleCAD.Geometry
{
	internal class cadGrip : DrawingVisual
	{
		private SimpleCAD m_plot = null;
		private ICadGeometry m_owner = null;
		private int m_gripIndex = -1;

		//=============================================================================
		public cadGrip(SimpleCAD plot, ICadGeometry owner, int gripIndex)
		{
			m_plot = plot;
			m_owner = owner;
			m_gripIndex = gripIndex;

			Draw();
		}

		//=============================================================================
		public Point GripLocalPoint()
		{
			if (m_plot != null && m_owner != null && m_gripIndex >= 0)
			{
				List<Point> pnts = m_owner.GetGripPoints();
				if (pnts != null && m_gripIndex < pnts.Count)
					return m_plot.GetLocalPoint(pnts[m_gripIndex]);
			}

			return new Point();
		}

		//=============================================================================
		public bool Move(Point globalPnt)
		{
			bool result = false;
			if (m_owner != null && m_gripIndex >= 0)
				result = m_owner.SetGripPoint(m_gripIndex, globalPnt);
			Draw();

			return result;
		}

		//=============================================================================
		private void Draw()
		{
			using (DrawingContext dc = this.RenderOpen())
			{
				Vector vec = new Vector(4.0, 4.0);
				Point pnt = GripLocalPoint();
				Point pnt1 = pnt - vec;
				Point pnt2 = pnt + vec;
				Rect rect = new Rect(pnt1, pnt2);
				Pen _pen = new Pen(Brushes.Black, 1.0);

				Brush fillBrush = Brushes.Blue;
				if (m_plot != null)
					fillBrush = m_plot.SelectionBrush;

				dc.DrawRectangle(fillBrush, _pen, rect);
			}
		}

		//=============================================================================
		public void Update()
		{
			Draw();
		}
	}
}
