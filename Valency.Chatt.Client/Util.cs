using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Valency.Chatt.Core.Util;

namespace Valency.Chatt.Client
{
	internal class Util
	{
		public const string AllUserDBName = "AllUser.db";

		public static DirectoryInfo GetAllUserDir()
		{
			return new(Path.Combine(Workspace.FullName, "Users"));
		}

		public static FileInfo GetAllUserDB()
		{
			return new FileInfo(Path.Combine(GetAllUserDir().FullName,AllUserDBName));
		}
	}
}
