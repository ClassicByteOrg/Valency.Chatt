using System.IO;
using System.Windows;
using static Valency.Chatt.Core.Util;

namespace Valency.Chatt.Client
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		internal static WebSocketClient w = new();
		public App()
		{
			w.MessageReceived += W_MessageReceived;
			w.ErrorOccurred += W_ErrorOccurred;
			w.Closed += W_Closed;
			w.Connected += W_Connected;
		}

		private void W_Connected(object? sender, EventArgs e)
		{
			// 可在此处弹窗或写日志
		}

		private void W_Closed(object? sender, WebSocketCloseEventArgs e)
		{
			// 可在此处弹窗或写日志
		}

		private void W_ErrorOccurred(object? sender, WebSocketErrorEventArgs e)
		{
			// 可在此处弹窗或写日志
		}

		private void W_MessageReceived(object? sender, string e)
		{
			// 由 MainWindow 订阅处理
		}

		protected override void OnStartup(StartupEventArgs e)
		{
			InitializeWorkspace();
			base.OnStartup(e);
			new LoginWindow().Show();
		}

		static void InitializeWorkspace()
		{
			if (!Workspace.Exists)
			{
				try
				{
					Workspace.Create();
				}
				catch (IOException)
				{
					throw;
				}
			}

			if (!Util.GetAllUserDir().Exists)
			{
				try
				{
					Util.GetAllUserDir().Create();
				}
				catch (IOException)
				{
					throw;
				}
			}
		}
	}
}