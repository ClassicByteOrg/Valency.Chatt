using System.Collections.ObjectModel;
using System.Windows;
using Valency.Chatt.Core;

namespace Valency.Chatt.Client
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private ObservableCollection<string> _messages = new();

		public MainWindow()
		{
			InitializeComponent();
			MessageHistory.ItemsSource = _messages;
			Loaded += MainWindow_Loaded;
		}

		private void MainWindow_Loaded(object sender, RoutedEventArgs e)
		{
			App.w.MessageReceived += W_MessageReceived;
		}

		private async void Button_Click(object sender, RoutedEventArgs e)
		{
			var text = SendMessageBox.Text.Trim();
			if (!string.IsNullOrEmpty(text))
			{
				var msg = new Message { MessageType = MessageType.SendMessage, Content = text };
				await App.w.SendMessageAsync(Valency.Chatt.Core.Util.ToJson(msg));
				_messages.Add($"我: {text}");
				SendMessageBox.Clear();
			}
		}

		private void W_MessageReceived(object? sender, string e)
		{
			try
			{
				var msg = Valency.Chatt.Core.Util.FromJson<Message>(e);
				if (msg.MessageType == MessageType.MessageReceived || msg.MessageType == MessageType.SendMessage)
				{
					Dispatcher.Invoke(() => _messages.Add($"对方: {msg.Content}"));
				}
				else
				{
					Dispatcher.Invoke(() => _messages.Add($"系统: {msg.Content}"));
				}
			}
			catch
			{
				Dispatcher.Invoke(() => _messages.Add($"对方: {e}"));
			}
		}
	}
}