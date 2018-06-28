using System;
using System.Net;
using System.Net.Sockets;
using System.Windows.Media;
using System.Windows.Threading;
using System.Windows.Documents;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace SimpleMessenger
{
	internal partial class ServerWindow
	{
		private readonly ConcurrentDictionary<EasyTCP.EasyTCP, string> Dic = new ConcurrentDictionary<EasyTCP.EasyTCP, string>();

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

		private void SendMessage(EasyTCP.EasyTCP Exclude, string Msg)
		{
			Do4All(MS => MS.Send(1, Msg), Exclude);
			Invoke(() => TB.Inlines.Add(Msg + Environment.NewLine));
		}

		private void Do4All(Action<EasyTCP.EasyTCP> Act, EasyTCP.EasyTCP Exclude = null)
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
					var MyStream = new MyEasyTCP(Constants.BufferSize);
					MyStream.OnData += MyStream_OnInfo;
					MyStream.OnClosed += MyStream_OnClosed;

					MyStream.Connect(Listener.AcceptTcpClient(), 1, 0);
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

		private void MyStream_OnClosed(EasyTCP.EasyTCP S)
		{
			SendMessage(S, Dic[S] + " left");
			Dic.TryRemove(S, out _);
		}

		private void MyStream_OnInfo(EasyTCP.EasyTCP Sender, ushort Code, object Value)
		{
			string Message;
			switch (Code)
			{
			case 0:
				SendMessage(Sender, (Dic[Sender] = Value.ToString()) + " joined");
				return;

			case 2:
				Message = string.Join(" ", Value as IEnumerable<int>);
				break;

			case 1:
			case 3:
			case 4:
				Message = Value.ToString();
				break;

			case 5:
				Do4All(TCP => TCP.Send(5, Value), Sender);
				return;

			default: return;
			}

			SendMessage(Sender, string.Format("{0}> {1}", Dic[Sender], Message));
		}
		#endregion
	}
}
