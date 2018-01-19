using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace MVVM_BootLoader
{
	public enum EnumModule
	{
		FOLLOW_LINE = 1,
		BATTERY_SAFETY = 2,
		CARD_READER = 3,
		CONTROL_SYSTEM = 4,
		DISPLAY_PANEL = 5,
		CROSS_FUNCTION = 6
	}

	public class BootLoaderViewModel: ViewModelBase
	{
		XbeeModel myXbee = new XbeeModel();
		List<string> HexData = new List<string>();
		public Thread ProgramThread;
		private System.Timers.Timer TimerCycle = new System.Timers.Timer( 1000 );

		enum EnumAck
		{
			ACK = 0x06,
			NACK = 0x07,
			XON = 0x11,
			XOFF = 0x13
		}

		public BootLoaderViewModel( string name, int add )
		{
			Name = name;
			TargetXbeeAdd = add;
			myXbee.parent = this;
			InitCommand();
			ListPortName = SerialPort.GetPortNames();
			TimerCycle.Elapsed += new System.Timers.ElapsedEventHandler( OnTimedEvent );
			LogOutput = "Welcome to AGV wireless program";
			SelectedBaud = 57600;
			SelectedChip = EnumModule.CONTROL_SYSTEM;
			if ( ListPortName.Count() == 1 )
				SelectedPort = ListPortName[ 0 ];
			//HostAddress = 0x41257fce;
		}

		private void OnTimedEvent( object sender, System.Timers.ElapsedEventArgs e )
		{
			CycleTime++;
		}

		public BootLoaderViewModel()
		{
			// TODO: Complete member initialization
		}

		#region ""---------------Property declare"---------------"
		private EnumModule _selectedChip;
		private string _selectedPort;
		private int _selectedBaud;
		private bool _isConnected;
		private int _totalHexLine;
		private string _pathOfFile;
		private int _cycleTime;
		private string _logOutput;
		private string _name;
		private int _channel;
		private int _iD;
		private int _hostAddress;
		private int _targetXbeeAdd;
		private int _hexIndex;
		private int _progressPercent;

		public int ProgressPercent
		{
			get { return _progressPercent; }
			set
			{
				_progressPercent = value;
				RaisePropertyChanged( "ProgressPercent" );
			}
		}

		public int HexIndex
		{
			get { return _hexIndex; }
			set
			{
				_hexIndex = value;
				ProgressPercent = (int)( ( (double)HexIndex / (double)TotalHexLine ) * 100 );
				RaisePropertyChanged( "HexIndex" );
			}
		}

		public int TargetXbeeAdd
		{
			get { return _targetXbeeAdd; }
			set
			{
				_targetXbeeAdd = value;
				RaisePropertyChanged( "TargetXbeeAdd" );
			}
		}

		public int HostAddress
		{
			get
			{
				return _hostAddress;
			}
			set
			{
				_hostAddress = value;
				RaisePropertyChanged( "HostAddress" );
			}
		}

		public int ID
		{
			get
			{
				return _iD;
			}
			set
			{
				_iD = value;
				RaisePropertyChanged( "id" );
			}
		}

		public int Channel
		{
			get { return _channel; }
			set
			{
				_channel = value;
				RaisePropertyChanged( "Channel" );
			}
		}

		public string Name
		{
			get { return _name; }
			set
			{
				_name = value;
			}
		}


		public string SelectedPort
		{
			get
			{
				return _selectedPort;
			}
			set
			{
				_selectedPort = value;
			}
		}

		public EnumModule SelectedChip
		{
			get
			{
				return _selectedChip;
			}
			set
			{
				_selectedChip = value;
			}
		}

		public int SelectedBaud
		{
			get
			{
				return _selectedBaud;
			}
			set
			{
				_selectedBaud = value;
			}
		}

		public bool IsConnected
		{
			get
			{
				return _isConnected;
			}
			set
			{
				_isConnected = value;
				RaisePropertyChanged( "IsConnected" );
			}
		}

		public int TotalHexLine
		{
			get
			{
				return _totalHexLine;
			}
			set
			{
				_totalHexLine = value;
				RaisePropertyChanged( "TotalHexLine" );
			}
		}
		public string PathOfFile
		{
			get
			{
				return _pathOfFile;
			}
			set
			{
				_pathOfFile = value;
				RaisePropertyChanged( "PathOfFile" );
			}
		}

		public string LogOutput
		{
			get
			{
				return _logOutput;
			}
			set
			{
				_logOutput = value + "\r";
				RaisePropertyChanged( "LogOutput" );
			}
		}

		public int CycleTime
		{
			get
			{
				return _cycleTime;
			}
			set
			{
				_cycleTime = value;
				RaisePropertyChanged( "CycleTime" );
			}
		}

		public string[] ListPortName { get; set; }

		#endregion

		#region "---------------Command"---------------"
		public ICommand ConnectCommand { get; set; }
		public ICommand OpenFileCommand { get; set; }
		public ICommand ProgramCommand { get; set; }
		public ICommand ReadChannelCommand { get; set; }
		public ICommand ReadIDCommand { get; set; }
		public ICommand CancelCommand { get; set; }
		public ICommand CheckBuildCommand { get; set; }
		public ICommand ResetCommand { get; set; }
		public ICommand WriteChannelCommand { get; set; }
		public ICommand WriteIDCommand { get; set; }


		private void InitCommand()
		{
			ConnectCommand = new RelayCommand( () => Connect_Execute() );
			OpenFileCommand = new RelayCommand( () => OpenFile_Execute() );
			ProgramCommand = new RelayCommand( () => Program_Execute() );
			ReadChannelCommand = new RelayCommand( () => ReadChannel_Execute() );
			ReadIDCommand = new RelayCommand( () => ReadID_Execute() );
			CancelCommand = new RelayCommand( () => Cancel_Execute() );
			CheckBuildCommand = new RelayCommand( () => CheckBuild_Execute() );
			ResetCommand = new RelayCommand( () => Reset_Execute() );
			WriteChannelCommand = new RelayCommand( () => WriteChannel_Execute() );
			WriteIDCommand = new RelayCommand( () => WriteID_Execute() );
		}

		private void WriteID_Execute()
		{
			byte[] tempID = BitConverter.GetBytes( ID );
			byte[] data = new byte[ 2 ] { tempID[ 1 ], tempID[ 0 ] };
			myXbee.Send_AT_Command( XbeeModel.AT_COMMAND_ENUM.PAN_ID, data );
			myXbee.Send_AT_Command( XbeeModel.AT_COMMAND_ENUM.WRITE );
			myXbee.Send_AT_Command( XbeeModel.AT_COMMAND_ENUM.APPLY_CHANGE );
		}

		private void WriteChannel_Execute()
		{
			byte[] tempChannel = BitConverter.GetBytes( Channel );
			byte[] data = new byte[ 1 ] { tempChannel[ 0 ] };
			myXbee.Send_AT_Command( XbeeModel.AT_COMMAND_ENUM.CHANNEL, data );
			myXbee.Send_AT_Command( XbeeModel.AT_COMMAND_ENUM.WRITE );
			myXbee.Send_AT_Command( XbeeModel.AT_COMMAND_ENUM.APPLY_CHANGE );
		}

		private void Reset_Execute()
		{
			ResetAGV();
		}

		private void Cancel_Execute()
		{
			breakTransfer = true;
		}

		private void CheckBuild_Execute()
		{
			myXbee.RemoveEventDataReceive();
			CheckBuildDate( (byte)SelectedChip );
		}

		private void ReadID_Execute()
		{
			if ( !ProgramThread.IsAlive )
				myXbee.CreatEventDataReceive();
			myXbee.Send_AT_Command( XbeeModel.AT_COMMAND_ENUM.PAN_ID );
		}

		private void ReadChannel_Execute()
		{
			if ( !ProgramThread.IsAlive )
				myXbee.CreatEventDataReceive();
			myXbee.Send_AT_Command( XbeeModel.AT_COMMAND_ENUM.CHANNEL );
		}

		private void Program_Execute()
		{
			myXbee.RemoveEventDataReceive();
			if ( ( ProgramThread == null ) || ( !ProgramThread.IsAlive ) )
			{
				ProgramThread = new Thread( () => ProgramPic( (byte)SelectedChip ) );
				ProgramThread.Start();
			}
		}

		private void OpenFile_Execute()
		{
			clsOpenHexFile openFile = new clsOpenHexFile();
			openFile.ReadHexData( ref HexData, out _totalHexLine, out _pathOfFile );
			if ( _pathOfFile.Length > 20 )
			{
				PathOfFile = _pathOfFile.Substring( _pathOfFile.Length - 20, 20 );
			}
			else
			{
				PathOfFile = _pathOfFile;
			}
			TotalHexLine = _totalHexLine;
		}

		private void Connect_Execute()
		{
			IsConnected = myXbee.Connect( SelectedPort, SelectedBaud );
			if ( IsConnected )
			{
				myXbee.CreatEventDataReceive();
				myXbee.Send_AT_Command( XbeeModel.AT_COMMAND_ENUM.SERIAL_NUMBER_LOW );
			}
		}

		#endregion


		private bool breakTransfer;
		public void ProgramPic( byte chipSelected )
		{
			LogOutput = "************Start************";
			CycleTime = 0;
			breakTransfer = false;
			TimerCycle.Start();

			//confirm select chip
			if ( !CommunicateChip( 'B', chipSelected ) )//Entry bootloader
			{
				LogOutput += "Can not connect to " + " " + Enum.GetName( typeof( EnumModule ), chipSelected );
				return;
			}
			else
			{
				LogOutput += "Connect successful to " + Enum.GetName( typeof( EnumModule ), chipSelected );
			}

			//start programming
			int TryTime = 0;
			byte TimeOutCount = 0;
			string str = "Send ";
			bool isAllowSend = true;
			bool isFinish = false;
			string hexAdd = "";
			HexIndex = 0;
			string add = "";

			while ( ( HexIndex < HexData.Count ) && ( !isFinish ) && !breakTransfer )
			{
				add = "";
				if ( isAllowSend )
				{
					hexAdd = HexData[ HexIndex ].Substring( 3, 4 );
					SendData( HexIndex );
					LogOutput += str + HexIndex.ToString() + HexData[ HexIndex ].Remove( HexData[ HexIndex ].Length - 1 );
					TryTime = 0;
					if ( TimeOutCount > 10 )
					{
						LogOutput += "************Transmit time out !!!************";
						break;
					}
				}
				isAllowSend = false;
				if ( myXbee.ReceiveXbee() )
				{
					if ( HexData[ HexIndex ] == ":00000001FF\r" )
						isFinish = true;
					switch ( myXbee.Receive[ 12 ] )
					{
						case (int)EnumAck.ACK:
							for ( int j = 0; j < 4; j++ )
							{
								add += (char)myXbee.Receive[ 13 + j ];
							}
							LogOutput += "Receive ACK: " + add;
							if ( add == hexAdd )
							{
								HexIndex++;
								isAllowSend = true;
								TimeOutCount = 0;
								str = "Send ";
							}
							else
							{
								LogOutput += "Re-send " + hexAdd;
							}
							break;
						case (int)EnumAck.XOFF://keep Reset Xbee so can not receive XOFF :((
							LogOutput += "************Receive time out !!!************";
							break;
						default:
							break;
					}
				}
				if ( ( ++TryTime > 100000 ) && ( isAllowSend == false ) )
				{
					TimeOutCount++;
					isAllowSend = true;
					str = "Re-send ";
				}
			}
			TimerCycle.Stop();
			LogOutput += "************Finish************";
			breakTransfer = true;
		}

		private bool CommunicateChip( char func, byte chipSelected, params int[] list )
		{
			byte[] addArray = new byte[ 3 ];
			addArray = BitConverter.GetBytes( HostAddress );
			byte[] data = new byte[] { Convert.ToByte( '#' ), addArray[ 3 ], addArray[ 2 ], addArray[ 1 ], addArray[ 0 ], Convert.ToByte( func ), chipSelected };
			myXbee.Receive[ 12 ] = 0;
			myXbee.XbeeSendData( data, TargetXbeeAdd );
			int timeOut = Environment.TickCount;
			while ( ( !myXbee.ReceiveXbee() ) && ( ( Environment.TickCount - timeOut ) < 1000 ) )
			{
				if ( ( Environment.TickCount - timeOut ) > 950 )
				{
					LogOutput += "Time out!!!";
					return false;
				}
			}
			if ( myXbee.Receive[ 12 ] == Convert.ToByte( func ) )
			{
				if ( myXbee.Receive[ 13 ] == chipSelected )
				{
					return true;
				}
				else
				{
					LogOutput += "Confirm error chip!!!";
					return false;
				}
			}
			else
				return false;
		}

		public bool CheckBuildDate( byte chipSelected )
		{
			string str = "";
			byte[] addArray = new byte[ 3 ];
			addArray = BitConverter.GetBytes( HostAddress );
			byte[] data = new byte[] { Convert.ToByte( '#' ), addArray[ 3 ], addArray[ 2 ], addArray[ 1 ], addArray[ 0 ], Convert.ToByte( 'V' ), chipSelected };
			myXbee.Receive[ 12 ] = 0;
			myXbee.XbeeSendData( data, TargetXbeeAdd );
			int timeOut = Environment.TickCount;
			while ( ( !myXbee.ReceiveXbee() ) && ( ( Environment.TickCount - timeOut ) < 1000 ) )
			{
				if ( ( Environment.TickCount - timeOut ) > 950 )
				{
					LogOutput += "Time out!!!";
					return false;
				}
			}
			if ( myXbee.Receive[ 12 ] == Convert.ToByte( 'V' ) )
			{
				for ( int i = 0; i <= 15; i++ )
				{
					if ( ( myXbee.Receive[ 13 + i ] == 0xFF ) || ( myXbee.Receive[ 13 + i ] == 0 ) ) break;
					str += Convert.ToChar( myXbee.Receive[ 13 + i ] );
				}
				LogOutput += str;
				return true;
			}
			else
				return false;
		}

		public void ResetAGV()
		{
			byte chipSelected = 0;
			byte[] addArray = new byte[ 3 ];
			addArray = BitConverter.GetBytes( HostAddress );
			byte[] data = new byte[] { Convert.ToByte( '#' ), addArray[ 3 ], addArray[ 2 ], addArray[ 1 ], addArray[ 0 ], Convert.ToByte( 'R' ), chipSelected };
			myXbee.XbeeSendData( data, TargetXbeeAdd );
		}

		public void SendData( int index )
		{
			byte[] data = Encoding.ASCII.GetBytes( HexData[ index ] );
			myXbee.XbeeSendData( data, TargetXbeeAdd );
		}

		public void DeleteHost()
		{
			breakTransfer = true;
			myXbee.ClosePort();
		}
	}
}
