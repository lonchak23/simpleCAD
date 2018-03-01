using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace simpleCAD
{
	public interface ICoordinateSystem
	{
		Point GetLocalPoint(Point globalPnt);

		double Get_Scale();
	}
}
