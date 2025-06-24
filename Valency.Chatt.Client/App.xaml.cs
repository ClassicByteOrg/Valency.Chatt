using System.IO;
using System.Windows;
using Valency.Chatt.Core;
using static Valency.Chatt.Core.Util;

namespace Valency.Chatt.Client
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		public App()
		{
		}

		protected override void OnStartup(StartupEventArgs e)
		{
			InitializeWorkspace();

			new MainWindow().Show();

			base.OnStartup(e);
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