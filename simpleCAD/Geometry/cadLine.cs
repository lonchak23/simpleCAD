using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace simpleCAD.Geometry
{
	internal class cadLine : ICadGeometry
	{
		private Point m_FirstPnt = new Point();
		private Point m_SecondPnt = new Point();

		private bool m_bFirstPnt_Setted = false;
		private bool m_bSecondPnt_Setted = false;

		private GeometryGripX_Property m_FirstPointX_Property = null;
		private GeometryGripY_Property m_FirstPointY_Property = null;
		private GeometryGripX_Property m_SecondPointX_Property = null;
		private GeometryGripY_Property m_SecondPointY_Property = null;

		private Brush m_Color = Brushes.Black;
		private double m_Thickness = 2.0;

		public cadLine()
		{
			m_FirstPointX_Property = new GeometryGripX_Property(this, "Start point X", 0);
			m_FirstPointY_Property = new GeometryGripY_Property(this, "Start point Y", 0);
			m_SecondPointX_Property = new GeometryGripX_Property(this,"End point X", 1);
			m_SecondPointY_Property = new GeometryGripY_Property(this, "End point Y", 1);
		}

		//=============================================================================
		public bool IsPlaced
		{
			get
			{
				return m_bFirstPnt_Setted && m_bSecondPnt_Setted;
			}
		}

		//=============================================================================
		public void OnMouseLeftButtonClick(Point globalPoint)
		{
			if (!m_bFirstPnt_Setted)
			{
				m_FirstPnt = globalPoint;
				m_bFirstPnt_Setted = true;
			}
			else if (!m_bSecondPnt_Setted)
			{
				m_SecondPnt = globalPoint;
				m_bSecondPnt_Setted = true;
			}
		}

		//=============================================================================
		public void OnMouseMove(Point globalPoint)
		{
			if (!m_bFirstPnt_Setted)
				m_FirstPnt = globalPoint;
			else if (!m_bSecondPnt_Setted)
				m_SecondPnt = globalPoint;
		}

		//=============================================================================
		public void Draw(ICoordinateSystem cs, DrawingContext dc)
		{
			if (cs != null && dc != null)
			{
				Pen _pen = new Pen(m_Color, m_Thickness);
				dc.DrawLine(_pen, cs.GetLocalPoint(m_FirstPnt), cs.GetLocalPoint(m_SecondPnt));
			}
		}

		//=============================================================================
		public DrawingVisual GetGeometryWrapper() { return null; }

		//=============================================================================
		public List<Point> GetGripPoints()
		{
			List<Point> grips = new List<Point>();
			grips.Add(m_FirstPnt);
			grips.Add(m_SecondPnt);

			return grips;
		}

		//=============================================================================
		public bool SetGripPoint(int gripIndex, Point pnt)
		{
			if (!IsPlaced)
				return false;

			if (0 == gripIndex || 1 == gripIndex)
			{

				if (0 == gripIndex)
					m_FirstPnt = pnt;
				else
					m_SecondPnt = pnt;
			}

			return false;
		}

		//=============================================================================
		private List<PropertyViewModel> m_Properties = null;
		public List<PropertyViewModel> Properties
		{
			get
			{
				if (m_Properties == null)
				{
					m_Properties = new List<PropertyViewModel>();
					m_Properties.Add(new GeometryType_Property(this, "Line"));

					List<Point> grips = GetGripPoints();
					if (grips.Count == 2)
					{
						m_Properties.Add(m_FirstPointX_Property);
						m_Properties.Add(m_FirstPointY_Property);

						m_Properties.Add(m_SecondPointX_Property);
						m_Properties.Add(m_SecondPointY_Property);
					}

					m_Properties.Add(new PropertyViewModel(this, Constants.SYSNAME_PROP_COLOR));
					m_Properties.Add(new PropertyViewModel(this, Constants.SYSNAME_PROP_THICKNESS));
				}
				return m_Properties;
			}
		}

		public object GetPropertyValue(string strPropSysName)
		{
			if (string.IsNullOrEmpty(strPropSysName))
				return null;

			if (Constants.SYSNAME_PROP_COLOR == strPropSysName)
				return m_Color;
			else if (Constants.SYSNAME_PROP_THICKNESS == strPropSysName)
				return m_Thickness;

			return null;
		}

		public bool SetPropertyValue(string strPropSysName, object propValue)
		{
			if (string.IsNullOrEmpty(strPropSysName))
				return false;

			if (Constants.SYSNAME_PROP_COLOR == strPropSysName)
			{
				try
				{
					BrushConverter bc = new BrushConverter();
					m_Color = bc.ConvertFrom(propValue) as Brush;
					return true;
				}
				catch { }
			}
			else if (Constants.SYSNAME_PROP_THICKNESS == strPropSysName)
			{
				try
				{
					m_Thickness = System.Convert.ToDouble(propValue);
					return true;
				}
				catch { }
			}

			return false;
		}
	}
}
