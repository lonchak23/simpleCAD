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
		bool IsPlaced { get; }
		List<PropertyViewModel> Properties { get; }

		void Draw(ICoordinateSystem cs, DrawingContext dc);

		DrawingVisual GetGeometryWrapper();
		List<Point> GetGripPoints();
		bool SetGripPoint(int gripIndex, Point pnt);

		void OnMouseLeftButtonClick(Point globalPoint);
		void OnMouseMove(Point globalPoint);

		object GetPropertyValue(string strPropSysName);
		bool SetPropertyValue(string strPropSysName, object propValue);
	}
}
