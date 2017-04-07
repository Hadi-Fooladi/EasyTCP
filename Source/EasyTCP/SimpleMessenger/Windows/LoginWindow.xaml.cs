using System;
using System.Windows;

namespace SimpleMessenger
{
	internal partial class LoginWindow
	{
		public LoginWindow()
		{
			InitializeComponent();
		}

		private void Show(Window W)
		{
			Hide();
			W.ShowDialog();
			Close();
		}

		private void bServer_OnClick(object sender, RoutedEventArgs e) => Show(new ServerWindow());
		private void bClient_OnClick(object sender, RoutedEventArgs e) => Show(new MainWindow(tbHost.Text, tbName.Text));
	}
}
