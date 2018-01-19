using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace MVVM_BootLoader
{
	class XbeeModel
	{
		public enum API_Commad: byte
		{
			TRANSMIT = 0x10,
			RECEIVE = 0x90,
			AT_RESPOND = 0x88,
			REMOTE_AT_RESPOND = 0x97
		}

		public enum AT_COMMAND_ENUM: int
		{
			SERIAL_NUMBER_LOW = 0x534c,		//SL
			PAN_ID = 0x4944,				//ID
			APPLY_CHANGE = 0x4143,			//AC
			WRITE = 0x5752,					//WR
			CHANNEL = 0x4348,				//CH
			CHANNEL_S2B = 0x5343			//SC
		}

		public enum COMMAND_STATUS_ENUM: byte
		{
			OK = 0x0,
			ERR = 1,
			INVALID_COMMAND = 2,
			INVALID_PARAMETER = 3,
			TX_FAILURE = 4,
			DONT_KNOW = 99
		}

		public COMMAND_STATUS_ENUM command_status;
		public SerialPort port = new SerialPort();
		private byte[] DataTransmit = new byte[ 200 ];
		public byte[] Receive = new byte[ 200 ];
		private UInt16 ReceiveLen;
		private int DataTransmithLen;
		//private List<byte> DataReceive;
		private byte ReceiveStep = 0;
		private int DataReceiveIndex = 0;
		private bool isConnect = false;
		public UInt32 Address;
		public short CH;
		public short ID;
		public BootLoaderViewModel parent;
		public bool isCreatedReceiveEvent = false;


		public XbeeModel()
		{
			SetupDataTransmit();
			CreatEventDataReceive();
		}

		public void CreatEventDataReceive()
		{
			if(!isCreatedReceiveEvent)
			{
				port.DataReceived += new SerialDataReceivedEventHandler( DataReceivedHandler );
				isCreatedReceiveEvent = true;
			}
		}

		public void RemoveEventDataReceive()
		{
			if ( isCreatedReceiveEvent )
			{
				port.DataReceived -= new SerialDataReceivedEventHandler( DataReceivedHandler );
				isCreatedReceiveEvent = false;
			}
		}

		public void DataReceivedHandler( object sender, SerialDataReceivedEventArgs e )
		{
			int ReceiveEachTime;
			if ( port.IsOpen == false )
				return;
			while ( port.BytesToRead != 0 )
			{
				ReceiveEachTime = port.ReadByte();
				if ( ( ReceiveEachTime == 0x7e ) & ( ReceiveStep == 0 ) )
				{
					ReceiveStep += 1;
					return;
				}
				switch ( ReceiveStep )
				{
					case 1:
						if ( ReceiveEachTime == 0 )
						{
							ReceiveStep += 1;
						}
						else
						{
							ReceiveStep = 0;
						}
						continue;
					case 2:
						ReceiveLen = (byte)ReceiveEachTime;
						ReceiveStep += 1;
						DataReceiveIndex = 0;
						continue;
					case 3:
						if ( DataReceiveIndex == 199 ) DataReceiveIndex = 0;//Error
						Receive[ DataReceiveIndex ] = (byte)ReceiveEachTime;
						DataReceiveIndex += 1;
						if ( DataReceiveIndex >= ReceiveLen )
						{
							ReceiveStep += 1;
						}
						continue;
					case 4:
						if ( ReceiveCheckSum() == ReceiveEachTime )
						{
							Receive[ DataReceiveIndex + 3 ] = (byte)ReceiveEachTime;
							AnalizeInformationReceived();
						}
						ReceiveStep = 0;
						break;
				}
			}
			return;
		}

		public bool Connect( string portName, int baud )
		{
			if ( !isConnect )
			{
				try
				{
					if ( !port.IsOpen )
					{
						port.PortName = portName;
						port.BaudRate = baud;
						port.Open();
						isConnect = true;
						return true;
					}
					return false;
				}
				catch
				{
					MessageBox.Show( "Please try again !!!" );
					return false;
				}
			}
			else
			{
				if ( port.IsOpen )
				{
					port.Close();
					isConnect = false;
				}
				isConnect = false;
				return false;
			}
		}

		public void ClosePort()
		{
			if ( port.IsOpen )
				port.Close();
		}
		private void SetupDataTransmit()
		{
			DataTransmit[ 0 ] = 0x7e;//Header
			DataTransmit[ 1 ] = 0x0;//Len
			DataTransmit[ 2 ] = 0x0;//Len
			DataTransmit[ 3 ] = 0x10;//Frame type
			DataTransmit[ 4 ] = 0x01;//Frame ID
			//64 bit Address
			DataTransmit[ 5 ] = 0x0;
			DataTransmit[ 6 ] = 0x13;
			DataTransmit[ 7 ] = 0xa2;
			DataTransmit[ 8 ] = 0x0;
			DataTransmit[ 9 ] = 0x40;
			DataTransmit[ 10 ] = 0x40;
			DataTransmit[ 11 ] = 0x40;
			DataTransmit[ 12 ] = 0x40;
			//16 bit Address
			DataTransmit[ 13 ] = 0xff;
			DataTransmit[ 14 ] = 0xfe;
			//Option
			DataTransmit[ 15 ] = 0x0;//Broadcast radius
			DataTransmit[ 16 ] = 0x0;//Option//Data
			DataTransmit[ 17 ] = 0x1;
			//Check sum
			DataTransmit[ 18 ] = 0x1;//max data len = 73 byte
		}

		public void XbeeSendData( byte[] data, int address )
		{
			port.DiscardInBuffer();//delete all Receive buffer to prevent receive unwanted data
			DataTransmithLen = data.Length + 14;
			DataTransmit[ 2 ] = (byte)DataTransmithLen;
			byte[] addArray = new byte[ 3 ];
			addArray = BitConverter.GetBytes( address );
			DataTransmit[ 9 ] = addArray[ 3 ];
			DataTransmit[ 10 ] = addArray[ 2 ];
			DataTransmit[ 11 ] = addArray[ 1 ];
			DataTransmit[ 12 ] = addArray[ 0 ];

			for ( byte i = 0; i <= data.Length - 1; i++ )
			{
				DataTransmit[ 17 + i ] = data[ i ];
			}
			DataTransmit[ 17 + data.Length ] = TransmithCheckSum();
			port.Write( DataTransmit, 0, 18 + data.Length );
		}

		private byte TransmithCheckSum()
		{
			int sumTemp = 0;
			int sum;
			byte[] sumArray = new byte[ 2 ];
			for ( byte i = 0; i <= DataTransmithLen - 1; i++ )
			{
				sumTemp = sumTemp + DataTransmit[ i + 3 ];
			}
			sumArray = BitConverter.GetBytes( sumTemp );
			sum = 0xff - sumArray[ 0 ];
			return (byte)sum;
		}

		private byte ReceiveCheckSum()
		{
			int sum = 0;
			byte[] tempArray = new byte[ 2 ];
			for ( byte i = 0; i <= ReceiveLen - 1; i++ )
			{
				sum = sum + Receive[ i ];
			}
			tempArray = BitConverter.GetBytes( sum );
			sum = 0xff - tempArray[ 0 ];
			return (byte)sum;
		}

		public bool ReceiveXbee()
		{
			byte ReceiveEachTime;
			if ( port.IsOpen == false )
				return false;
			while ( port.BytesToRead != 0 )
			{
				ReceiveEachTime = (byte)port.ReadByte();
				if ( ( ReceiveEachTime == 0x7e ) & ( ReceiveStep == 0 ) )
				{
					ReceiveStep += 1;
					return false;
				}
				switch ( ReceiveStep )
				{
					case 1:
						if ( ReceiveEachTime == 0 )
						{
							ReceiveStep += 1;
						}
						else
						{
							ReceiveStep = 0;
						}
						continue;
					case 2:
						ReceiveLen = ReceiveEachTime;
						ReceiveStep += 1;
						DataReceiveIndex = 0;
						continue;
					case 3:
						if ( DataReceiveIndex == 199 ) DataReceiveIndex = 0;//Error
						Receive[ DataReceiveIndex ] = ReceiveEachTime;
						DataReceiveIndex += 1;
						if ( DataReceiveIndex >= ReceiveLen )
						{
							ReceiveStep += 1;
						}
						continue;
					case 4:
						if ( ReceiveCheckSum() == ReceiveEachTime )
						{
							Receive[ DataReceiveIndex + 3 ] = ReceiveEachTime;
							if ( Receive[ 0 ] == 0x90 )
								return true;
							AnalizeInformationReceived();
						}
						ReceiveStep = 0;
						break;
				}
			}
			return false;
		}

		private void AnalizeInformationReceived()
		{
			switch ( Receive[ 0 ] )
			{
				case (byte)API_Commad.RECEIVE:

					Debug.Print( "Received transmit data" );
					break;
				case (byte)API_Commad.AT_RESPOND:
					{
						AT_COMMAND_ENUM AT_Command;
						AT_Command = (AT_COMMAND_ENUM)( Receive[ 2 ] * 256 + Receive[ 3 ] );
						COMMAND_STATUS_ENUM temp = (COMMAND_STATUS_ENUM)Receive[ 4 ];
						Debug.Print( "Receive AT_RESPOND of command " + AT_Command.ToString() + " with status " + temp.ToString() );
						switch ( AT_Command )
						{
							case AT_COMMAND_ENUM.SERIAL_NUMBER_LOW:
								{
									byte[] add = new byte[ 4 ] { Receive[ 8 ], Receive[ 7 ], Receive[ 6 ], Receive[ 5 ] };
									Address = BitConverter.ToUInt32( add, 0 );
									Debug.Print( "Received AT address: " + Address.ToString( "X4" ) );
									parent.HostAddress = (int)Address;
									break;
								}
							case AT_COMMAND_ENUM.CHANNEL:
								{
									if ( ReceiveLen == 6 )
									{
										byte Channel = Receive[ 5 ];
										CH = Channel;
										parent.Channel = CH;
										Debug.Print( "Received AT CH: " + CH.ToString( "X4" ) );
									}
									break;
								}
							case AT_COMMAND_ENUM.PAN_ID:
								{
									if ( ReceiveLen == 7 )
									{
										byte[] pan_id = new byte[ 2 ] { Receive[ 6 ], Receive[ 5 ] };
										ID = BitConverter.ToInt16( pan_id, 0 );
										parent.ID = ID;
										Debug.Print( "Received AT ID: " + ID.ToString( "X4" ) );
									}
									else if ( ReceiveLen == 13 )
									{
										byte[] pan_id = new byte[ 2 ] { Receive[ 2 ], Receive[ 11 ] };
										ID = BitConverter.ToInt16( pan_id, 0 );
										parent.ID = ID;
										Debug.Print( "Received AT ID: " + ID.ToString( "X4" ) );
									}
									break;
								}

							case AT_COMMAND_ENUM.CHANNEL_S2B:
								{
									if ( ReceiveLen == 7 )
									{
										byte[] temp_sc = new byte[] { Receive[ 6 ], Receive[ 5 ] };
										CH = BitConverter.ToInt16( temp_sc, 0 );
										Debug.Print( "Received AT ID: " + CH.ToString( "X4" ) );
									}
									parent.Channel = CH;
									break;
								}
						}
						command_status = (COMMAND_STATUS_ENUM)temp;
						break;
					}
				case (byte)API_Commad.REMOTE_AT_RESPOND:
					{
						AT_COMMAND_ENUM AT_Command;
						AT_Command = (AT_COMMAND_ENUM)( Receive[ 12 ] * 256 + Receive[ 13 ] );
						COMMAND_STATUS_ENUM temp = (COMMAND_STATUS_ENUM)Receive[ 14 ];
						Debug.Print( "Received remode AT respond: " + AT_Command.ToString() + " with status " + temp.ToString() );
						command_status = (COMMAND_STATUS_ENUM)temp;
						break;
					}
			}
		}


		public void Send_AT_Command( AT_COMMAND_ENUM cmd )
		{
			byte[] data;
			byte[] command = BitConverter.GetBytes( (int)cmd );
			data = new byte[ 8 ];
			data[ 0 ] = 0x7e;
			data[ 1 ] = 0;
			data[ 2 ] = 4;
			data[ 3 ] = 0x8;
			data[ 4 ] = 0x1;
			data[ 5 ] = command[ 1 ];
			data[ 6 ] = command[ 0 ];

			byte i;
			int sum;
			byte[] sumArray;
			byte len;
			len = 4;
			sum = 0;
			for ( i = 3; i <= len + 2; i++ )
			{
				sum = sum + data[ i ];
			}
			sumArray = BitConverter.GetBytes( sum );
			data[ len + 3 ] = (byte)( 0xff - sumArray[ 0 ] );

			port.Write( data, 0, 8 );
		}

		public void Send_AT_Command( AT_COMMAND_ENUM cmd, byte[] parameter )
		{
			byte[] data;
			byte[] command = BitConverter.GetBytes( (int)cmd );
			data = new byte[ 8 + parameter.Length ];
			data[ 0 ] = 0x7e;
			data[ 1 ] = 0;
			data[ 2 ] = (byte)( 4 + parameter.Length );
			data[ 3 ] = 0x8;
			data[ 4 ] = 0x1;
			data[ 5 ] = command[ 1 ];
			data[ 6 ] = command[ 0 ];
			for ( byte j = 0; j <= parameter.Length - 1; j++ )
			{
				data[ 7 + j ] = parameter[ j ];
			}

			byte i;
			int sum;
			byte[] sumArray;
			byte len;
			len = (byte)( 4 + parameter.Length );
			sum = 0;
			for ( i = 3; i <= len + 2; i++ )
			{
				sum = sum + data[ i ];
			}
			sumArray = BitConverter.GetBytes( sum );
			data[ len + 3 ] = (byte)( 0xff - sumArray[ 0 ] );

			port.Write( data, 0, data.Length );
		}

		public void Send_Remote_AT_Command( UInt32 address, byte RemoteOption, AT_COMMAND_ENUM cmd )
		{
			byte[] data;
			byte[] command = BitConverter.GetBytes( (byte)cmd );
			data = new byte[ 19 ] { 0x7e, 0x0, 0xf, 0x17, 0x1, 0x0, 0x13, 0xa2, 0x0, 0, 0, 0, 0, 0xff, 0xfe, RemoteOption, command[ 1 ], command[ 0 ], 0 };
			byte DestByte;
			for ( int i = 0; i <= 3; i++ )
			{
				DestByte = (byte)( ( address >> ( 24 - i * 8 ) ) & 0xff );
				data[ 9 + i ] = DestByte;
			}

			int sum;
			byte[] sumArray;
			byte len;
			len = (byte)data.Length;
			sum = 0;
			for ( int i = 3; i <= len - 1; i++ )
			{
				sum = sum + data[ i ];
			}
			sumArray = BitConverter.GetBytes( sum );
			data[ len - 1 ] = (byte)( 0xff - sumArray[ 0 ] );

			port.Write( data, 0, len );
		}

		public void Send_Remote_AT_Command( UInt32 address, byte RemoteOption, AT_COMMAND_ENUM cmd, byte[] Parameter )
		{
			byte[] data;
			byte[] command = BitConverter.GetBytes( (int)cmd );
			data = new byte[ 19 + Parameter.Length - 1 ];
			data[ 0 ] = 0x7e;
			data[ 1 ] = 0x0;
			data[ 2 ] = (byte)( data.Length - 4 );
			data[ 3 ] = 0x17;
			data[ 4 ] = 0x1;
			data[ 5 ] = 0x0;
			data[ 6 ] = 0x13;
			data[ 7 ] = 0xa2;
			data[ 8 ] = 0x0;
			byte DestByte;
			for ( int i = 0; i <= 3; i++ )
			{
				DestByte = (byte)( ( address >> ( 24 - i * 8 ) ) & 0xff );
				data[ 9 + i ] = DestByte;
			}
			data[ 13 ] = 0xff;
			data[ 14 ] = 0xfe;
			data[ 15 ] = RemoteOption;
			data[ 16 ] = command[ 1 ];
			data[ 17 ] = command[ 0 ];
			for ( byte i = 0; i <= Parameter.Length - 1; i++ )
			{
				data[ 18 + i ] = Parameter[ i ];
			}
			int sum;
			byte[] sumArray;
			byte len;
			len = (byte)( data.Length );
			sum = 0;
			for ( int i = 3; i <= len - 1; i++ )
			{
				sum = sum + data[ i ];
			}
			sumArray = BitConverter.GetBytes( sum );
			data[ len - 1 ] = (byte)( 0xff - sumArray[ 0 ] );

			port.Write( data, 0, len );
		}

	}
}
