// auto-generated by EasyTCP (v5.4.20)

using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using System.Collections.Generic;

namespace SimpleMessenger
{
	internal class MyStream
	{
		public static readonly Version Version = new Version("2.0");

		public MyStream(int WriteBufferLength = 65536)
		{
			B = new byte[WriteBufferLength];
			MS = new MemoryStream(B);
			BW = new BinaryWriter(MS, Encoding.Unicode);

			T = new Thread(ReceiveData) { IsBackground = true };
		}

		#region Fields
		[Obsolete]
		public int Sleep4Data;

		private BinaryReader BR;
		private TcpClient Client;
		private NetworkStream NS;

		private readonly byte[] B;
		private readonly MemoryStream MS;
		private readonly BinaryWriter BW;

		private bool Closing;
		private readonly Thread T;
		private readonly Mutex M = new Mutex();
		#endregion

		#region Delegates
		public delegate void dlg(MyStream Sender);
		public delegate void dlgException(MyStream Sender, Exception E);

		public delegate void dlgInfo(MyStream Sender, string Name);
		public delegate void dlgMessage(MyStream Sender, string Message);
		public delegate void dlgPicture(MyStream Sender, ByteArray Data);
		#endregion

		#region Events
		public event dlg OnClosed;
		public event dlgException OnException;
		public event dlgInfo OnInfo;
		public event dlgMessage OnMessage;
		public event dlgPicture OnPicture;

		protected void fireException(Exception E) { if (OnException != null) OnException(this, E); }
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
			WriteCode(0xFFFF);
			Flush();
		}

		public void Wait4Close(int MilliSeconds = 3000) { T.Join(MilliSeconds); }

		[Obsolete]
		public void ForceClose() { }

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

		private void ExclusiveRun(Action A)
		{
			try { M.WaitOne(); A(); }
			finally { M.ReleaseMutex(); }
		}
		#endregion

		#region Send Methods
		public void SendInfo(string Name)
		{
			if (Closing) return;

			WriteCode(0);
			BW.Write(Name);
			Flush();
		}

		public void SendMessage(string Message)
		{
			if (Closing) return;

			WriteCode(1);
			BW.Write(Message);
			Flush();
		}

		public void SendPicture(ByteArray Data)
		{
			if (Closing) return;

			WriteCode(2);
			BW.Write(Data);
			Flush();
		}
		#endregion

		#region SyncSend Methods
		public void SyncSendInfo(string Name) => ExclusiveRun(() => SendInfo(Name));

		public void SyncSendMessage(string Message) => ExclusiveRun(() => SendMessage(Message));

		public void SyncSendPicture(ByteArray Data) => ExclusiveRun(() => SendPicture(Data));
		#endregion

		private void ReceiveData()
		{
			try
			{
				for (;;)
				{
					ushort Code = BR.ReadUInt16();
					int Len = BR.ReadInt32();

					switch (Code)
					{
					case 0xFFFF:
						SendCloseRequest();

						Client.Close();
						OnClosed?.Invoke(this);
						return; // Exit the thread

					case 0: // Info
					{
						string oName;

						BR.Read(out oName);

						OnInfo?.Invoke(this, oName);
						break;
					}

					case 1: // Message
					{
						string oMessage;

						BR.Read(out oMessage);

						OnMessage?.Invoke(this, oMessage);
						break;
					}

					case 2: // Picture
					{
						ByteArray oData;

						BR.Read(out oData);

						OnPicture?.Invoke(this, oData);
						break;
					}

					default:
						BR.ReadBytes(Len); // Skip Packet
						break;
					}
				}
			}
			catch (Exception E)
			{
				fireException(E);
			}
		}
	}
}
