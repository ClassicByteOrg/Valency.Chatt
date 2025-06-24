using System.Diagnostics;
using Valency.Chatt.Core;
using Valency.Chatt.Server;

namespace Valency.Chatt.Server;

class Program
{
	static WebSocketServer webSocketServer = new WebSocketServer(54258);
	// 用户名到客户端ID的映射
	static Dictionary<string, string> userClientMap = new();
	// 客户端ID到用户名的映射
	static Dictionary<string, string> clientUserMap = new();

	static async Task Main()
	{
		Console.CancelKeyPress += (s, e) =>
		{
			Console.WriteLine("'Ctrl+C'pressed,Server is closing...");
			webSocketServer.Stop();
		};
		webSocketServer.ClientConnected += WebSocketServer_ClientConnected;
		webSocketServer.ClientDisconnected += WebSocketServer_ClientDisconnected;
		webSocketServer.ErrorOccurred += WebSocketServer_ErrorOccurred;
		webSocketServer.MessageReceived += WebSocketServer_MessageReceived;
		await webSocketServer.StartAsync();
		Console.WriteLine("Server Running...");
	}

	private static async void WebSocketServer_MessageReceived(object? sender, WebSocketMessageEventArgs e)
	{
		if (string.IsNullOrEmpty(e.ClientId) || string.IsNullOrEmpty(e.Message)) return;
		var msg = Util.FromJson<Message>(e.Message);
		switch (msg.MessageType)
		{
			case MessageType.Login:
				var login = Util.FromJson<Valency.Chatt.Core.MessageImps.LoginAndRegisterMessage>(msg.Content);
				var user = UserManager.GetUserByName(login.Name);
				if (user != null && user.PasswordHash == login.PasswordHash)
				{
					userClientMap[login.Name] = e.ClientId!;
					clientUserMap[e.ClientId!] = login.Name;
					await webSocketServer.SendMessageAsync(e.ClientId!, Util.ToJson(new Message { MessageType = MessageType.Login, Content = "登录成功" }));
				}
				else
				{
					await webSocketServer.SendMessageAsync(e.ClientId!, Util.ToJson(new Message { MessageType = MessageType.Login, Content = "用户名或密码错误" }));
				}
				break;
			case MessageType.Register:
				var reg = Util.FromJson<Valency.Chatt.Core.MessageImps.LoginAndRegisterMessage>(msg.Content);
				if (UserManager.GetUserByName(reg.Name) == null)
				{
					var newUser = new User { Id = UserManager.GetNextUserId(), Name = reg.Name, Email = "", PasswordHash = reg.PasswordHash };
					UserManager.AddUser(newUser);
					await webSocketServer.SendMessageAsync(e.ClientId!, Util.ToJson(new Message { MessageType = MessageType.Register, Content = "注册成功" }));
				}
				else
				{
					await webSocketServer.SendMessageAsync(e.ClientId!, Util.ToJson(new Message { MessageType = MessageType.Register, Content = "用户名已存在" }));
				}
				break;
			case MessageType.SendMessage:
				if (clientUserMap.TryGetValue(e.ClientId!, out var fromUser))
				{
					// 广播给所有在线用户
					await webSocketServer.BroadcastMessageAsync(Util.ToJson(new Message { MessageType = MessageType.MessageReceived, Content = $"{fromUser}: {msg.Content}" }));
				}
				break;
			default:
				await webSocketServer.SendMessageAsync(e.ClientId!, Util.ToJson(new Message { MessageType = MessageType.NullRequest, Content = "未知请求" }));
				break;
		}
	}

	private static void WebSocketServer_ErrorOccurred(object? sender, Exception e)
	{
		Debug.WriteLine(e.ToString());
	}

	private static void WebSocketServer_ClientDisconnected(object? sender, string e)
	{
		if (clientUserMap.TryGetValue(e, out var user))
		{
			userClientMap.Remove(user);
			clientUserMap.Remove(e);
		}
		Console.WriteLine($"断开连接：{e}");
	}

	private static async void WebSocketServer_ClientConnected(object? sender, string e)
	{
		Console.WriteLine($"新客户端连接: {e}");
		await webSocketServer.SendMessageAsync(e, Util.ToJson(new Message { MessageType = MessageType.NullRequest, Content = "欢迎连接服务器，请先登录。" }));
	}
}
