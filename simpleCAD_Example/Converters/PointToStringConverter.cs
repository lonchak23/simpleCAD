using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace simpleCAD_Example
{
	internal class PointToStringConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			string strResult = string.Empty;

			if (value is Point)
			{
				Point pnt = (Point)value;
				string strX = pnt.X.ToString("0.00");
				strX = strX.Replace(',', '.');
				string strY = pnt.Y.ToString("0.00");
				strY = strY.Replace(',', '.');

				strResult = strX + ", " + strY;
			}

			return strResult;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
