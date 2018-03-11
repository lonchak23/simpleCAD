using simpleCAD.Geometry;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace simpleCAD
{
	/// <summary>
	/// Base interface for all geometry objects that can be drawn by cad.
	/// </summary>
	public interface ICadGeometry
	{
		string DisplayName { get; }

		ImageSource GeomImage { get; }

		bool IsPlaced { get; }
		List<Property_ViewModel> Properties { get; }

		void Draw(ICoordinateSystem cs, DrawingContext dc);

		DrawingVisual GetGeometryWrapper();
		List<Point> GetGripPoints();
		bool SetGripPoint(int gripIndex, Point pnt);

		void OnMouseLeftButtonClick(Point globalPoint);
		void OnMouseMove(Point globalPoint);

		void OnKeyDown(System.Windows.Input.KeyEventArgs e);

		object GetPropertyValue(string strPropSysName);
		bool SetPropertyValue(string strPropSysName, object propValue);

		ITooltip Tooltip { get; }

		/// <summary>
		/// Need for realization "Prototype" pattern.
		/// simpleCad will call Clone method to add new object at a plot.
		/// This method allow user to configure which object simpleCad should create at runtime.
		/// </summary>
		/// <returns></returns>
		ICadGeometry Clone();
	}
}
