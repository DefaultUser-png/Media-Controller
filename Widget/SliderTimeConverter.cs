using Microsoft.UI.Xaml.Data;
using System;

namespace Converters {
	public class SliderTimeConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, string language) {
			if (value == null) return "0";
			var s = System.Convert.ToInt32(value);
			int m = s / 60;
			int h = m / 60;
			if (h > 0) return $"{h:0}:";
			return $"{m:0}:{s % 60:00}";
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language) {
			throw new NotImplementedException();
		}

	}
}
