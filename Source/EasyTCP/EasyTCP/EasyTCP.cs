using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Reflection;
using System.Net.Sockets;
using System.Collections.Generic;

namespace EasyTCP
{
	public class EasyTCP
	{
		public delegate void dlgData(EasyTCP Sender, ushort Code, object Value);

		public EasyTCP(int WriteBufferLength = 65536)
		{
			B = new byte[WriteBufferLength];
			MS = new MemoryStream(B);
			BW = new BinaryWriter(MS, Encoding.Unicode);

			T = new Thread(ReceiveData) { IsBackground = true };
		}

		public static Version Version => Assembly.GetExecutingAssembly().GetName().Version;

		private readonly Dictionary<ushort, Type> Packets = new Dictionary<ushort, Type>();

		public void DefinePacket<PacketType>(ushort Code) => DefinePacket(Code, typeof(PacketType));
		public void DefinePacket(ushort Code, Type PacketType) => CompositeTypeIO.GetOrCreate(Packets[Code] = PacketType);

		public void Send(ushort Code, object Value)
		{
			lock (WriteLock)
			{
				#region Finding Type IO
				var T = Value.GetType();
				var IO = TypeIOs.All[T];

				if (IO == null)
				{
					var CI = T.GetCollectionInterface();
					if (CI == null)
						throw new Exception("Value data type neither is not a collection or not defined");

					// Finding collection element type
					//var ET = CI.GetGenericArguments()[0];
					var ET = T.GetCollectionElementType();
					foreach (var CIO in TypeIOs.CollectionIOs)
						if (CIO.ListType == ET)
						{
							IO = CIO;
							break;
						}

					if (IO == null)
						throw new Exception("Collection element type is not defined");

					TypeIOs.AddComposite(T, IO);
				}
				#endregion

				WriteCode(Code);
				IO.Write(BW, Value);
				Flush();
			}
		}

		#region Fields
		private NetworkStream NS;
		private BinaryReader BR;
		private TcpClient Client;

		private readonly byte[] B;
		private readonly MemoryStream MS;
		private readonly BinaryWriter BW;

		private bool Closing;
		private readonly object WriteLock = new object();

		private readonly Thread T;
		#endregion

		#region Events
		public event Action<EasyTCP> OnClosed;
		private void fireClosed() => OnClosed?.Invoke(this);

		public event Action<EasyTCP, Exception> OnException;
		protected void fireException(Exception E) => OnException?.Invoke(this, E);

		public event dlgData OnData;
		private void fireData(ushort Code, object Value) => OnData?.Invoke(this, Code, Value);
		#endregion

		#region Essential Methods
		public void Connect(TcpClient Client, int VersionMajor, int VersionMinor)
		{
			this.Client = Client;
			Client.NoDelay = true;

			NS = Client.GetStream();
			BR = new BinaryReader(NS, Encoding.Unicode);
			var BW = new BinaryWriter(NS);

			BW.Write(VersionMajor);
			BW.Write(VersionMinor);

			var Major = BR.ReadInt32();
			BR.ReadInt32(); // Skip Minor

			if (Major != VersionMajor)
				throw new Exception("Version Mismatch");

			T.Start();
		}

		public void SendCloseRequest()
		{
			if (Closing) return;

			Closing = true;

			lock (WriteLock)
			{
				WriteCode(0xFFFF);
				Flush();
			}
		}

		public void Wait4Close(int MilliSeconds = 3000) { T.Join(MilliSeconds); }

		private void WriteCode(ushort Code)
		{
			BW.Write(Code);
			MS.Position += 4;
		}

		private void Flush()
		{
			var Len = (int)(MS.Position - 6);
			MS.Position = 2;
			BW.Write(Len);

			try
			{
				NS.Write(B, 0, Len + 6);
			}
			catch (Exception E)
			{
				Closing = true;
				fireException(E);
			}

			MS.Position = 0;
		}
		#endregion

		private void ReceiveData()
		{
			try
			{
				for (; ; )
				{
					ushort Code = BR.ReadUInt16();
					int Len = BR.ReadInt32();

					if (Code == 0xFFFF)
					{
						SendCloseRequest();

						Client.Close();
						fireClosed();
						return; // Exit the thread
					}

					if (Packets.TryGetValue(Code, out var PacketType))
						fireData(Code, TypeIOs.All[PacketType].Read(BR));
					else
						BR.ReadBytes(Len); // Skip Packet
				}
			}
			catch (Exception E) { fireException(E); }
		}
	}
}
