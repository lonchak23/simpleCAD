using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace simpleCAD.Geometry
{
	[Serializable]
	public class cadArc : ICadGeometry, ISerializable
	{
		private Point m_FirstPnt = new Point();
		private Point m_SecondPnt = new Point();
		private Point m_ThirdPnt = new Point();
		private Point m_CenterPnt = new Point();

		private double m_rRadius = 0;

		// true - cant callculate circle from 3 points
		private bool m_bInvalidData = false;

		private bool m_bFirstPnt_IsSetted = false;
		private bool m_bSecondPnt_IsSetted = false;
		private bool m_bThirdPnt_IsSetted = false;


		private Color m_Color = Colors.Black;
		private double m_Thickness = 2.0;

		//
		public static string PROP_FIRST_PNT_X = "Start point X";
		public static string PROP_FIRST_PNT_Y = "Start point Y";
		//
		public static string PROP_SECONG_PNT_X = "Second point X";
		public static string PROP_SECONG_PNT_Y = "Second point Y";
		//
		public static string PROP_THIRD_PNT_X = "Third point X";
		public static string PROP_THIRD_PNT_Y = "Third point Y";
		//
		public static string PROP_CENTER_PNT_X = "Center point X";
		public static string PROP_CENTER_PNT_Y = "Center point Y";

		public cadArc() { }

		//=============================================================================
		private double CalculateRadius(Point firstPnt, Point secondPnt, Point thirdPnt, ref Point centerPnt)
		{
			m_bInvalidData = false;

			double x1 = firstPnt.X;
			double y1 = firstPnt.Y;

			double x2 = secondPnt.X;
			double y2 = secondPnt.Y;

			double x3 = thirdPnt.X;
			double y3 = thirdPnt.Y;

			// 
			// http://paulbourke.net/geometry/circlesphere/

			if((x2-x1 == 0) || (x3-x2 == 0))
			{
				m_bInvalidData = true;
				return -1;
			}

			double ma = (y2 - y1) / (x2 - x1);
			double mb = (y3 - y2) / (x3 - x2);

			if(mb - ma == 0)
			{
				m_bInvalidData = true;
				return -1;
			}

			centerPnt.X = (ma * mb * (y1 - y3) + mb * (x1 + x2) - ma * (x2 + x3)) / (2*(mb - ma));
			centerPnt.Y = -(1 / ma) * (centerPnt.X - (x1 + x2) / 2) + (y1 + y2) / 2;

			return (firstPnt - centerPnt).Length;
		}

		//=============================================================================
		public static string sDisplayName = "cadArc";
		public string DisplayName { get { return sDisplayName; } }

		//=============================================================================
		private ImageSource m_Image = null;
		public ImageSource GeomImage
		{
			get
			{
				if (m_Image == null)
				{
					BitmapImage logo = new BitmapImage();
					logo.BeginInit();
					logo.UriSource = new Uri("pack://application:,,,/simpleCAD;component/Images/img_arc.png");
					logo.EndInit();

					m_Image = logo;
				}

				return m_Image;
			}
		}

		//=============================================================================
		public bool IsPlaced
		{
			get
			{
				return m_bFirstPnt_IsSetted && m_bSecondPnt_IsSetted && m_bThirdPnt_IsSetted;
			}
		}

		//=============================================================================
		public void OnMouseLeftButtonClick(Point globalPoint)
		{
			if (!m_bFirstPnt_IsSetted)
			{
				m_FirstPnt = globalPoint;
				m_bFirstPnt_IsSetted = true;
			}
			else if (!m_bSecondPnt_IsSetted)
			{
				m_SecondPnt = globalPoint;
				m_bSecondPnt_IsSetted = true;
			}
			else if(!m_bThirdPnt_IsSetted)
			{
				m_ThirdPnt = globalPoint;
				m_rRadius = CalculateRadius(m_FirstPnt, m_SecondPnt, m_ThirdPnt, ref m_CenterPnt);
				m_bThirdPnt_IsSetted = true;
			}
		}

		//=============================================================================
		public void OnMouseMove(Point globalPoint)
		{
			if (!m_bFirstPnt_IsSetted)
				m_FirstPnt = globalPoint;
			else if (!m_bSecondPnt_IsSetted)
				m_SecondPnt = globalPoint;
			else if (!m_bThirdPnt_IsSetted)
			{
				m_ThirdPnt = globalPoint;
				m_rRadius = CalculateRadius(m_FirstPnt, m_SecondPnt, m_ThirdPnt, ref m_CenterPnt);
			}
		}

		//=============================================================================
		public void OnKeyDown(System.Windows.Input.KeyEventArgs e) { }

		//=============================================================================
		public void Draw(ICoordinateSystem cs, DrawingContext dc)
		{
			if (cs != null && dc != null && m_bFirstPnt_IsSetted)
			{
				Pen _pen = new Pen(new SolidColorBrush(m_Color), m_Thickness);

				PathFigure pf = new PathFigure();
				if (!m_bInvalidData && m_bSecondPnt_IsSetted)
				{
					double scaledRadius = m_rRadius * cs.Get_Scale();

					double rFirstAngle = Math.Atan2(m_FirstPnt.Y - m_CenterPnt.Y, m_FirstPnt.X - m_CenterPnt.X);
					// Atan2 return result in [-pi, pi]. Convert it to [0, 2pi].
					if (rFirstAngle < 0)
						rFirstAngle += 2*Math.PI;
					double rSecondAngle = Math.Atan2(m_SecondPnt.Y - m_CenterPnt.Y, m_SecondPnt.X - m_CenterPnt.X);
					if (rSecondAngle < 0)
						rSecondAngle += 2 * Math.PI;
					double rThirdAngle = Math.Atan2(m_ThirdPnt.Y - m_CenterPnt.Y, m_ThirdPnt.X - m_CenterPnt.X);
					if (rThirdAngle < 0)
						rThirdAngle += 2 * Math.PI;

					Point startPnt = m_FirstPnt;
					Point endPnt = new Point(0, 0);
					SweepDirection sd = SweepDirection.Clockwise;
					bool bIsLargeArc = false;
					if (rFirstAngle > rSecondAngle)
					{
						if(rFirstAngle > rThirdAngle)
						{
							if(rSecondAngle > rThirdAngle)
							{
								endPnt = m_ThirdPnt;
								sd = SweepDirection.Clockwise;
								//sd = SweepDirection.Counterclockwise;
								bIsLargeArc = (rFirstAngle - rThirdAngle) >= Math.PI;
							}
							else
							{
								endPnt = m_ThirdPnt;
								sd = SweepDirection.Counterclockwise;
								//sd = SweepDirection.Clockwise;
								bIsLargeArc = (2*Math.PI - (rFirstAngle - rThirdAngle)) >= Math.PI;
							}
						}
						else
						{
							endPnt = m_ThirdPnt;
							sd = SweepDirection.Clockwise;
							//sd = SweepDirection.Counterclockwise;

							double rDiff = rFirstAngle - rThirdAngle;
							rDiff += 2*Math.PI;
							bIsLargeArc = rDiff >= Math.PI;
						}
					}
					else
					{
						if(rFirstAngle > rThirdAngle)
						{
							endPnt = m_ThirdPnt;
							sd = SweepDirection.Counterclockwise;
							//sd = SweepDirection.Clockwise;

							double rDiff = rFirstAngle - rThirdAngle;
							rDiff -= 2 * Math.PI;
							bIsLargeArc = Math.Abs(rDiff) >= Math.PI;
						}
						else
						{
							if(rSecondAngle > rThirdAngle)
							{
								endPnt = m_ThirdPnt;
								sd = SweepDirection.Clockwise;
								//sd = SweepDirection.Counterclockwise;

								double rDiff = rFirstAngle - rThirdAngle;
								rDiff += 2 * Math.PI;
								bIsLargeArc = Math.Abs(rDiff) >= Math.PI;
							}
							else
							{
								endPnt = m_ThirdPnt;
								sd = SweepDirection.Counterclockwise;
								//sd = SweepDirection.Clockwise;

								double rDiff = rFirstAngle - rThirdAngle;
								//if (true || rDiff > 0)
								//	rDiff += 2 * Math.PI;
								bIsLargeArc = Math.Abs(rDiff) >= Math.PI;
							}
						}
					}
					pf.StartPoint = cs.GetLocalPoint(startPnt);
					// draw arc
					pf.Segments.Add(new ArcSegment(cs.GetLocalPoint(endPnt), new Size(scaledRadius, scaledRadius), 0, bIsLargeArc, sd, true));

					// DEBUG INFO
					if (false)
					{
						string str = string.Format("{0:F2}, {1:F2}, {2:F2}", rFirstAngle, rSecondAngle, rThirdAngle);
						Debug.WriteLine(str);

						Vector vec = new Vector(4.0, 4.0);
						Point pnt = cs.GetLocalPoint(m_SecondPnt);
						Point pnt1 = pnt - vec;
						Point pnt2 = pnt + vec;
						Rect rect = new Rect(pnt1, pnt2);
						dc.DrawRectangle(Brushes.Aqua, _pen, rect);

						pnt = cs.GetLocalPoint(m_CenterPnt);
						pnt1 = pnt - vec;
						pnt2 = pnt + vec;
						rect = new Rect(pnt1, pnt2);
						dc.DrawRectangle(Brushes.Red, _pen, rect);

						dc.DrawEllipse(null, new Pen(new SolidColorBrush(Colors.Red), 3), cs.GetLocalPoint(m_CenterPnt), scaledRadius, scaledRadius);
					}
				}
				else
				{
					pf.StartPoint = cs.GetLocalPoint(m_FirstPnt);
					// draw line
					pf.Segments.Add(new LineSegment(cs.GetLocalPoint(m_SecondPnt), true));
				}

				PathGeometry pg = new PathGeometry(new[] { pf });

				dc.DrawGeometry(null, _pen, pg);
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
			grips.Add(m_ThirdPnt);
			grips.Add(m_CenterPnt);

			return grips;
		}

		//=============================================================================
		public bool SetGripPoint(int gripIndex, Point pnt)
		{
			if (!IsPlaced)
				return false;

			if (gripIndex >= 0 && gripIndex < 4)
			{
				bool bRecalc = false;
				if (0 == gripIndex)
				{
					m_FirstPnt = pnt;
					bRecalc = true;
				}
				else if (1 == gripIndex)
				{
					m_SecondPnt = pnt;
					bRecalc = true;
				}
				else if (2 == gripIndex)
				{
					m_ThirdPnt = pnt;
					bRecalc = true;
				}
				else if (3 == gripIndex)
				{
					Point oldVal = m_CenterPnt;
					m_CenterPnt = pnt;

					Vector vecOffset = m_CenterPnt - oldVal;
					m_FirstPnt += vecOffset;
					m_SecondPnt += vecOffset;
					m_ThirdPnt += vecOffset;
				}

				if(bRecalc)
					m_rRadius = CalculateRadius(m_FirstPnt, m_SecondPnt, m_ThirdPnt, ref m_CenterPnt);

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

					m_Properties.Add(new GeometryProperty(this, PROP_FIRST_PNT_X));
					m_Properties.Add(new GeometryProperty(this, PROP_FIRST_PNT_Y));

					m_Properties.Add(new GeometryProperty(this, PROP_SECONG_PNT_X));
					m_Properties.Add(new GeometryProperty(this, PROP_SECONG_PNT_Y));

					m_Properties.Add(new GeometryProperty(this, PROP_THIRD_PNT_X));
					m_Properties.Add(new GeometryProperty(this, PROP_THIRD_PNT_Y));

					m_Properties.Add(new GeometryProperty(this, PROP_CENTER_PNT_X));
					m_Properties.Add(new GeometryProperty(this, PROP_CENTER_PNT_Y));

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
			//
			else if (PROP_FIRST_PNT_X == strPropSysName)
				return m_FirstPnt.X;
			else if (PROP_FIRST_PNT_Y == strPropSysName)
				return m_FirstPnt.Y;
			//
			else if (PROP_SECONG_PNT_X == strPropSysName)
				return m_SecondPnt.X;
			else if (PROP_SECONG_PNT_Y == strPropSysName)
				return m_SecondPnt.Y;
			//
			else if (PROP_THIRD_PNT_X == strPropSysName)
				return m_ThirdPnt.X;
			else if (PROP_THIRD_PNT_Y == strPropSysName)
				return m_ThirdPnt.Y;
			//
			else if (PROP_CENTER_PNT_X == strPropSysName)
				return m_CenterPnt.X;
			else if (PROP_CENTER_PNT_Y == strPropSysName)
				return m_CenterPnt.Y;

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
			//
			else if (PROP_FIRST_PNT_X == strPropSysName)
			{
				try
				{
					m_FirstPnt.X = System.Convert.ToDouble(propValue);

					m_rRadius = CalculateRadius(m_FirstPnt, m_SecondPnt, m_ThirdPnt, ref m_CenterPnt);

					return true;
				}
				catch { }
			}
			else if (PROP_FIRST_PNT_Y == strPropSysName)
			{
				try
				{
					m_FirstPnt.Y = System.Convert.ToDouble(propValue);

					m_rRadius = CalculateRadius(m_FirstPnt, m_SecondPnt, m_ThirdPnt, ref m_CenterPnt);

					return true;
				}
				catch { }
			}
			//
			else if (PROP_SECONG_PNT_X == strPropSysName)
			{
				try
				{
					m_SecondPnt.X = System.Convert.ToDouble(propValue);

					m_rRadius = CalculateRadius(m_FirstPnt, m_SecondPnt, m_ThirdPnt, ref m_CenterPnt);

					return true;
				}
				catch { }
			}
			else if (PROP_SECONG_PNT_Y == strPropSysName)
			{
				try
				{
					m_SecondPnt.Y = System.Convert.ToDouble(propValue);

					m_rRadius = CalculateRadius(m_FirstPnt, m_SecondPnt, m_ThirdPnt, ref m_CenterPnt);

					return true;
				}
				catch { }
			}
			//
			else if (PROP_THIRD_PNT_X == strPropSysName)
			{
				try
				{
					m_ThirdPnt.X = System.Convert.ToDouble(propValue);

					m_rRadius = CalculateRadius(m_FirstPnt, m_SecondPnt, m_ThirdPnt, ref m_CenterPnt);

					return true;
				}
				catch { }
			}
			else if (PROP_THIRD_PNT_Y == strPropSysName)
			{
				try
				{
					m_ThirdPnt.Y = System.Convert.ToDouble(propValue);

					m_rRadius = CalculateRadius(m_FirstPnt, m_SecondPnt, m_ThirdPnt, ref m_CenterPnt);

					return true;
				}
				catch { }
			}
			//
			else if (PROP_CENTER_PNT_X == strPropSysName)
			{
				try
				{
					double oldX = m_CenterPnt.X;
					m_CenterPnt.X = System.Convert.ToDouble(propValue);

					double offset_X = m_CenterPnt.X - oldX;
					m_FirstPnt.X += offset_X;
					m_SecondPnt.X += offset_X;
					m_ThirdPnt.X += offset_X;

					return true;
				}
				catch { }
			}
			else if (PROP_CENTER_PNT_Y == strPropSysName)
			{
				try
				{
					double oldY = m_CenterPnt.Y;
					m_CenterPnt.Y = System.Convert.ToDouble(propValue);

					double offset_Y = m_CenterPnt.Y - oldY;
					m_FirstPnt.Y += offset_Y;
					m_SecondPnt.Y += offset_Y;
					m_ThirdPnt.Y += offset_Y;

					return true;
				}
				catch { }
			}

			return false;
		}

		//=============================================================================
		public ITooltip Tooltip
		{
			get
			{
				if (!m_bFirstPnt_IsSetted)
					return new Tooltip(sDisplayName, GeomImage, "Input first point", TooltipType.eTooltip);
				else if (!m_bSecondPnt_IsSetted)
					return new Tooltip(sDisplayName, GeomImage, "Input second point", TooltipType.eTooltip);
				else if (!m_bThirdPnt_IsSetted)
					return new Tooltip(sDisplayName, GeomImage, "Input third point", TooltipType.eTooltip);

				return null;
			}
		}

		//=============================================================================
		public ICadGeometry Clone()
		{
			return Utils.DeepClone<cadArc>(this);
		}

		//=============================================================================
		// Implement this method to serialize data. The method is called 
		// on serialization.
		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("m_FirstPnt", m_FirstPnt);
			info.AddValue("m_SecondPnt", m_SecondPnt);
			info.AddValue("m_ThirdPnt", m_ThirdPnt);
			info.AddValue("m_CenterPnt", m_CenterPnt);
			info.AddValue("m_rRadius", m_rRadius);

			info.AddValue("m_bInvalidData", m_bInvalidData);

			info.AddValue("m_bFirstPnt_IsSetted", m_bFirstPnt_IsSetted);
			info.AddValue("m_bSecondPnt_IsSetted", m_bSecondPnt_IsSetted);
			info.AddValue("m_bThirdPnt_IsSetted", m_bThirdPnt_IsSetted);

			info.AddValue("m_Color", m_Color.ToString());

			info.AddValue("m_Thickness", m_Thickness);
		}

		//=============================================================================
		// The special constructor is used to deserialize values.
		public cadArc(SerializationInfo info, StreamingContext context)
		{
			m_FirstPnt = (Point)info.GetValue("m_FirstPnt", typeof(Point));
			m_SecondPnt = (Point)info.GetValue("m_SecondPnt", typeof(Point));
			m_ThirdPnt = (Point)info.GetValue("m_ThirdPnt", typeof(Point));
			m_CenterPnt = (Point)info.GetValue("m_CenterPnt", typeof(Point));
			m_rRadius = (double)info.GetValue("m_rRadius", typeof(double));

			m_bInvalidData = (bool)info.GetValue("m_bInvalidData", typeof(bool));

			m_bFirstPnt_IsSetted = (bool)info.GetValue("m_bFirstPnt_IsSetted", typeof(bool));
			m_bSecondPnt_IsSetted = (bool)info.GetValue("m_bSecondPnt_IsSetted", typeof(bool));
			m_bThirdPnt_IsSetted = (bool)info.GetValue("m_bThirdPnt_IsSetted", typeof(bool));

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
