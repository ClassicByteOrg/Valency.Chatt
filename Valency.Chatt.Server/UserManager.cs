using Microsoft.Data.Sqlite;
using Valency.Chatt.Core;

namespace Valency.Chatt.Server
{
	internal class UserManager
	{
		private static readonly string DbPath = Path.Combine(Path.Combine(Directory.CreateDirectory(Directory.GetCurrentDirectory() + "/data/").FullName), "UserInfo.DB");
		private static readonly string ConnectionString = $"Data Source={DbPath}";

		static UserManager()
		{
			InitializeDatabase();
		}

		private static void InitializeDatabase()
		{
			if (!File.Exists(DbPath))
			{
				using var connection = new SqliteConnection(ConnectionString);
				connection.Open();
				var cmd = connection.CreateCommand();
				cmd.CommandText = @"
				CREATE TABLE Users (
					Id INTEGER PRIMARY KEY,
					Name TEXT NOT NULL UNIQUE,
					Email TEXT NOT NULL,
					PasswordHash TEXT NOT NULL
				);
				CREATE TABLE UserFriends (
					UserId INTEGER NOT NULL,
					FriendId INTEGER NOT NULL,
					PRIMARY KEY (UserId, FriendId),
					FOREIGN KEY (UserId) REFERENCES Users(Id),
					FOREIGN KEY (FriendId) REFERENCES Users(Id)
				);
				";
				cmd.ExecuteNonQuery();
			}
		}

		public static void AddUser(User user)
		{
			using var connection = new SqliteConnection(ConnectionString);
			connection.Open();
			var cmd = connection.CreateCommand();
			cmd.CommandText = "INSERT INTO Users (Id, Name, Email, PasswordHash) VALUES (@Id, @Name, @Email, @PasswordHash)";
			cmd.Parameters.AddWithValue("@Id", user.Id);
			cmd.Parameters.AddWithValue("@Name", user.Name);
			cmd.Parameters.AddWithValue("@Email", user.Email);
			cmd.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);
			cmd.ExecuteNonQuery();
		}

		public static User? GetUser(int id)
		{
			using var connection = new SqliteConnection(ConnectionString);
			connection.Open();
			var cmd = connection.CreateCommand();
			cmd.CommandText = "SELECT Id, Name, Email, PasswordHash FROM Users WHERE Id = @Id";
			cmd.Parameters.AddWithValue("@Id", id);
			using var reader = cmd.ExecuteReader();
			if (reader.Read())
			{
				var user = new User
				{
					Id = reader.GetInt32(0),
					Name = reader.GetString(1),
					Email = reader.GetString(2),
					PasswordHash = reader.GetString(3),
					Friends = GetFriends(reader.GetInt32(0))
				};
				return user;
			}
			return null;
		}

		public static User? GetUserByName(string name)
		{
			using var connection = new SqliteConnection(ConnectionString);
			connection.Open();
			var cmd = connection.CreateCommand();
			cmd.CommandText = "SELECT Id, Name, Email, PasswordHash FROM Users WHERE Name = @Name";
			cmd.Parameters.AddWithValue("@Name", name);
			using var reader = cmd.ExecuteReader();
			if (reader.Read())
			{
				var user = new User
				{
					Id = reader.GetInt32(0),
					Name = reader.GetString(1),
					Email = reader.GetString(2),
					PasswordHash = reader.GetString(3),
					Friends = GetFriends(reader.GetInt32(0))
				};
				return user;
			}
			return null;
		}

		public static int GetNextUserId()
		{
			using var connection = new SqliteConnection(ConnectionString);
			connection.Open();
			var cmd = connection.CreateCommand();
			cmd.CommandText = "SELECT IFNULL(MAX(Id), 0) + 1 FROM Users";
			return Convert.ToInt32(cmd.ExecuteScalar());
		}

		public static List<User> GetFriends(int userId)
		{
			var friends = new List<User>();
			using var connection = new SqliteConnection(ConnectionString);
			connection.Open();
			var cmd = connection.CreateCommand();
			cmd.CommandText = @"SELECT u.Id, u.Name, u.Email, u.PasswordHash FROM Users u
				JOIN UserFriends f ON u.Id = f.FriendId WHERE f.UserId = @UserId";
			cmd.Parameters.AddWithValue("@UserId", userId);
			using var reader = cmd.ExecuteReader();
			while (reader.Read())
			{
				friends.Add(new User
				{
					Id = reader.GetInt32(0),
					Name = reader.GetString(1),
					Email = reader.GetString(2),
					PasswordHash = reader.GetString(3)
				});
			}
			return friends;
		}

		public static void AddFriend(int userId, int friendId)
		{
			using var connection = new SqliteConnection(ConnectionString);
			connection.Open();
			var cmd = connection.CreateCommand();
			cmd.CommandText = "INSERT OR IGNORE INTO UserFriends (UserId, FriendId) VALUES (@UserId, @FriendId)";
			cmd.Parameters.AddWithValue("@UserId", userId);
			cmd.Parameters.AddWithValue("@FriendId", friendId);
			cmd.ExecuteNonQuery();
		}

		public static void RemoveFriend(int userId, int friendId)
		{
			using var connection = new SqliteConnection(ConnectionString);
			connection.Open();
			var cmd = connection.CreateCommand();
			cmd.CommandText = "DELETE FROM UserFriends WHERE UserId = @UserId AND FriendId = @FriendId";
			cmd.Parameters.AddWithValue("@UserId", userId);
			cmd.Parameters.AddWithValue("@FriendId", friendId);
			cmd.ExecuteNonQuery();
		}
	}
}
