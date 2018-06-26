using System;
using System.IO;
using System.Windows;
using Microsoft.Win32;
using System.Net.Sockets;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

using EasyTCP;

namespace SimpleMessenger
{
	internal partial class MainWindow
	{
		private readonly MyEasyTCP TCP;

		public MainWindow(string Host, string Name)
		{
			InitializeComponent();

			TCP = new MyEasyTCP(Constants.BufferSize);
			TCP.OnData += TCP_OnData;
			TCP.OnClosed += TCP_OnClosed;

			TCP.Connect(new TcpClient(Host, PORT), 1, 0);

			TCP.Send(0, Name);

			Title = Name;

			Closed += (s, e) => TCP.SendCloseRequest();
		}

		private void TCP_OnClosed(EasyTCP.EasyTCP Sender) => Invoke(Close);

		private void TCP_OnData(EasyTCP.EasyTCP Sender, ushort Code, object Value)
		{
			switch (Code)
			{
			case 0:
				break;
			case 1:
				Invoke(() => TB.Inlines.Add(Value + Environment.NewLine));
				break;
			}
		}

		private const int PORT = 4987;

		private void Invoke(Action A) => Dispatcher.BeginInvoke(A);

		#region Event Handlers
		private void bSend_OnClick(object sender, RoutedEventArgs e)
		{
			TCP.Send(1, tbMsg.Text);
			tbMsg.Text = "";
		}

		private void bPic_OnClick(object sender, RoutedEventArgs e)
		{
		//	var OFD = new OpenFileDialog();
		//	if (OFD.ShowDialog() ?? false)
		//	{
		//		var BM = new BitmapImage();
		//		BM.BeginInit();
		//		BM.UriSource = new Uri(OFD.FileName, UriKind.Absolute);
		//		BM.CacheOption = BitmapCacheOption.OnLoad;
		//		BM.EndInit();

		//		TB.Inlines.Add(new Image { Source = BM });
		//		TB.Inlines.Add(Environment.NewLine);

		//		var E = new PngBitmapEncoder();
		//		E.Frames.Add(BitmapFrame.Create(BM));
		//		using (MemoryStream Mem = new MemoryStream())
		//		{
		//			E.Save(Mem);
		//			var B = Mem.ToArray();
		//			MS.SendPicture(new ByteArray(B, 0, B.Length));
		//		}
		//	}
		}

		//private void MyStream_OnPicture(MyStream Sender, ByteArray Data)
		//{
		//	Invoke(() =>
		//	{
		//		using (var Mem = new MemoryStream(Data.B, false))
		//		{
		//			var BM = new BitmapImage();
		//			BM.BeginInit();
		//			BM.StreamSource = Mem;
		//			BM.CacheOption = BitmapCacheOption.OnLoad;
		//			BM.EndInit();

		//			TB.Inlines.Add(new Image { Source = BM });
		//			TB.Inlines.Add(Environment.NewLine);
		//		}
		//	});
		//}
		#endregion
	}
}
