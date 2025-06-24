using System.Text.Json;

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

	public static string ToJson<T>(T obj, JsonSerializerOptions? options = null)
	{
		return JsonSerializer.Serialize(obj, options);
	}

	public static T? FromJson<T>(string json, JsonSerializerOptions? options = null)
	{
		return JsonSerializer.Deserialize<T>(json, options);
	}
	public static string Sha512(string input)
	{
		using var sha512 = System.Security.Cryptography.SHA512.Create();
		byte[] bytes = System.Text.Encoding.UTF8.GetBytes(input);
		byte[] hash = sha512.ComputeHash(bytes);
		return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
	}

}
