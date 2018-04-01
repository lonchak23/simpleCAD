using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace simpleCAD_Example
{
	internal class DocumentChangesToBangeConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			// If changes count = 0 then no need to show changes in badge.
			// Badge will hide if it receives empty string.
			if(value is int)
			{
				int iChanges = (int)value;
				if (iChanges > 0)
					return iChanges.ToString();
			}

			return string.Empty;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
