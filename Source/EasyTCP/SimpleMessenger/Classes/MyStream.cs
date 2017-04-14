// auto-generated by EasyTCP (v2.0.6)

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
		public static readonly Version Version = new Version(1, 0);

		public MyStream()
		{
			MS = new MemoryStream(B);
			BW = new BinaryWriter(MS, Encoding.Unicode);

			T = new Thread(ReceiveData);
		}

		#region Fields
		/// <summary>
		/// Amount of sleep time if no data exist (in millisecond)
		/// </summary>
		public int Sleep4Data = 100;

		private BinaryReader BR;
		private TcpClient Client;
		private NetworkStream NS;

		private readonly MemoryStream MS;
		private readonly BinaryWriter BW;
		private readonly byte[] B = new byte[65536];

		private readonly Thread T;
		private bool Closing, Running = true;
		#endregion

		#region Delegates
		public delegate void dlg(MyStream Sender);
		public delegate void dlgInfo(MyStream Sender, string Name);
		public delegate void dlgMessage(MyStream Sender, string Message);
		public delegate void dlgPicHeader(MyStream Sender, long id, int SegmentCount);
		public delegate void dlgPicSegment(MyStream Sender, long id, int Num, ByteArray Data);
		#endregion

		#region Events
		public event dlg OnClosed;
		public event dlgInfo OnInfo;
		public event dlgMessage OnMessage;
		public event dlgPicHeader OnPicHeader;
		public event dlgPicSegment OnPicSegment;

		protected void fireClosed() { if (OnClosed != null) OnClosed(this); }
		protected void fireInfo(string Name) { if (OnInfo != null) OnInfo(this, Name); }
		protected void fireMessage(string Message) { if (OnMessage != null) OnMessage(this, Message); }
		protected void firePicHeader(long id, int SegmentCount) { if (OnPicHeader != null) OnPicHeader(this, id, SegmentCount); }
		protected void firePicSegment(long id, int Num, ByteArray Data) { if (OnPicSegment != null) OnPicSegment(this, id, Num, Data); }
		#endregion

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

		public void ForceClose() { Running = false; T.Join(); }

		private void WriteCode(ushort Code)
		{
			BW.Write(Code);
			MS.Position += 2;
		}

		private void Flush()
		{
			var Len = (ushort)(MS.Position - 4);
			MS.Position = 2;
			BW.Write(Len);
			NS.Write(B, 0, Len + 4);
			MS.Position = 0;
		}

		public void SendInfo(string Name)
		{
			WriteCode(0);
			BW.Write(Name);
			Flush();
		}

		public void SendMessage(string Message)
		{
			WriteCode(1);
			BW.Write(Message);
			Flush();
		}

		public void SendPicHeader(long id, int SegmentCount)
		{
			WriteCode(2);
			BW.Write(id);
			BW.Write(SegmentCount);
			Flush();
		}

		public void SendPicSegment(long id, int Num, ByteArray Data)
		{
			WriteCode(3);
			BW.Write(id);
			BW.Write(Num);
			BW.Write(Data);
			Flush();
		}

		private void ReceiveData()
		{
			while (Running)
			{
				if (Client.Available <= 0)
				{
					Thread.Sleep(Sleep4Data);
					continue;
				}

				ushort
					Code = BR.ReadUInt16(),
					Len = BR.ReadUInt16();

				switch (Code)
				{
				case 0xFFFF:
					SendCloseRequest();

					Client.Close();
					fireClosed();
					return; // Exit the thread

				case 0: // Info
				{
					var oName = BR.ReadString();

					fireInfo(oName);
					break;
				}

				case 1: // Message
				{
					var oMessage = BR.ReadString();

					fireMessage(oMessage);
					break;
				}

				case 2: // PicHeader
				{
					var oid = BR.ReadInt64();
					var oSegmentCount = BR.ReadInt32();

					firePicHeader(oid, oSegmentCount);
					break;
				}

				case 3: // PicSegment
				{
					var oid = BR.ReadInt64();
					var oNum = BR.ReadInt32();
					var oData = new ByteArray(BR);

					firePicSegment(oid, oNum, oData);
					break;
				}

				default:
					BR.ReadBytes(Len); // Skip Packet
					break;
				}
			}
		}
	}
}
