using System.Diagnostics;
using System.Net;
using System.Net.WebSockets;
using System.Text;

namespace Valency.Chatt.Server;

class Program
{
	static WebSocketServer webSocketServer = new WebSocketServer(54258);
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

	private static void WebSocketServer_MessageReceived(object? sender, WebSocketMessageEventArgs e)
	{
		throw new NotImplementedException();
	}

	private static void WebSocketServer_ErrorOccurred(object? sender, Exception e)
	{
		Debug.WriteLine(e.ToString());
		throw e;
	}

	private static void WebSocketServer_ClientDisconnected(object? sender, string e)
	{
		throw new NotImplementedException();
	}

	private static async void WebSocketServer_ClientConnected(object? sender, string e)
	{
		Console.WriteLine("Helloworld!");
		await webSocketServer.SendMessageAsync(e,"nihao");
	}
}
