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
	public class cadCircle : ICadGeometry, ISerializable
	{
		private Point m_pntCenter = new Point(0.0, 0.0);
		private double m_rRadius = 0.0;

		private Color m_Color = Colors.Black;
		private Color m_FillColor = Colors.Transparent;
		private double m_Thickness = 2.0;

		public static string PROP_PNT_CENTER_X = "Center point X";
		public static string PROP_PNT_CENTER_Y = "Center point Y";
		public static string PROP_RADIUS = "Radius";

		public cadCircle() { }

		//=============================================================================
		private static string sDisplayName = "cadCircle";
		public string DisplayName { get { return sDisplayName; } }

		//=============================================================================
		public DrawingVisual GetGeometryWrapper() { return null; }

		//=============================================================================
		private bool m_bCenter_IsSetted = false;
		private bool m_bRadius_IsSetted = false;
		public bool IsPlaced { get { return m_bCenter_IsSetted && m_bRadius_IsSetted; } }

		//=============================================================================
		public void OnMouseLeftButtonClick(Point globalPoint)
		{
			if(!m_bCenter_IsSetted)
			{
				m_pntCenter = globalPoint;
				m_bCenter_IsSetted = true;
			}
			else if(!m_bRadius_IsSetted)
			{
				Vector vec = globalPoint - m_pntCenter;
				m_rRadius = vec.Length;
				m_bRadius_IsSetted = true;
			}
		}

		//=============================================================================
		public void OnMouseMove(Point globalPoint)
		{
			if(m_bCenter_IsSetted && !m_bRadius_IsSetted)
			{
				Vector vec = globalPoint - m_pntCenter;
				m_rRadius = vec.Length;
			}
		}

		//=============================================================================
		public void OnKeyDown(System.Windows.Input.KeyEventArgs e) { }

		//=============================================================================
		public void Draw(ICoordinateSystem cs, DrawingContext dc)
		{
			if (!m_bCenter_IsSetted)
				return;

			if(cs != null && dc != null)
			{
				Pen _pen = new Pen(new SolidColorBrush(m_Color), m_Thickness);
				//
				// If fill with transparent color then circle fill area will act in HitTest.
				// Fill with null brush will disable circle HitTest on click in fill area.
				Brush fillBrush = null;
				if (m_FillColor != Colors.Transparent)
					fillBrush = new SolidColorBrush(m_FillColor);

				double rScale = cs.Get_Scale();

				dc.DrawEllipse(fillBrush, _pen, cs.GetLocalPoint(m_pntCenter), m_rRadius * rScale, m_rRadius * rScale);
			}
		}

		//=============================================================================
		private List<Property_ViewModel> m_Properties = null;
		public List<Property_ViewModel> Properties
		{
			get
			{
				if(m_Properties == null)
				{
					m_Properties = new List<Property_ViewModel>();

					m_Properties.Add(new GeometryType_Property(this, sDisplayName));

					m_Properties.Add(new GeometryProperty(this, PROP_PNT_CENTER_X));
					m_Properties.Add(new GeometryProperty(this, PROP_PNT_CENTER_Y));

					m_Properties.Add(new GeometryProperty(this, PROP_RADIUS));

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
			else if (PROP_RADIUS == strPropSysName)
				return m_rRadius;
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

			if(PROP_PNT_CENTER_X == strPropSysName)
			{
				try
				{
					m_pntCenter.X = System.Convert.ToDouble(propValue);
					return true;
				}
				catch { }
			}
			else if(PROP_PNT_CENTER_Y == strPropSysName)
			{
				try
				{
					m_pntCenter.Y = System.Convert.ToDouble(propValue);
					return true;
				}
				catch { }
			}
			else if(PROP_RADIUS == strPropSysName)
			{
				try
				{
					m_rRadius = System.Convert.ToDouble(propValue);
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
			result.Add(m_pntCenter + new Vector(m_rRadius, 0.0));

			return result;
		}

		//=============================================================================
		public bool SetGripPoint(int gripIndex, Point pnt)
		{
			if (!IsPlaced)
				return false;

			if (0 == gripIndex || 1 == gripIndex)
			{

				if (0 == gripIndex)
					m_pntCenter = pnt;
				else
				{
					Vector vec = pnt - m_pntCenter;
					m_rRadius = vec.Length;
				}

				return true;
			}

			return false;
		}

		//=============================================================================
		public ICadGeometry Clone()
		{
			return Utils.DeepClone<cadCircle>(this);
		}

		//=============================================================================
		// Implement this method to serialize data. The method is called 
		// on serialization.
		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("m_pntCenter", m_pntCenter);
			info.AddValue("m_rRadius", m_rRadius);

			info.AddValue("m_bCenter_IsSetted", m_bCenter_IsSetted);
			info.AddValue("m_bRadius_IsSetted", m_bRadius_IsSetted);

			info.AddValue("m_Color", m_Color.ToString());
			info.AddValue("m_FillColor", m_FillColor.ToString());

			info.AddValue("m_Thickness", m_Thickness);
		}

		//=============================================================================
		// The special constructor is used to deserialize values.
		public cadCircle(SerializationInfo info, StreamingContext context)
		{
			m_pntCenter = (Point)info.GetValue("m_pntCenter", typeof(Point));
			m_rRadius = (double)info.GetValue("m_rRadius", typeof(double));

			m_bCenter_IsSetted = (bool)info.GetValue("m_bCenter_IsSetted", typeof(bool));
			m_bRadius_IsSetted = (bool)info.GetValue("m_bRadius_IsSetted", typeof(bool));

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
