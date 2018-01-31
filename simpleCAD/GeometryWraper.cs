using simpleCAD.Geometry;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace simpleCAD
{
	/// <summary>
	/// DrawingVisual wrap around ICadGeometry.
	/// DrawingVisual.DrawingContext need for drawing ICadGeometry.
	/// </summary>
	internal class GeometryWraper : DrawingVisual, ICadGeometry
	{
		public GeometryWraper(DrawingHost owner, ICadGeometry geom)
		{
			m_owner = owner;
			m_geometry = geom;
		}

		//---------------------------------------------------------
		private DrawingHost m_owner = null;
		public DrawingHost Owner { get { return m_owner; } }
		//---------------------------------------------------------
		private ICadGeometry m_geometry = null;
		public ICadGeometry Geometry { get { return m_geometry; } }
		
		public bool IsPlaced
		{
			get
			{
				if (m_geometry != null)
					return m_geometry.IsPlaced;
				return true;
			}
		}
		public List<PropertyViewModel> Properties
		{
			get
			{
				if (m_geometry != null)
					return m_geometry.Properties;

				return new List<PropertyViewModel>();
			}
		}

		public void Draw(ICoordinateSystem cs, DrawingContext dc)
		{
			if(m_geometry != null)
			{
				using (DrawingContext thisDC = this.RenderOpen())
				{
					m_geometry.Draw(cs, thisDC);
				}
			}
		}

		public DrawingVisual GetGeometryWrapper() { return this; }

		public void OnMouseLeftButtonClick(Point globalPoint)
		{
			if (m_geometry != null)
				m_geometry.OnMouseLeftButtonClick(globalPoint);
		}
		public void OnMouseMove(Point globalPoint)
		{
			if (m_geometry != null)
			{
				m_geometry.OnMouseMove(globalPoint);
				Draw(m_owner, null);
			}
		}
		public List<Point> GetGripPoints()
		{
			if (m_geometry != null)
				return m_geometry.GetGripPoints();

			return new List<Point>();
		}
		public bool SetGripPoint(int gripIndex, Point pnt)
		{
			if (m_geometry != null && m_owner != null)
			{
				bool bRes = m_geometry.SetGripPoint(gripIndex, pnt);
				Draw(m_owner, null);
				return bRes;
			}

			return false;
		}

		public object GetPropertyValue(string strPropSysName)
		{
			if (m_geometry != null)
				return m_geometry.GetPropertyValue(strPropSysName);

			return null;
		}
		public bool SetPropertyValue(string strPropSysName, object propValue)
		{
			if (m_geometry != null)
			{
				bool bRes = m_geometry.SetPropertyValue(strPropSysName, propValue);
				Draw(m_owner, null);
				return bRes;
			}

			return false;
		}
	}
}
