using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace simpleCAD.Geometry
{
	/// <summary>
	/// Base view model for geometry property.
	/// 
	/// </summary>
	public abstract class Property_ViewModel : BaseViewModel
	{
		public Property_ViewModel(ICadGeometry owner, string sysName)
		{
			m_owner = owner;
			m_strSystemName = sysName;

			this.PropertyChanged += OnPropertyChanged;
		}

		public Property_ViewModel(ICadGeometry owner, string sysName, string localName)
			: this(owner, sysName)
		{
			m_strLocalName = localName;
		}

		//=============================================================================
		protected ICadGeometry m_owner = null;

		//=============================================================================
		private string m_strSystemName = string.Empty;
		public string SystemName
		{
			get { return m_strSystemName; }
		}

		//=============================================================================
		private string m_strLocalName = string.Empty;
		public virtual string Name
		{
			get
			{
				if (!string.IsNullOrEmpty(m_strLocalName))
					return m_strLocalName;

				return SystemName;
			}
		}

		//=============================================================================
		public virtual object Value { get; set; }

		//=============================================================================
		protected bool m_IsReadOnly = false;
		public virtual bool IsReadOnly
		{
			get { return m_IsReadOnly; }
		}

		//=============================================================================
		public void Update_Value()
		{
			NotifyPropertyChanged(() => Value);
		}

		//=============================================================================
		private void OnPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "Value")
			{
				// notify DrawingHost for update all geometry
				DrawingHost.UpdatePlot();
			}
		}
	}

	public class GeometryProperty : Property_ViewModel
	{
		public GeometryProperty(ICadGeometry owner, string strPropSystemName)
			: base(owner, strPropSystemName) { }

		//=============================================================================
		public override object Value
		{
			get
			{
				if (m_owner != null)
					return m_owner.GetPropertyValue(SystemName);

				return null;
			}
			set
			{
				if (m_owner != null)
					m_owner.SetPropertyValue(SystemName, value);

				NotifyPropertyChanged(() => Value);
			}
		}
	}

	public class GeometryType_Property : Property_ViewModel
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

	//public abstract class GeometryGrip_Property : Property_ViewModel
	//{
	//	private int m_index = -1;
	//
	//	protected GeometryGrip_Property(ICadGeometry owner, string strSysName, int index)
	//		: base(owner, strSysName)
	//	{
	//		m_index = index;
	//	}
	//
	//	//=============================================================================
	//	protected Point GetPont()
	//	{
	//		if (m_owner != null && m_index >= 0)
	//		{
	//			List<Point> pnts = m_owner.GetGripPoints();
	//			if (pnts != null && m_index < pnts.Count)
	//				return pnts[m_index];
	//		}
	//
	//		return new Point();
	//	}
	//
	//	//=============================================================================
	//	protected bool SetPoint(Point pnt)
	//	{
	//		bool result = false;
	//		if (m_owner != null)
	//			result = m_owner.SetGripPoint(m_index, pnt);
	//
	//		DrawingHost.UpdatePlot();
	//
	//		return result;
	//	}
	//}
	//
	//public class GeometryGripX_Property : GeometryGrip_Property
	//{
	//	public GeometryGripX_Property(ICadGeometry owner, string strSysName, int index)
	//		: base(owner, strSysName, index) { }
	//
	//	//=============================================================================
	//	public override object Value
	//	{
	//		get { return GetPont().X; }
	//		set
	//		{
	//			try
	//			{
	//				double rVal = Convert.ToDouble(value);
	//				Point oldPnt = GetPont();
	//				SetPoint(new Point(rVal, oldPnt.Y));
	//			}
	//			catch { }
	//
	//			NotifyPropertyChanged(() => Value);
	//		}
	//	}
	//}
	//
	//public class GeometryGripY_Property : GeometryGrip_Property
	//{
	//	public GeometryGripY_Property(ICadGeometry owner, string strSysName, int index)
	//		: base(owner, strSysName, index) { }
	//
	//	//=============================================================================
	//	public override object Value
	//	{
	//		get { return GetPont().Y; }
	//		set
	//		{
	//			try
	//			{
	//				double rVal = Convert.ToDouble(value);
	//				Point oldPnt = GetPont();
	//				SetPoint(new Point(oldPnt.X, rVal));
	//			}
	//			catch { }
	//
	//			NotifyPropertyChanged(() => Value);
	//		}
	//	}
	//}
}
