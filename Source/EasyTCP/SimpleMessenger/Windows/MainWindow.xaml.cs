using System;
using System.IO;
using System.Windows;
using Microsoft.Win32;
using System.Net.Sockets;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace SimpleMessenger
{
	internal partial class MainWindow
	{
		private readonly MyStream MS;

		public MainWindow(string Host, string Name)
		{
			InitializeComponent();

			MS = new MyStream(Constants.BufferSize);
			MS.OnClosed += MyStream_OnClosed;
			MS.OnMessage += MyStream_OnMessage;
			MS.OnPicture += MyStream_OnPicture;

			MS.Connect(new TcpClient(Host, PORT));

			MS.SendInfo(Name);

			Title = Name;

			Closed += (s, e) => MS.SendCloseRequest();
		}

		private const int PORT = 4987;

		private void Invoke(Action A) => Dispatcher.BeginInvoke(A);

		#region Event Handlers
		private void MyStream_OnClosed(MyStream Sender) => Invoke(Close);
		private void MyStream_OnMessage(MyStream Sender, string Message) => Invoke(() => TB.Inlines.Add(Message + Environment.NewLine));

		private void bSend_OnClick(object sender, RoutedEventArgs e)
		{
			MS.SendMessage(tbMsg.Text);
			tbMsg.Text = "";
		}

		private void bPic_OnClick(object sender, RoutedEventArgs e)
		{
			var OFD = new OpenFileDialog();
			if (OFD.ShowDialog() ?? false)
			{
				var BM = new BitmapImage();
				BM.BeginInit();
				BM.UriSource = new Uri(OFD.FileName, UriKind.Absolute);
				BM.CacheOption = BitmapCacheOption.OnLoad;
				BM.EndInit();

				TB.Inlines.Add(new Image { Source = BM });
				TB.Inlines.Add(Environment.NewLine);

				var E = new PngBitmapEncoder();
				E.Frames.Add(BitmapFrame.Create(BM));
				using (MemoryStream Mem = new MemoryStream())
				{
					E.Save(Mem);
					var B = Mem.ToArray();
					MS.SendPicture(new ByteArray(B, 0, B.Length));
				}
			}
		}

		private void MyStream_OnPicture(MyStream Sender, ByteArray Data)
		{
			Invoke(() =>
			{
				using (var Mem = new MemoryStream(Data.B, false))
				{
					var BM = new BitmapImage();
					BM.BeginInit();
					BM.StreamSource = Mem;
					BM.CacheOption = BitmapCacheOption.OnLoad;
					BM.EndInit();

					TB.Inlines.Add(new Image { Source = BM });
					TB.Inlines.Add(Environment.NewLine);
				}
			});
		}
		#endregion
	}
}
