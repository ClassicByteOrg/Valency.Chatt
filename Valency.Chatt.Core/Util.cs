namespace Valency.Chatt.Core;

public class Util
{
	private static DirectoryInfo? workspace;
	public static DirectoryInfo Workspace
	{
		get
		{
			workspace = new DirectoryInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".Valency", ".Chatt"));
			return workspace;
		}
		set
		{
			workspace = value;
		}
	}
}
