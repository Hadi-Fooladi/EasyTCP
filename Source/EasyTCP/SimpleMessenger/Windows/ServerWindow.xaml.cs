using System;
using System.Net;
using System.Net.Sockets;
using System.Windows.Media;
using System.Windows.Threading;
using System.Windows.Documents;
using System.Collections.Concurrent;

namespace SimpleMessenger
{
	internal partial class ServerWindow
	{
		private readonly ConcurrentDictionary<MyStream, string> Dic = new ConcurrentDictionary<MyStream, string>();

		public ServerWindow()
		{
			InitializeComponent();

			Listener = new TcpListener(IPAddress.Any, PORT);
			Listener.Start();

			var ListenerTimer = new DispatcherTimer
			{
				IsEnabled = true,
				Interval = TimeSpan.FromSeconds(.05)
			};
			ListenerTimer.Tick += ListenerTimer_Tick;

			Closed += ServerWindow_Closed;
		}

		private void ServerWindow_Closed(object sender, EventArgs e)
		{
			foreach (var MS in Dic.Keys)
			{
				MS.OnClosed -= MyStream_OnClosed;
				MS.SendCloseRequest();
			}

			foreach (var MS in Dic.Keys)
				MS.Wait4Close();
		}

		private const int PORT = 4987;

		private readonly TcpListener Listener;

		private void Invoke(Action A) => Dispatcher.BeginInvoke(A);

		private void SendMessage(MyStream Exclude, string Msg)
		{
			Do4All(MS => MS.SendMessage(Msg), Exclude);
			Invoke(() => TB.Inlines.Add(Msg + Environment.NewLine));
		}

		private void Do4All(Action<MyStream> Act, MyStream Exclude = null)
		{
			foreach (var MS in Dic.Keys)
				if (MS != Exclude)
					Act(MS);
		}

		#region Event Handlers
		private void ListenerTimer_Tick(object sender, EventArgs e)
		{
			if (Listener.Pending())
				try
				{
					var MyStream = new MyStream(Constants.BufferSize);
					MyStream.OnInfo += MyStream_OnInfo;
					MyStream.OnClosed += MyStream_OnClosed;
					MyStream.OnMessage += MyStream_OnMessage;
					MyStream.OnPicture += MyStream_OnPicture;
					MyStream.Connect(Listener.AcceptTcpClient());
				}
				catch (Exception E)
				{
					var R = new Run
					{
						Foreground = Brushes.Red,
						Text = "Error: " + E.Message + Environment.NewLine
					};
					TB.Inlines.Add(R);
				}
		}

		private void MyStream_OnPicture(MyStream Sender, ByteArray Data) => Do4All(MS => MS.SendPicture(Data), Sender);

		private void MyStream_OnClosed(MyStream Sender)
		{
			SendMessage(Sender, Dic[Sender] + " left");
			Dic.TryRemove(Sender, out var _);
		}

		private void MyStream_OnInfo(MyStream Sender, string Name) => SendMessage(Sender, (Dic[Sender] = Name) + " joined");
		private void MyStream_OnMessage(MyStream Sender, string Message) => SendMessage(Sender, string.Format("{0}> {1}", Dic[Sender], Message));
		#endregion
	}
}
