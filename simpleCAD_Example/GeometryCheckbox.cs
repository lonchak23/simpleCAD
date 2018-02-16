using simpleCAD;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace simpleCAD_Example
{
	/// <summary>
	/// CheckBox with TargetGeometry dependency property, which one should be binded to DrawingHost.GeometryToCreate dependency property.
	/// If user clicks in this checkbox, then ICagGeometry will be get from DataContext and set to TargetProperty.
	/// 
	/// Why do you use so difficult way instead using default checkbox and bind his IsChecked property to DrawingHost.GeometryToCreate with converter?
	/// Converter with multibinding works perfect in one way - from source(IsChecked) to target(GeometryToCreate), i can pass ICadGeometry from data context to converter
	/// and return it from Convert() method to Target property. But when Target(GeometryToCreate) was changed - for example another Checkbox with another underlying ICadGeometry was clicked, 
	/// i cant get current Checkbox underlying ICadGeometry from anywhere in converter ConvertBack() method. In ConvertBack() I need current CheckBox.ICadGeometry 
	/// for comparsion with DrawingHost.GeometryToCreate and return bool value - Is current checkbox ICadGeometry setted in DrawingHost.GeometryToCreate.
	/// </summary>
	internal class GeometryCheckbox :  CheckBox
	{
		//=============================================================================
		public static readonly DependencyProperty TargetGeometryProperty;
		public ICadGeometry TargetGeometry
		{
			get { return (ICadGeometry)GetValue(GeometryCheckbox.TargetGeometryProperty); }
			set { SetValue(GeometryCheckbox.TargetGeometryProperty, value); }
		}
		private static void On_TargetGeometry_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			GeometryCheckbox gc = d as GeometryCheckbox;
			if (gc != null)
				gc.OnTargetGeometryChanged(e.NewValue as ICadGeometry);
		}

		//=============================================================================
		static GeometryCheckbox()
		{
			GeometryCheckbox.TargetGeometryProperty = DependencyProperty.Register(
				"TargetGeometry",
				typeof(ICadGeometry),
				typeof(GeometryCheckbox),
				new FrameworkPropertyMetadata(null, On_TargetGeometry_Changed));
		}

		//=============================================================================
		public void OnTargetGeometryChanged(ICadGeometry newTargetGeometry)
		{
			if (_GetGeometry() != newTargetGeometry)
				this.IsChecked = false;
			else
				this.IsChecked = true;
		}

		//=============================================================================
		private ICadGeometry _GetGeometry()
		{
			return this.DataContext as ICadGeometry;
		}

		//=============================================================================
		protected override void OnClick()
		{
			base.OnClick();

			ICadGeometry newTargetGeometry = null;
			if ((bool)IsChecked)
				newTargetGeometry = _GetGeometry();

			TargetGeometry = newTargetGeometry;
		}
	}
}
