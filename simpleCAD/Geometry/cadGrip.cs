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
	public class cadGrip : DrawingVisual
	{
		private cadGeometry m_owner = null;
		private int m_gripIndex = -1;

		//=============================================================================
		public cadGrip(cadGeometry owner, int gripIndex)
		{
			m_owner = owner;
			m_gripIndex = gripIndex;

			Draw();
		}

		//=============================================================================
		public Point GripLocalPoint()
		{
			if (m_owner != null && m_gripIndex >= 0)
			{
				List<Point> pnts = m_owner.GetGripPoints();
				if (pnts != null && m_gripIndex < pnts.Count)
					return m_owner.GetLocalPoint(pnts[m_gripIndex]);
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
				dc.DrawRectangle(Brushes.Aqua, _pen, rect);
			}
		}

		//=============================================================================
		public void Update()
		{
			Draw();
		}
	}
}
