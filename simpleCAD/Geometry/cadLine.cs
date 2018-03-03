using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Media;

namespace simpleCAD.Geometry
{
	[Serializable]
	public class cadLine : ICadGeometry, ISerializable
	{
		private Point m_FirstPnt = new Point();
		private Point m_SecondPnt = new Point();

		private bool m_bFirstPnt_Setted = false;
		private bool m_bSecondPnt_Setted = false;

		private Color m_Color = Colors.Black;
		private double m_Thickness = 2.0;

		public static string PROP_START_PNT_X = "Start point X";
		public static string PROP_START_PNT_Y = "Start point Y";
		public static string PROP_END_PNT_X = "End point X";
		public static string PROP_END_PNT_Y = "End point Y";

		public cadLine() { }

		//=============================================================================
		public static string sDisplayName = "cadLine";
		public string DisplayName { get { return sDisplayName; } }

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
				Pen _pen = new Pen(new SolidColorBrush(m_Color), m_Thickness);
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

					m_Properties.Add(new GeometryProperty(this, PROP_START_PNT_X));
					m_Properties.Add(new GeometryProperty(this, PROP_START_PNT_Y));

					m_Properties.Add(new GeometryProperty(this, PROP_END_PNT_X));
					m_Properties.Add(new GeometryProperty(this, PROP_END_PNT_Y));

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
			else if (PROP_START_PNT_X == strPropSysName)
				return m_FirstPnt.X;
			else if (PROP_START_PNT_Y == strPropSysName)
				return m_FirstPnt.Y;
			else if (PROP_END_PNT_X == strPropSysName)
				return m_SecondPnt.X;
			else if (PROP_END_PNT_Y == strPropSysName)
				return m_SecondPnt.Y;

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
			else if(PROP_START_PNT_X == strPropSysName)
			{
				try
				{
					m_FirstPnt.X = System.Convert.ToDouble(propValue);
					return true;
				}
				catch { }
			}
			else if (PROP_START_PNT_Y == strPropSysName)
			{
				try
				{
					m_FirstPnt.Y = System.Convert.ToDouble(propValue);
					return true;
				}
				catch { }
			}
			else if (PROP_END_PNT_X == strPropSysName)
			{
				try
				{
					m_SecondPnt.X = System.Convert.ToDouble(propValue);
					return true;
				}
				catch { }
			}
			else if (PROP_END_PNT_Y == strPropSysName)
			{
				try
				{
					m_SecondPnt.Y = System.Convert.ToDouble(propValue);
					return true;
				}
				catch { }
			}

			return false;
		}

		//=============================================================================
		public ICadGeometry Clone()
		{
			return Utils.DeepClone<cadLine>(this);
		}

		//=============================================================================
		// Implement this method to serialize data. The method is called 
		// on serialization.
		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("m_FirstPnt", m_FirstPnt);
			info.AddValue("m_SecondPnt", m_SecondPnt);

			info.AddValue("m_bFirstPnt_Setted", m_bFirstPnt_Setted);
			info.AddValue("m_bSecondPnt_Setted", m_bSecondPnt_Setted);

			info.AddValue("m_Color", m_Color.ToString());

			info.AddValue("m_Thickness", m_Thickness);
		}

		//=============================================================================
		// The special constructor is used to deserialize values.
		public cadLine(SerializationInfo info, StreamingContext context)
		{
			m_FirstPnt = (Point)info.GetValue("m_FirstPnt", typeof(Point));
			m_SecondPnt = (Point)info.GetValue("m_SecondPnt", typeof(Point));

			m_bFirstPnt_Setted = (bool)info.GetValue("m_bFirstPnt_Setted", typeof(bool));
			m_bSecondPnt_Setted = (bool)info.GetValue("m_bSecondPnt_Setted", typeof(bool));

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
