using System.Diagnostics;
using System.Net.WebSockets;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Valency.Chatt.Client
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		WebSocketClient w = new WebSocketClient();
		public MainWindow()
		{

			InitializeComponent();
			this.Loaded += MainWindow_Loaded;
			w.MessageReceived += W_MessageReceived;
		}

		private void W_MessageReceived(object? sender, string e)
		{
			MessageBox.Show(e);
		}

		private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
		{
			 await w.ConnectAsync("ws://132.232.242.101:54258");
		}

		private async void Button_Click(object sender, RoutedEventArgs e)
		{
		 	await w.SendMessageAsync(SendMessageBox.Text);
		}
	}

}