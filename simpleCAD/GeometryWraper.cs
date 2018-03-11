using simpleCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Media;

namespace simpleCAD
{
	[Serializable]
	/// <summary>
	/// DrawingVisual wrap around ICadGeometry.
	/// DrawingVisual.DrawingContext need for drawing ICadGeometry.
	/// </summary>
	internal class GeometryWraper : DrawingVisual, ICadGeometry, ISerializable
	{
		public GeometryWraper(SimpleCAD owner, ICadGeometry geom)
		{
			m_owner = owner;
			m_geometry = geom;
		}

		//=============================================================================
		public static string sDisplayName = "geometryWrapper";
		public string DisplayName { get { return sDisplayName; } }

		//=============================================================================
		public ImageSource GeomImage
		{
			get
			{
				if (m_geometry != null)
					return m_geometry.GeomImage;

				return null;
			}
		}

		//=============================================================================
		private SimpleCAD m_owner = null;
		public SimpleCAD Owner { get { return m_owner; } set { m_owner = value; } }
		//=============================================================================
		private ICadGeometry m_geometry = null;
		public ICadGeometry Geometry { get { return m_geometry; } }

		//=============================================================================
		public bool IsPlaced
		{
			get
			{
				if (m_geometry != null)
					return m_geometry.IsPlaced;
				return true;
			}
		}

		//=============================================================================
		public List<Property_ViewModel> Properties
		{
			get
			{
				return m_geometry.Properties;
			}
		}

		//=============================================================================
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

		//=============================================================================
		public DrawingVisual GetGeometryWrapper() { return this; }

		//=============================================================================
		public void OnMouseLeftButtonClick(Point globalPoint)
		{
			if (m_geometry != null)
				m_geometry.OnMouseLeftButtonClick(globalPoint);
		}

		//=============================================================================
		public void OnMouseMove(Point globalPoint)
		{
			if (m_geometry != null)
			{
				m_geometry.OnMouseMove(globalPoint);
				Draw(m_owner, null);
			}
		}

		//=============================================================================
		public void OnKeyDown(System.Windows.Input.KeyEventArgs e)
		{
			if (m_geometry != null)
				m_geometry.OnKeyDown(e);
		}

		//=============================================================================
		public List<Point> GetGripPoints()
		{
			if (m_geometry != null)
				return m_geometry.GetGripPoints();

			return new List<Point>();
		}

		//=============================================================================
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

		//=============================================================================
		public object GetPropertyValue(string strPropSysName)
		{
			if (m_geometry != null)
				return m_geometry.GetPropertyValue(strPropSysName);

			return null;
		}

		//=============================================================================
		public bool SetPropertyValue(string strPropSysName, object propValue)
		{
			if (m_geometry != null)
			{
				bool bRes = m_geometry.SetPropertyValue(strPropSysName, propValue);
				OnPropertyChanged();
				return bRes;
			}

			return false;
		}

		//=============================================================================
		public void OnPropertyChanged()
		{
			if (m_geometry != null)
				Draw(m_owner, null);
		}

		//=============================================================================
		public ICadGeometry Clone()
		{
			ICadGeometry cloneGeom = null;
			if (m_geometry != null)
				cloneGeom = m_geometry.Clone();

			return new GeometryWraper(m_owner, cloneGeom);
		}

		//=============================================================================
		// Implement this method to serialize data. The method is called 
		// on serialization.
		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("m_geometry", m_geometry);
		}

		//=============================================================================
		// The special constructor is used to deserialize values.
		public GeometryWraper(SerializationInfo info, StreamingContext context)
		{
			m_geometry = (ICadGeometry)info.GetValue("m_geometry", typeof(ICadGeometry));
		}

		//=============================================================================
		public ITooltip Tooltip
		{
			get
			{
				if (m_geometry != null)
					return m_geometry.Tooltip;

				return null;
			}
		}
	}
}
