using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using MVVM_BootLoader;
using System.Windows.Controls;
using System.IO;

namespace MVVM_BootLoader
{
	class Main_ViewModel: ViewModelBase
	{
		clsOpenHexFile writer = new clsOpenHexFile();

		public Main_ViewModel()
		{
			ListAGV = new ObservableCollection<BootLoaderViewModel>();
			ListAGVName = new ObservableCollection<string>();

			ListAGVName.CollectionChanged += ListAGVName_CollectionChanged;
			InitCommand();
			writer.ReadLog( ref _listAGVName );
			ListAGVName = _listAGVName;
			NewAGVAddress = "40C403B0";
			NewAGVName = "AGV";
		}

		private void ListAGVName_CollectionChanged( object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e )
		{
			RaisePropertyChanged( "ListAGVName" );
		}

		#region "Command declare"
		public ICommand AddCommand { get; set; }
		public ICommand DeleteCommand { get; set; }
		public ICommand SelectRecentCommand { get; set; }
		public ICommand ClearHistoryCommand { get; set; }
		public ICommand ClearLogCommand { get; set; }

		private void InitCommand()
		{
			AddCommand = new RelayCommand<UIElementCollection>(
				( s ) => true,
				( s ) => { Add_Execute( s ); } );

			DeleteCommand = new RelayCommand<BootLoaderViewModel>(
				( b ) => true,
				( b ) => { Delete_Execute( b ); } );

			SelectRecentCommand = new RelayCommand<string>(
				( m ) => true,
				( m ) => { SelectRecent_Execute( m ); } );

			ClearHistoryCommand = new RelayCommand( () => { ClearHistory_Execute(); } );
		}

		private void ClearHistory_Execute()
		{
			ListAGVName.Clear();
			writer.DeleteHistory(); ;
		}

		private void SelectRecent_Execute( string m )
		{
			string[] stringArray = m.Split( ',' );
			NewAGVName = stringArray[ 0 ];
			NewAGVAddress = stringArray[ 1 ];
		}

		private void Delete_Execute( BootLoaderViewModel b )
		{
			b.DeleteHost();
			ListAGV.Remove( b );
		}

		private void Add_Execute( UIElementCollection p )
		{

			int add = 0;
			string name = "";
			bool isIDInt = false;
			foreach ( var item in p )
			{
				TextBox a = item as TextBox;
				if ( a == null )
				{
					continue;
				}
				switch ( a.Name )
				{
					case "txbAdd":
						isIDInt = Int32.TryParse( a.Text, System.Globalization.NumberStyles.HexNumber, null, out add );
						break;
					case "txbName":
						name = a.Text;
						break;
				}
			}

			if ( !isIDInt || string.IsNullOrEmpty( name ) )
			{
				return;
			}

			BootLoaderViewModel agv = new BootLoaderViewModel( name, add );
			ListAGV.Add( agv );

			string str = name + "," + add.ToString( "X4" );
			if ( !ListAGVName.Contains( str ) )
			{
				writer.WriteLog( str );
				ListAGVName.Add( str );
			}
		}

		#endregion

		#region "Property declare"
		private ObservableCollection<BootLoaderViewModel> _listAGV;
		ObservableCollection<string> _listAGVName;
		string _newAGVName;
		string _newAGVAddress;

		public string NewAGVAddress
		{
			get { return _newAGVAddress; }
			set
			{
				_newAGVAddress = value;
				RaisePropertyChanged( "NewAGVAddress" );
			}
		}

		public string NewAGVName
		{
			get { return _newAGVName; }
			set
			{
				_newAGVName = value;
				RaisePropertyChanged( "NewAGVName" );
			}
		}

		public ObservableCollection<string> ListAGVName
		{
			get { return _listAGVName; }
			set { _listAGVName = value; }
		}

		public ObservableCollection<BootLoaderViewModel> ListAGV
		{
			get { return _listAGV; }
			set { _listAGV = value; }
		}

		#endregion

	}
}
