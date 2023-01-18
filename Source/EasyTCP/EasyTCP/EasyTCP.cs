using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using System.Collections.Generic;

namespace EasyTCP
{
	public class EasyTCP
	{
		public delegate void dlgData(EasyTCP sender, ushort code, object value);

		public EasyTCP(int writeBufferLength = 65536)
		{
			_buffer = new byte[writeBufferLength];
			_ms = new MemoryStream(_buffer);
			_bw = new BinaryWriter(_ms, Encoding.Unicode);

			_readerThread = new Thread(ReceiveData) { IsBackground = true };
		}

		public static Version Version => typeof(EasyTCP).Assembly.GetName().Version;

		public void DefinePacket<PacketType>(ushort code) => DefinePacket(code, typeof(PacketType));
		public void DefinePacket(ushort code, Type packetType) => _ioContainer.GetOrCreate(_packets[code] = packetType);

		public void DefinePacketIO(Type packetType, ITypeIO io) => _ioContainer.Add(packetType, io);
		public void DefinePacketIO<PacketType>(ITypeIO io) => DefinePacketIO(typeof(PacketType), io);

		public void Send(ushort code, object value)
		{
			lock (_writeLock)
			{
				var io = _ioContainer.GetOrCreate(value.GetType());

				WriteCode(code);
				io.Write(_bw, value);
				Flush();
			}
		}

		#region Fields
		NetworkStream _ns;
		BinaryReader _br;
		TcpClient _client;

		readonly byte[] _buffer;
		readonly MemoryStream _ms;
		readonly BinaryWriter _bw;

		bool _isClosing;
		readonly object _writeLock = new object();

		readonly Thread _readerThread;

		readonly TypeIOs _ioContainer = new TypeIOs();
		readonly Dictionary<ushort, Type> _packets = new Dictionary<ushort, Type>();
		#endregion

		#region Events
		public event Action<EasyTCP> OnClosed;
		void FireClosed() => OnClosed?.Invoke(this);

		public event Action<EasyTCP, Exception> OnException;
		protected void fireException(Exception ex) => OnException?.Invoke(this, ex);

		public event dlgData OnData;
		void FireData(ushort code, object value) => OnData?.Invoke(this, code, value);
		#endregion

		#region Essential Methods
		public void Connect(TcpClient client, int versionMajor, int versionMinor)
		{
			_client = client;
			client.NoDelay = true;

			_ns = client.GetStream();
			_br = new BinaryReader(_ns, Encoding.Unicode);
			var bw = new BinaryWriter(_ns);

			bw.Write(versionMajor);
			bw.Write(versionMinor);

			var major = _br.ReadInt32();
			_br.ReadInt32(); // Skip Minor

			if (major != versionMajor)
				throw new Exception("Version Mismatch");

			_readerThread.Start();
		}

		public void SendCloseRequest()
		{
			if (_isClosing) return;

			_isClosing = true;

			lock (_writeLock)
			{
				WriteCode(0xFFFF);
				Flush();
			}
		}

		public void Wait4Close(int milliSeconds = 3000) { _readerThread.Join(milliSeconds); }

		void WriteCode(ushort code)
		{
			_bw.Write(code);
			_ms.Position += 4;
		}

		void Flush()
		{
			var len = (int)(_ms.Position - 6);
			_ms.Position = 2;
			_bw.Write(len);

			try
			{
				_ns.Write(_buffer, 0, len + 6);
			}
			catch (Exception E)
			{
				_isClosing = true;
				fireException(E);
			}

			_ms.Position = 0;
		}
		#endregion

		void ReceiveData()
		{
			try
			{
				for (; ; )
				{
					ushort code = _br.ReadUInt16();
					int len = _br.ReadInt32();

					if (code == 0xFFFF)
					{
						SendCloseRequest();

						_client.Close();
						FireClosed();
						return; // Exit the thread
					}

					if (_packets.TryGetValue(code, out var packetType))
						FireData(code, _ioContainer.Get(packetType).Read(_br));
					else
						_br.ReadBytes(len); // Skip the packet (Undefined packet [code])
				}
			}
			catch (Exception ex) { fireException(ex); }
		}
	}
}
