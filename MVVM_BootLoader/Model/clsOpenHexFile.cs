using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVVM_BootLoader
{
	public class clsOpenHexFile
	{
		public void ReadHexData( ref List<string> hexData, out int totalLine,out string pathOfFile )
		{
			OpenFileDialog OpenFile = new OpenFileDialog();
			Stream myStream = null;
			OpenFile.Filter = "Hex file (*.hex)|*.hex|All files (*.*)|*.*";
			hexData.Clear();
			OpenFile.ShowDialog();

			if ( !string.IsNullOrEmpty( OpenFile.FileName ) )
			{
				myStream = OpenFile.OpenFile();
				StreamReader sReader = new StreamReader( myStream );
				while ( sReader.Peek() >= 0 )
				{
					string data;
					data = sReader.ReadLine() + Convert.ToChar( 13 );
					hexData.Add( data );
					if ( data == ":00000001FF\r" )
						break;
				}
				myStream.Close();
				sReader.Close();
			}
			totalLine = hexData.Count;
			pathOfFile = OpenFile.FileName;
		}

		public void WriteLog(string txt)
		{
			string fileName = "History.txt";

			using ( StreamWriter writer = new StreamWriter( fileName, true ) )
			{
				writer.WriteLine( txt );
			}
		}

		public void DeleteHistory()
		{
			if ( !File.Exists( ".\\History.txt" ) ) return;
			File.Delete( ".\\History.txt" );
		}

		public void ReadLog( ref ObservableCollection<string> listString )
		{
			if ( !File.Exists( ".\\History.txt" ) ) return;
			StreamReader sReader = new StreamReader( ".\\History.txt" );
			while ( sReader.Peek() >= 0 )
			{
				string data;
				data = sReader.ReadLine();
				listString.Add( data );
			}
			sReader.Close();
		}
	}
}
