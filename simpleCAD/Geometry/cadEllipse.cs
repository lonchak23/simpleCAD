using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace simpleCAD.Geometry
{
	[Serializable]
	public class cadEllipse : ICadGeometry, ISerializable
	{
		private Point m_pntCenter = new Point(0.0, 0.0);
		private double m_rRadiusX = 0.0;
		private double m_rRadiusY = 0.0;

		private Color m_Color = Colors.Black;
		private Color m_FillColor = Colors.Transparent;
		private double m_Thickness = 2.0;

		public static string PROP_PNT_CENTER_X = "Center point X";
		public static string PROP_PNT_CENTER_Y = "Center point Y";
		public static string PROP_RADIUS_X = "Radius X";
		public static string PROP_RADIUS_Y = "Radius Y";

		public cadEllipse() { }

		//=============================================================================
		private static string sDisplayName = "cadEllipse";
		public string DisplayName { get { return sDisplayName; } }

		//=============================================================================
		public DrawingVisual GetGeometryWrapper() { return null; }

		//=============================================================================
		private bool m_bCenter_IsSetted = false;
		private bool m_bRadiusX_IsSetted = false;
		private bool m_bRadiusY_IsSetted = false;
		public bool IsPlaced { get { return m_bCenter_IsSetted && m_bRadiusX_IsSetted && m_bRadiusY_IsSetted; } }

		//=============================================================================
		public void OnMouseLeftButtonClick(Point globalPoint)
		{
			if (!m_bCenter_IsSetted)
			{
				m_pntCenter = globalPoint;
				m_bCenter_IsSetted = true;
			}
			else if (!m_bRadiusX_IsSetted)
			{
				Vector vec = globalPoint - m_pntCenter;
				m_rRadiusX = vec.Length;
				// draw circle
				m_rRadiusY = m_rRadiusX;
				m_bRadiusX_IsSetted = true;
			}
			else if (!m_bRadiusY_IsSetted)
			{
				Vector vec = globalPoint - m_pntCenter;
				m_rRadiusY = vec.Length;
				m_bRadiusY_IsSetted = true;
			}
		}

		//=============================================================================
		public void OnMouseMove(Point globalPoint)
		{
			if (m_bCenter_IsSetted && (!m_bRadiusX_IsSetted || !m_bRadiusY_IsSetted))
			{
				Vector vec = globalPoint - m_pntCenter;
				if (!m_bRadiusX_IsSetted)
				{
					m_rRadiusX = vec.Length;
					// draw circle
					m_rRadiusY = m_rRadiusX;
				}
				else if (!m_bRadiusY_IsSetted)
					m_rRadiusY = vec.Length;
			}
		}

		//=============================================================================
		public void Draw(ICoordinateSystem cs, DrawingContext dc)
		{
			if (!m_bCenter_IsSetted)
				return;

			if (cs != null && dc != null)
			{
				Pen _pen = new Pen(new SolidColorBrush(m_Color), m_Thickness);
				//
				// If fill with transparent color then circle fill area will act in HitTest.
				// Fill with null brush will disable circle HitTest on click in fill area.
				Brush fillBrush = null;
				if (m_FillColor != Colors.Transparent)
					fillBrush = new SolidColorBrush(m_FillColor);
				dc.DrawEllipse(fillBrush, _pen, cs.GetLocalPoint(m_pntCenter), m_rRadiusX, m_rRadiusY);
			}
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

					m_Properties.Add(new GeometryProperty(this, PROP_PNT_CENTER_X));
					m_Properties.Add(new GeometryProperty(this, PROP_PNT_CENTER_Y));

					m_Properties.Add(new GeometryProperty(this, PROP_RADIUS_X));
					m_Properties.Add(new GeometryProperty(this, PROP_RADIUS_Y));

					m_Properties.Add(new GeometryProperty(this, Constants.SYSNAME_PROP_COLOR));
					m_Properties.Add(new GeometryProperty(this, Constants.SYSNAME_PROP_FILL_COLOR));
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

			if (PROP_PNT_CENTER_X == strPropSysName)
				return m_pntCenter.X;
			else if (PROP_PNT_CENTER_Y == strPropSysName)
				return m_pntCenter.Y;
			else if (PROP_RADIUS_X == strPropSysName)
				return m_rRadiusX;
			else if (PROP_RADIUS_Y == strPropSysName)
				return m_rRadiusY;
			else if (Constants.SYSNAME_PROP_COLOR == strPropSysName)
				return m_Color;
			else if (Constants.SYSNAME_PROP_FILL_COLOR == strPropSysName)
				return m_FillColor;
			else if (Constants.SYSNAME_PROP_THICKNESS == strPropSysName)
				return m_Thickness;

			return null;
		}

		//=============================================================================
		public bool SetPropertyValue(string strPropSysName, object propValue)
		{
			if (string.IsNullOrEmpty(strPropSysName))
				return false;

			if (PROP_PNT_CENTER_X == strPropSysName)
			{
				try
				{
					m_pntCenter.X = System.Convert.ToDouble(propValue);
					return true;
				}
				catch { }
			}
			else if (PROP_PNT_CENTER_Y == strPropSysName)
			{
				try
				{
					m_pntCenter.Y = System.Convert.ToDouble(propValue);
					return true;
				}
				catch { }
			}
			else if (PROP_RADIUS_X == strPropSysName)
			{
				try
				{
					m_rRadiusX = System.Convert.ToDouble(propValue);
					return true;
				}
				catch { }
			}
			else if (PROP_RADIUS_Y == strPropSysName)
			{
				try
				{
					m_rRadiusY = System.Convert.ToDouble(propValue);
					return true;
				}
				catch { }
			}
			else if (Constants.SYSNAME_PROP_COLOR == strPropSysName)
			{
				try
				{
					m_Color = (Color)ColorConverter.ConvertFromString(propValue as string);
					return true;
				}
				catch { }
			}
			else if (Constants.SYSNAME_PROP_FILL_COLOR == strPropSysName)
			{
				try
				{
					m_FillColor = (Color)ColorConverter.ConvertFromString(propValue as string);
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
		public List<Point> GetGripPoints()
		{
			List<Point> result = new List<Point>();

			result.Add(m_pntCenter);
			result.Add(m_pntCenter + new Vector(m_rRadiusX, 0.0));
			result.Add(m_pntCenter + new Vector(0.0, m_rRadiusY));

			return result;
		}

		//=============================================================================
		public bool SetGripPoint(int gripIndex, Point pnt)
		{
			if (!IsPlaced)
				return false;

			if (0 == gripIndex || 1 == gripIndex || 2 == gripIndex)
			{

				if (0 == gripIndex)
					m_pntCenter = pnt;
				else if (1 == gripIndex)
				{
					Vector vec = pnt - m_pntCenter;
					m_rRadiusX = vec.Length;
				}
				else if (2 == gripIndex)
				{
					Vector vec = pnt - m_pntCenter;
					m_rRadiusY = vec.Length;
				}

				return true;
			}

			return false;
		}

		//=============================================================================
		public ICadGeometry Clone()
		{
			return Utils.DeepClone<cadEllipse>(this);
		}

		//=============================================================================
		// Implement this method to serialize data. The method is called 
		// on serialization.
		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("m_pntCenter", m_pntCenter);
			info.AddValue("m_rRadiusX", m_rRadiusX);
			info.AddValue("m_rRadiusY", m_rRadiusY);

			info.AddValue("m_bCenter_IsSetted", m_bCenter_IsSetted);
			info.AddValue("m_bRadiusX_IsSetted", m_bRadiusX_IsSetted);
			info.AddValue("m_bRadiusY_IsSetted", m_bRadiusY_IsSetted);

			info.AddValue("m_Color", m_Color.ToString());
			info.AddValue("m_FillColor", m_FillColor.ToString());

			info.AddValue("m_Thickness", m_Thickness);
		}

		//=============================================================================
		// The special constructor is used to deserialize values.
		public cadEllipse(SerializationInfo info, StreamingContext context)
		{
			m_pntCenter = (Point)info.GetValue("m_pntCenter", typeof(Point));
			m_rRadiusX = (double)info.GetValue("m_rRadiusX", typeof(double));
			m_rRadiusY = (double)info.GetValue("m_rRadiusY", typeof(double));

			m_bCenter_IsSetted = (bool)info.GetValue("m_bCenter_IsSetted", typeof(bool));
			m_bRadiusX_IsSetted = (bool)info.GetValue("m_bRadiusX_IsSetted", typeof(bool));
			m_bRadiusY_IsSetted = (bool)info.GetValue("m_bRadiusY_IsSetted", typeof(bool));

			try
			{
				m_Color = (Color)ColorConverter.ConvertFromString((string)info.GetValue("m_Color", typeof(string)));
			}
			catch
			{
				m_Color = Colors.Black;
			}

			try
			{
				m_FillColor = (Color)ColorConverter.ConvertFromString((string)info.GetValue("m_FillColor", typeof(string)));
			}
			catch
			{
				m_FillColor = Colors.Transparent;
			}

			m_Thickness = (double)info.GetValue("m_Thickness", typeof(double));
		}
	}
}
