using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MVVM_BootLoader
{
	public class ViewModelBase: INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		protected void RaisePropertyChanged( string propertyName = "" )
		{
			var handler = PropertyChanged;
			if ( handler != null ) handler( this, new PropertyChangedEventArgs( propertyName ) );
		}
	}
	public class ListAGVtoComboBox: IMultiValueConverter
	{
		public object Convert( object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture )
		{
			return null;
		}

		public object[] ConvertBack( object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture )
		{
			return null;
		}
	}



	public class PersonImageConverter: IValueConverter
	{
		/// <summary>
		/// Hàm chuyển đổi từ đường dẫn ảnh sang đối tượng Bitmap
		/// </summary>
		public object Convert( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
		{
			string imageName = value.ToString();
			Uri uri = new Uri( imageName, UriKind.RelativeOrAbsolute );
			BitmapFrame source = BitmapFrame.Create( uri );

			return source;
		}

		public object ConvertBack( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
		{
			throw new NotImplementedException();
		}
	}

	public class StringToIntergerConverter: IValueConverter
	{

		public object Convert( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
		{
			string input = value.ToString();
			int ret;
			if ( Int32.TryParse( input, out ret ) )
			{
				return ret;
			}
			return value;
		}

		public object ConvertBack( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
		{
			string input = value.ToString();
			int ret;
			if ( Int32.TryParse( input, out ret ) )
			{
				return ret;
			}
			return value;
		}
	}

	public class BoolToColorConverter: IValueConverter
	{
		public object Convert( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
		{
			bool input;
			object ret;
			if ( bool.TryParse( value.ToString(), out input ) )
			{
				if ( input )
					ret = Brushes.YellowGreen;
				else
					ret = Brushes.Gray;
			}
			else
				ret = Brushes.Gray;
			return ret;
		}

		public object ConvertBack( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
		{
			throw new NotImplementedException();
		}
	}

	public class AddressToColor: IValueConverter
	{
		public object Convert( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
		{
			if ( (int)value == 0 )
			{
				return Brushes.Transparent;
			}
			else
				return Brushes.YellowGreen;
		}

		public object ConvertBack( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
		{
			throw new NotImplementedException();
		}
	}


	public class BoolToStringConverter: IValueConverter
	{
		public object Convert( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
		{
			bool input;
			string ret;
			if ( bool.TryParse( value.ToString(), out input ) )
			{
				if ( input )
					ret = "Disconnect";
				else
					ret = "Connect";
			}
			else
				ret = "Connect";
			return ret;
		}

		public object ConvertBack( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
		{
			throw new NotImplementedException();
		}
	}

	public class IntToStringConverter: IValueConverter
	{
		public object Convert( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
		{
			int ret = (int)value;
			switch ( parameter.ToString() )
			{
				case "Hex":
					{
						return ret.ToString( "X2" );
						break;
					}
				case "Time":
					{
						return ret.ToString() + "s";
						break;
					}
			}
			return null;
		}

		public object ConvertBack( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
		{
			//string to int
			string input = value.ToString();
			int ret;
			if ( Int32.TryParse( input, System.Globalization.NumberStyles.HexNumber, null, out ret ) )
			{
				return ret;
			}
			return null;
		}
	}

	//[ValueConversion( typeof( DateTime ), typeof( String ) )]
	public class DateConverter: IValueConverter
	{
		public object Convert( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
		{
			DateTime date = (DateTime)value;
			return date.ToShortDateString();
		}

		public object ConvertBack( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
		{
			string strValue = value as string;
			DateTime resultDateTime;
			if ( DateTime.TryParse( strValue, out resultDateTime ) )
			{
				return resultDateTime;
			}
			return System.Windows.DependencyProperty.UnsetValue;
		}
	}


}
