using Valency.Chatt.Core;
using static Valency.Chatt.Client.Util;

namespace Valency.Chatt.Client
{
	internal class UserManager
	{
		public static List<UserManager>? GetAllUserManagers()
		{
			if (!GetAllUserDB().Exists)
			{
				return null;
			}
			else
			{
				return null;
			}
		}

		public User User { get; set; }
	}
}
