using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MVVM_BootLoader
{
	/// <summary>
	/// Interaction logic for usBootLoader.xaml
	/// </summary>
	public partial class usBootLoader:UserControl
	{
		public usBootLoader()
		{
			InitializeComponent();
			//if( SerialPort.GetPortNames().Length==1)
			//{
			//	cmbPort.SelectedIndex = 1;
			//}
		}

		private void txtLogger_TextChanged( object sender,TextChangedEventArgs e )
		{
			txtLog.ScrollToEnd();
		}
	}
}
