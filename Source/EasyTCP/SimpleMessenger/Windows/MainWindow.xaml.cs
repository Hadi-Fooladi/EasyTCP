using System;
using System.Windows;
using System.Net.Sockets;
using System.Collections.Generic;

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
			if (Code == 1)
				Invoke(() => TB.Inlines.Add(Value + Environment.NewLine));
		}

		private const int PORT = 4987;

		private void Invoke(Action A) => Dispatcher.BeginInvoke(A);

		private static int Random3Digits => Global.Rnd.Next(1, 1000);

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

		private void bRandom_OnClick(object sender, RoutedEventArgs e)
		{
			int i, n = Global.Rnd.Next(2, 6);
			var L = new List<int>();

			for (i = 0; i < n; i++)
				L.Add(Random3Digits);

			//var L = new int[n];

			//for (i = 0; i < n; i++)
			//	L[i] = Random3Digits;

			TCP.Send(2, L);
		}

		private void bVector_OnClick(object sender, RoutedEventArgs e)
			=> TCP.Send(3, new Vector { x = Random3Digits, y = Random3Digits, z = Random3Digits });

		private void bPerson_OnClick(object sender, RoutedEventArgs e) => TCP.Send(4, new Person(100));
		#endregion
	}
}
