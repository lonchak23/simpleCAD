using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace simpleCAD.Geometry
{
	public abstract class PropertyViewModel : BaseViewModel
	{
		protected PropertyViewModel(string name)
		{
			Name = name;
		}

		//=============================================================================
		private string m_Name = string.Empty;

		public string Name
		{
			get { return m_Name; }
			set
			{
				if (string.Compare(m_Name, value) != 0)
				{
					m_Name = value;
					NotifyPropertyChanged(() => Name);
				}
			}
		}

		//=============================================================================
		public abstract object Value { get; set; }

		//=============================================================================
		protected bool m_IsReadOnly = false;
		public bool IsReadOnly
		{
			get { return m_IsReadOnly; }
		}

		//=============================================================================
		public void Update_Value()
		{
			NotifyPropertyChanged(() => Value);
		}
	}

	public class GeometryType_Property : PropertyViewModel
	{
		public GeometryType_Property(string typeName)
			: base("Geometry")
		{
			m_IsReadOnly = true;
			m_GeometryType = typeName;
		}

		//=============================================================================
		private string m_GeometryType = string.Empty;

		public override object Value
		{
			get { return m_GeometryType; }
			set
			{
				NotifyPropertyChanged(() => Value);
			}
		}
	}

	public abstract class GeometryGrip_Property : PropertyViewModel
	{
		private cadGeometry m_geom = null;
		private int m_index = -1;

		protected GeometryGrip_Property(string name, cadGeometry geom, int index)
			: base(name)
		{
			m_geom = geom;
			m_index = index;
		}

		//=============================================================================
		protected Point GetPont()
		{
			if (m_geom != null && m_index >= 0)
			{
				List<Point> pnts = m_geom.GetGripPoints();
				if (pnts != null && m_index < pnts.Count)
					return pnts[m_index];
			}

			return new Point();
		}

		//=============================================================================
		protected bool SetPoint(Point pnt)
		{
			bool result = false;
			if (m_geom != null)
				result = m_geom.SetGripPoint(m_index, pnt);

			DrawingHost.RedrawGrips();

			return result;
		}
	}

	public class GeometryGripX_Property : GeometryGrip_Property
	{
		public GeometryGripX_Property(string name, cadGeometry geom, int index)
			: base(name, geom, index) { }

		//=============================================================================
		public override object Value
		{
			get { return GetPont().X; }
			set
			{
				try
				{
					double rVal = Convert.ToDouble(value);
					Point oldPnt = GetPont();
					SetPoint(new Point(rVal, oldPnt.Y));
				}
				catch { }

				NotifyPropertyChanged(() => Value);
			}
		}
	}

	public class GeometryGripY_Property : GeometryGrip_Property
	{
		public GeometryGripY_Property(string name, cadGeometry geom, int index)
			: base(name, geom, index) { }

		//=============================================================================
		public override object Value
		{
			get { return GetPont().Y; }
			set
			{
				try
				{
					double rVal = Convert.ToDouble(value);
					Point oldPnt = GetPont();
					SetPoint(new Point(oldPnt.X, rVal));
				}
				catch { }

				NotifyPropertyChanged(() => Value);
			}
		}
	}

	public class GeometryColor_Property : PropertyViewModel
	{
		private BrushConverter m_bc = new BrushConverter();
		private cadGeometry m_geom = null;

		public GeometryColor_Property(cadGeometry geom)
			: base("Color")
		{
			m_geom = geom;
		}

		//=============================================================================
		public override object Value
		{
			get
			{
				if (m_geom != null)
					return m_geom.Color;

				return Brushes.Black;
			}
			set
			{
				string strVal = string.Empty;
				if (value is string)
					strVal = value as string;

				if (m_geom != null)
				{
					try
					{
						Brush brushVal = m_bc.ConvertFromString(strVal) as Brush;
						if (brushVal != null)
							m_geom.Color = brushVal;
					}
					catch { }
				}

				NotifyPropertyChanged(() => Value);
			}
		}
	}

	public class GeometryThickness_Property : PropertyViewModel
	{
		private cadGeometry m_geom = null;

		public GeometryThickness_Property(cadGeometry geom)
			: base("Thickness")
		{
			m_geom = geom;
		}

		//=============================================================================
		public override object Value
		{
			get
			{
				if (m_geom != null)
					return m_geom.Thickness;

				return 0.0;
			}
			set
			{
				if (m_geom != null)
				{
					try
					{
						m_geom.Thickness = Convert.ToDouble(value);
					}
					catch { }
				}
			}
		}
	}
}
