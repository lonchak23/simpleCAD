using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace simpleCAD.Geometry
{
	internal class cadLine : cadGeometry
	{
		private Point m_FirstPnt = new Point();
		private Point m_SecondPnt = new Point();

		private bool m_bFirstPnt_Setted = false;
		private bool m_bSecondPnt_Setted = false;

		private GeometryGripX_Property m_FirstPointX_Property = null;
		private GeometryGripY_Property m_FirstPointY_Property = null;
		private GeometryGripX_Property m_SecondPointX_Property = null;
		private GeometryGripY_Property m_SecondPointY_Property = null;

		public cadLine(DrawingHost owner)
			: base(owner)
		{
			m_FirstPointX_Property = new GeometryGripX_Property("Start point X", this, 0);
			m_FirstPointY_Property = new GeometryGripY_Property("Start point Y", this, 0);
			m_SecondPointX_Property = new GeometryGripX_Property("End point X", this, 1);
			m_SecondPointY_Property = new GeometryGripY_Property("End point Y", this, 1);
		}

		//=============================================================================
		public override bool IsInitialized()
		{
			return m_bFirstPnt_Setted && m_bSecondPnt_Setted;
		}

		//=============================================================================
		public override void SetPoint(Point pnt, bool bByClick)
		{
			if (!m_bFirstPnt_Setted)
			{
				m_FirstPnt = pnt;
				if(bByClick)
					m_bFirstPnt_Setted = true;
			}
			else if (!m_bSecondPnt_Setted)
			{
				m_SecondPnt = pnt;
				if (bByClick)
					m_bSecondPnt_Setted = true;
				Draw();
			}
		}

		//=============================================================================
		public override void Draw()
		{
			using (DrawingContext dc = this.RenderOpen())
			{
				Pen _pen = new Pen(Color, Thickness);
				dc.DrawLine(_pen, GetLocalPoint(m_FirstPnt), GetLocalPoint(m_SecondPnt));
			}
		}

		//=============================================================================
		public override List<Point> GetGripPoints()
		{
			List<Point> grips = new List<Point>();
			grips.Add(m_FirstPnt);
			grips.Add(m_SecondPnt);

			return grips;
		}

		//=============================================================================
		public override bool SetGripPoint(int gripIndex, Point pnt)
		{
			if (!IsInitialized())
				return false;

			if (0 == gripIndex || 1 == gripIndex)
			{

				if (0 == gripIndex)
				{
					m_FirstPnt = pnt;
					m_FirstPointX_Property.Update_Value();
					m_FirstPointY_Property.Update_Value();
				}
				else
				{
					m_SecondPnt = pnt;
					m_SecondPointX_Property.Update_Value();
					m_SecondPointY_Property.Update_Value();
				}

				Draw();
			}

			return false;
		}

		//=============================================================================
		private List<PropertyViewModel> m_Properties = null;
		public override List<PropertyViewModel> Properties
		{
			get
			{
				if (m_Properties == null)
				{
					m_Properties = new List<PropertyViewModel>();
					m_Properties.Add(new GeometryType_Property("Line"));

					List<Point> grips = GetGripPoints();
					if (grips.Count == 2)
					{
						m_Properties.Add(m_FirstPointX_Property);
						m_Properties.Add(m_FirstPointY_Property);

						m_Properties.Add(m_SecondPointX_Property);
						m_Properties.Add(m_SecondPointY_Property);
					}

					m_Properties.Add(m_ColorProperty);
					m_Properties.Add(m_ThicknessProperty);
				}
				return m_Properties;
			}
		}
	}
}
