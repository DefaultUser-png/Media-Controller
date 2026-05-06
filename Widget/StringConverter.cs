using Microsoft.UI.Xaml.Data;
using System;

namespace Converters {
	public class StringConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, string language) {
			string? formatString = parameter as string;
			if (!string.IsNullOrEmpty(formatString)) return string.Format(formatString, value);
			return "";
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language) {
			throw new NotImplementedException();
		}

	}
}
