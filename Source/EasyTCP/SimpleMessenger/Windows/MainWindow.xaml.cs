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

			MS = new MyStream();
			MS.OnClosed += MyStream_OnClosed;
			MS.OnMessage += MyStream_OnMessage;
			MS.OnPicHeader += MS_OnPicHeader;
			MS.OnPicSegment += MS_OnPicSegment;

			MS.Connect(new TcpClient(Host, PORT));

			MS.SendInfo(Name);

			Title = Name;

			Closed += (s, e) => MS.SendCloseRequest();
		}

		private const int PORT = 4987;

		private MemoryStream Mem;
		private BinaryWriter BW;
		private int SegmentCount;

		#region Event Handlers
		private void MyStream_OnClosed(MyStream Sender) => Dispatcher.Invoke(Close);
		private void MyStream_OnMessage(MyStream Sender, string Message) => Dispatcher.Invoke(() => TB.Inlines.Add(Message + Environment.NewLine));

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
				//var img = new Image();
				var BM = new BitmapImage();
				BM.BeginInit();
				BM.UriSource = new Uri(OFD.FileName, UriKind.Absolute);
				BM.CacheOption = BitmapCacheOption.OnLoad;
				BM.EndInit();
				//img.Source = BM;
				//TB.Inlines.Add(img);
				//TB.Inlines.Add(Environment.NewLine);

				var E = new PngBitmapEncoder();
				E.Frames.Add(BitmapFrame.Create(BM));
				using (MemoryStream ms = new MemoryStream())
				{
					E.Save(ms);
					var B = ms.ToArray();

					const ushort MAX = 65000;
					int
						i, n = B.Length,
						SegmentCount = (int)Math.Ceiling(n / (double)MAX);

					MS.SendPicHeader(0, SegmentCount);
					for (i = 0; i < SegmentCount; i++)
					{
						var BA = new ByteArray(B, i * MAX, (ushort)Math.Min(MAX, n - i * MAX));
						MS.SendPicSegment(0, i, BA);
					}
				}
			}
		}

		private void MS_OnPicSegment(MyStream Sender, long id, int Num, ByteArray Data)
		{
			BW.Write(Data.B);
			if (Num == SegmentCount - 1)
			{
				Dispatcher.Invoke(() =>
				{
					var img = new Image();
					var BM = new BitmapImage();
					BM.BeginInit();
					BM.StreamSource = Mem;
					BM.CacheOption = BitmapCacheOption.OnLoad;
					BM.EndInit();
					img.Source = BM;
					TB.Inlines.Add(img);
					TB.Inlines.Add(Environment.NewLine);
				});
				Mem.Close();
			}
		}

		private void MS_OnPicHeader(MyStream Sender, long id, int SegmentCount)
		{
			Mem = new MemoryStream();
			BW = new BinaryWriter(Mem);
			this.SegmentCount = SegmentCount;
		}
		#endregion
	}
}
