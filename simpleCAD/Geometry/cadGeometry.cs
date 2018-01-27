using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace simpleCAD.Geometry
{
	public abstract class cadGeometry : DrawingVisual
	{
		protected GeometryColor_Property m_ColorProperty = null;
		protected GeometryThickness_Property m_ThicknessProperty = null;
		protected DrawingHost m_owner = null;

		protected cadGeometry(DrawingHost owner)
		{
			m_ColorProperty = new GeometryColor_Property(this);
			m_ThicknessProperty = new GeometryThickness_Property(this);
			m_owner = owner;
		}

		//=============================================================================
		public Point GetLocalPoint(Point globalPnt)
		{
			if (m_owner != null)
				return m_owner.GetLocalPoint(globalPnt);

			return globalPnt;
		}

		//=============================================================================
		public virtual void Draw() { }

		//=============================================================================
		public virtual void SetPoint(Point pnt, bool bByClick) { }

		//=============================================================================
		public virtual bool IsInitialized()
		{
			return false;
		}

		//=============================================================================
		public virtual List<Point> GetGripPoints() { return new List<Point>();} 

		//=============================================================================
		public virtual bool SetGripPoint(int gripIndex, Point pnt)
		{
			return false;
		}

		//=============================================================================
		public virtual List<PropertyViewModel> Properties { get{ return new List<PropertyViewModel>();} }

		public static double DEF_PRECISION = 0.0001;

		//=============================================================================
		protected Brush m_Color = Brushes.Black;
		public virtual Brush Color
		{
			get { return m_Color; }
			set
			{
				m_Color = value;
				m_ColorProperty.Update_Value();
				Draw();
			}
		}

		//=============================================================================
		protected double m_Thickness = 2.0;
		public virtual double Thickness
		{
			get { return m_Thickness; }
			set
			{
				if (Math.Abs(m_Thickness - value) > DEF_PRECISION)
				{
					m_Thickness = value;
					m_ThicknessProperty.Update_Value();
					Draw();
				}
			}
		}
	}
}
