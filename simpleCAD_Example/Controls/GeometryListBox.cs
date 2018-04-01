using simpleCAD;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace simpleCAD_Example.Controls
{
	public class GeometryListBox : ListBox
	{
		//=============================================================================
		protected override bool IsItemItsOwnContainerOverride(object item)
		{
			return item is GeometryListBoxItem;
		}

		//=============================================================================
		protected override DependencyObject GetContainerForItemOverride()
		{
			return new GeometryListBoxItem();
		}

		//=============================================================================
		protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
		{
			base.OnPreviewMouseDown(e);

			GeometryListBoxItem item = Utils.TryFindParent<GeometryListBoxItem>((DependencyObject)e.OriginalSource);
			if (item != null)
				item.OnClick();

			// mark event handled, item is selected already
			e.Handled = true;
		}
	}


	/// <summary>
	/// ListBoxItem with TargetGeometry dependency property, which one should be binded to DrawingHost.GeometryToCreate dependency property.
	/// If user clicks at this item, then ICagGeometry will be get from DataContext and set to TargetProperty.
	/// 
	/// Why do you use so difficult way instead using default ListBoxItem and bind his IsSelected property to DrawingHost.GeometryToCreate with converter?
	/// Converter with multibinding works perfect in one way - from source(IsSelected) to target(GeometryToCreate), i can pass ICadGeometry from data context to converter
	/// and return it from Convert() method to Target property. But when Target(GeometryToCreate) was changed - for example another ListBoxItem with another underlying ICadGeometry was selected, 
	/// i cant get current ListBoxItem underlying ICadGeometry from anywhere in converter ConvertBack() method. In ConvertBack() I need current ListBoxItem.ICadGeometry 
	/// for comparsion with DrawingHost.GeometryToCreate and return bool value - Is current checkbox ICadGeometry setted in DrawingHost.GeometryToCreate.
	/// </summary>
	public class GeometryListBoxItem : ListBoxItem
	{
		//=============================================================================
		public static readonly DependencyProperty TargetGeometryProperty;
		public ICadGeometry TargetGeometry
		{
			get { return (ICadGeometry)GetValue(GeometryListBoxItem.TargetGeometryProperty); }
			set { SetValue(GeometryListBoxItem.TargetGeometryProperty, value); }
		}
		private static void On_TargetGeometry_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			GeometryListBoxItem gc = d as GeometryListBoxItem;
			if (gc != null)
				gc.OnTargetGeometryChanged(e.NewValue as ICadGeometry);
		}

		//=============================================================================
		static GeometryListBoxItem()
		{
			GeometryListBoxItem.TargetGeometryProperty = DependencyProperty.Register(
				"TargetGeometry",
				typeof(ICadGeometry),
				typeof(GeometryListBoxItem),
				new FrameworkPropertyMetadata(null, On_TargetGeometry_Changed));
		}

		//=============================================================================
		public void OnTargetGeometryChanged(ICadGeometry newTargetGeometry)
		{
			//
			// Dont comare like
			// _GetGeometry() != newTargetGeometry
			//
			// because its comparsion of instances. If i get deep clone copy of instance(serialize - deserialize)
			// and then comapre it with original it will be not equal instances.
			// So its need to compare DisplayName or some unique ICadGeometry identificator.
			string thisGeomName = string.Empty;
			ICadGeometry thisGeom = _GetGeometry();
			if (thisGeom != null)
				thisGeomName = thisGeom.DisplayName;

			string targetGeomName = string.Empty;
			if (newTargetGeometry != null)
				targetGeomName = newTargetGeometry.DisplayName;

			if (thisGeomName != targetGeomName)
				this.IsSelected = false;
			else
				this.IsSelected = true;
		}

		//=============================================================================
		public ICadGeometry _GetGeometry()
		{
			return this.DataContext as ICadGeometry;
		}

		//=============================================================================
		public void OnClick()
		{
			ICadGeometry newTargetGeometry = null;
			if (!IsSelected)
				newTargetGeometry = _GetGeometry();

			TargetGeometry = newTargetGeometry;
		}
	}
}
