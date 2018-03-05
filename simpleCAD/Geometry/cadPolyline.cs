
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace simpleCAD.Geometry
{
	[Serializable]
	public class cadPolyline : ICadGeometry, ISerializable
	{
		private List<Point> m_points = new List<Point>();

		private bool m_bIsTempPointSetted = false;
		private Point m_TempPoint = new Point();

		private bool m_IsPlaced = false;

		private Color m_Color = Colors.Black;
		private double m_Thickness = 2.0;

		public cadPolyline() { }

		//=============================================================================
		public static string sDisplayName = "cadPolyline";
		public string DisplayName { get { return sDisplayName; } }

		//=============================================================================
		public bool IsPlaced { get { return m_IsPlaced; } }

		//=============================================================================
		public void OnMouseLeftButtonClick(Point globalPoint)
		{
			if(!m_IsPlaced)
				m_points.Add(globalPoint);
		}

		//=============================================================================
		public void OnMouseMove(Point globalPoint)
		{
			if(!m_IsPlaced && m_points.Count > 0)
			{
				m_bIsTempPointSetted = true;
				m_TempPoint = globalPoint;
			}
		}

		//=============================================================================
		public void OnKeyDown(System.Windows.Input.KeyEventArgs e)
		{
			if (!m_IsPlaced && Key.Enter == e.Key)
				m_IsPlaced = true;
		}

		//=============================================================================
		public void Draw(ICoordinateSystem cs, DrawingContext dc)
		{
			if (cs != null && dc != null && m_points.Count > 0)
			{
				Pen _pen = new Pen(new SolidColorBrush(m_Color), m_Thickness);

				for (int i = 1; i < m_points.Count; ++i)
					dc.DrawLine(_pen, cs.GetLocalPoint(m_points[i-1]), cs.GetLocalPoint(m_points[i]));

				if(!m_IsPlaced && m_bIsTempPointSetted)
					dc.DrawLine(_pen, cs.GetLocalPoint(m_points[m_points.Count-1]), cs.GetLocalPoint(m_TempPoint));
			}
		}

		//=============================================================================
		public DrawingVisual GetGeometryWrapper() { return null; }

		//=============================================================================
		public List<Point> GetGripPoints()
		{
			List<Point> grips = new List<Point>();

			for (int i = 0; i < m_points.Count; ++i)
				grips.Add(m_points[i]);

			return grips;
		}

		//=============================================================================
		public bool SetGripPoint(int gripIndex, Point pnt)
		{
			if(gripIndex >=0 && gripIndex < m_points.Count)
			{
				m_points[gripIndex] = pnt;
				return true;
			}

			return false;
		}

		//=============================================================================
		private List<Property_ViewModel> m_Properties = null;
		public List<Property_ViewModel> Properties
		{
			get
			{
				if (m_Properties == null)
				{
					m_Properties = new List<Property_ViewModel>();
					m_Properties.Add(new GeometryType_Property(this, sDisplayName));

					m_Properties.Add(new GeometryProperty(this, Constants.SYSNAME_PROP_COLOR));
					m_Properties.Add(new GeometryProperty(this, Constants.SYSNAME_PROP_THICKNESS));
				}
				return m_Properties;
			}
		}

		//=============================================================================
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

		//=============================================================================
		public bool SetPropertyValue(string strPropSysName, object propValue)
		{
			if (string.IsNullOrEmpty(strPropSysName))
				return false;

			if (Constants.SYSNAME_PROP_COLOR == strPropSysName)
			{
				try
				{
					m_Color = (Color)ColorConverter.ConvertFromString(propValue as string);
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

		//=============================================================================
		public ICadGeometry Clone()
		{
			return Utils.DeepClone<cadPolyline>(this);
		}

		//=============================================================================
		// Implement this method to serialize data. The method is called 
		// on serialization.
		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("m_points", m_points);

			info.AddValue("m_IsPlaced", m_IsPlaced);

			info.AddValue("m_Color", m_Color.ToString());

			info.AddValue("m_Thickness", m_Thickness);
		}

		//=============================================================================
		// The special constructor is used to deserialize values.
		public cadPolyline(SerializationInfo info, StreamingContext context)
		{
			m_points = (List<Point>)info.GetValue("m_points", typeof(List<Point>));

			m_IsPlaced = (bool)info.GetValue("m_IsPlaced", typeof(bool));

			try
			{
				m_Color = (Color)ColorConverter.ConvertFromString((string)info.GetValue("m_Color", typeof(string)));
			}
			catch
			{
				m_Color = Colors.Black;
			}

			m_Thickness = (double)info.GetValue("m_Thickness", typeof(double));
		}
	}
}