namespace Valency.Chatt.Core
{
	public class User
	{
		public required int Id { get; set; }
		public required string Name { get; set; }

		public required string Email { get; set; }

		public required string PasswordHash { get; set; }

		public List<User>? Friends { get; set; }

		public User() { }
	}
}
