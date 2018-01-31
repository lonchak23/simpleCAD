using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace simpleCAD.Geometry
{
	public class PropertyViewModel : BaseViewModel
	{
		public PropertyViewModel(ICadGeometry owner, string name)
		{
			m_owner = owner;
			m_Name = name;
		}

		//=============================================================================
		protected ICadGeometry m_owner = null;

		//=============================================================================
		private string m_Name = string.Empty;

		public string Name
		{
			get { return m_Name; }
		}

		//=============================================================================
		public virtual object Value
		{
			get
			{
				if (m_owner != null)
					return m_owner.GetPropertyValue(m_Name);

				return null;
			}
			set
			{
				if (m_owner != null)
					m_owner.SetPropertyValue(m_Name, value);

				NotifyPropertyChanged(() => Value);
			}
		}

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
		public GeometryType_Property(ICadGeometry owner, string typeName)
			: base(owner, "GeometryType")
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
		private int m_index = -1;

		protected GeometryGrip_Property(ICadGeometry owner, string strSysName, int index)
			: base(owner, strSysName)
		{
			m_index = index;
		}

		//=============================================================================
		protected Point GetPont()
		{
			if (m_owner != null && m_index >= 0)
			{
				List<Point> pnts = m_owner.GetGripPoints();
				if (pnts != null && m_index < pnts.Count)
					return pnts[m_index];
			}

			return new Point();
		}

		//=============================================================================
		protected bool SetPoint(Point pnt)
		{
			bool result = false;
			if (m_owner != null)
				result = m_owner.SetGripPoint(m_index, pnt);

			DrawingHost.RedrawGrips();

			return result;
		}
	}

	public class GeometryGripX_Property : GeometryGrip_Property
	{
		public GeometryGripX_Property(ICadGeometry owner, string strSysName, int index)
			: base(owner, strSysName, index) { }

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
		public GeometryGripY_Property(ICadGeometry owner, string strSysName, int index)
			: base(owner, strSysName, index) { }

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
}
