using System.Windows;
using Valency.Chatt.Core;
using Valency.Chatt.Core.MessageImps;
using static Valency.Chatt.Core.Util;

namespace Valency.Chatt.Client
{
	/// <summary>
	/// LoginWindow.xaml 的交互逻辑
	/// </summary>
	public partial class LoginWindow : Window
	{
		public LoginWindow()
		{
			InitializeComponent();
		}

		//Login
		private async void Button_Click(object sender, RoutedEventArgs e)
		{
			await App.w.SendMessageAsync(ToJson(new Message()
			{
				Content = ToJson(new LoginAndRegisterMessage()
				{
					Name = this.UserNameBox.Text,
					PasswordHash = Sha512(PasswordInputBox.Password)
				}),
				MessageType = MessageType.Login
			}));

		}

		//Register
		private async void Button_Click_1(object sender, RoutedEventArgs e)
		{
			await App.w.SendMessageAsync(ToJson(new Message()
			{
				Content = ToJson(new LoginAndRegisterMessage()
				{
					Name = this.UserNameBox.Text,
					PasswordHash = Sha512(PasswordInputBox.Password)
				}),
				MessageType = MessageType.Login
			}));

		}
	}
}
