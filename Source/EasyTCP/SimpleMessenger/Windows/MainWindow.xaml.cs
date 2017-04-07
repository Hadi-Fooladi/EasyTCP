using System;
using System.Windows;
using System.Threading;
using System.Net.Sockets;

namespace SimpleMessenger
{
	internal partial class MainWindow
	{
		private readonly MyStream MS;

		public MainWindow(string Host, string Name)
		{
			InitializeComponent();

			var C = new TcpClient(Host, PORT);
			MS = new MyStream(C);
			MS.OnClosed += MyStream_OnClosed;
			MS.OnMessage += MyStream_OnMessage;

			Thread.Sleep(100);
			MS.SendInfo(Name);

			Title = Name;

			Closed += (s, e) => MS.SendCloseRequest();
		}

		private const int PORT = 4987;

		#region Event Handlers
		private void MyStream_OnClosed(MyStream Sender) => Dispatcher.Invoke(Close);
		private void MyStream_OnMessage(MyStream Sender, string Message) => Dispatcher.Invoke(() => TB.Inlines.Add(Message + Environment.NewLine));

		private void bSend_OnClick(object sender, RoutedEventArgs e)
		{
			MS.SendMessage(tbMsg.Text);
			tbMsg.Text = "";
		}
		#endregion
	}
}
