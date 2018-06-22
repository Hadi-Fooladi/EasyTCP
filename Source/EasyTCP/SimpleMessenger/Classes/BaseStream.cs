// Use with 'EasyTCP (v6.0.22)'

using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Net.Sockets;

namespace EasyTCP
{
	internal abstract class BaseStream
	{
		protected BaseStream(int WriteBufferLength = 65536)
		{
			B = new byte[WriteBufferLength];
			MS = new MemoryStream(B);
			BW = new BinaryWriter(MS, Encoding.Unicode);
		}

		#region Fields
		private NetworkStream NS;
		protected BinaryReader BR;
		protected TcpClient Client;

		private readonly byte[] B;
		private readonly MemoryStream MS;
		protected readonly BinaryWriter BW;

		protected bool Closing;
		private readonly Mutex M = new Mutex();

		protected object WriteLock = new object();
		#endregion

		protected abstract Thread T { get; }
		protected abstract Version Version { get; }

		#region Events
		public event Action<BaseStream>  OnClosed;
		protected void fireClosed() { OnClosed.Fire(this); }

		public event Action<BaseStream, Exception> OnException;
		protected void fireException(Exception E) { OnException.Fire(this, E); }
		#endregion

		#region Essential Methods
		public void Connect(TcpClient Client)
		{
			this.Client = Client;
			Client.NoDelay = true;

			NS = Client.GetStream();
			BR = new BinaryReader(NS, Encoding.Unicode);
			var BW = new BinaryWriter(NS);

			BW.Write(Version.Major);
			BW.Write(Version.Minor);

			var Major = BR.ReadInt32();
			BR.ReadInt32(); // Skip Minor

			if (Major != Version.Major)
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

		protected void WriteCode(ushort Code)
		{
			BW.Write(Code);
			MS.Position += 4;
		}

		protected void Flush()
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
	}
}
