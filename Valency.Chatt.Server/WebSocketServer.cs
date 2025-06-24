using System.Collections.Concurrent;
using System.Net;
using System.Net.WebSockets;
using System.Text;

namespace Valency.Chatt.Server;

/// <summary>
/// WebSocket服务器帮助类
/// </summary>
public class WebSocketServer : IDisposable
{
	private readonly HttpListener _listener;
	private readonly CancellationTokenSource _cancellationTokenSource;
	private readonly ConcurrentDictionary<string, WebSocket> _connectedClients;
	private readonly int _bufferSize;
	private bool _isDisposed;

	/// <summary>
	/// 客户端连接事件
	/// </summary>
	public event EventHandler<string>? ClientConnected;

	/// <summary>
	/// 客户端断开连接事件
	/// </summary>
	public event EventHandler<string>? ClientDisconnected;

	/// <summary>
	/// 收到消息事件
	/// </summary>
	public event EventHandler<WebSocketMessageEventArgs>? MessageReceived;

	/// <summary>
	/// 错误发生事件
	/// </summary>
	public event EventHandler<Exception>? ErrorOccurred;

	/// <summary>
	/// 初始化WebSocket服务器帮助类
	/// </summary>
	/// <param name="port">监听端口</param>
	/// <param name="bufferSize">缓冲区大小</param>
	public WebSocketServer(int port = 8080, int bufferSize = 4096)
	{
		_listener = new HttpListener();
		_listener.Prefixes.Add($"http://+:{port}/");
		_cancellationTokenSource = new CancellationTokenSource();
		_connectedClients = new ConcurrentDictionary<string, WebSocket>();
		_bufferSize = bufferSize;
	}

	/// <summary>
	/// 启动服务器
	/// </summary>
	public async Task StartAsync()
	{
		try
		{
			_listener.Start();
			Console.WriteLine($"WebSocket服务器已启动，监听端口: {_listener.Prefixes}");

			await AcceptConnectionsAsync();
		}
		catch (Exception ex)
		{
			ErrorOccurred?.Invoke(this, ex);
			Console.WriteLine($"启动服务器时出错: {ex.Message}");
		}
	}

	/// <summary>
	/// 停止服务器
	/// </summary>
	public void Stop()
	{
		_cancellationTokenSource.Cancel();
		_listener.Close();

		// 关闭所有客户端连接
		foreach (var client in _connectedClients)
		{
			try
			{
				if (client.Value.State == WebSocketState.Open)
				{
					client.Value.CloseAsync(
						WebSocketCloseStatus.NormalClosure,
						"服务器关闭",
						CancellationToken.None).Wait();
				}
			}
			catch (Exception ex)
			{
				ErrorOccurred?.Invoke(this, ex);
				Console.WriteLine($"关闭客户端连接时出错: {ex.Message}");
			}
		}

		_connectedClients.Clear();
	}

	/// <summary>
	/// 向特定客户端发送消息
	/// </summary>
	/// <param name="clientId">客户端ID</param>
	/// <param name="message">消息内容</param>
	public async Task SendMessageAsync(string clientId, string message)
	{
		if (_connectedClients.TryGetValue(clientId, out var client) &&
			client.State == WebSocketState.Open)
		{
			var buffer = Encoding.UTF8.GetBytes(message);
			await client.SendAsync(
				new ArraySegment<byte>(buffer),
				WebSocketMessageType.Text,
				true,
				CancellationToken.None);
		}
	}

	/// <summary>
	/// 向所有客户端广播消息
	/// </summary>
	/// <param name="message">消息内容</param>
	public async Task BroadcastMessageAsync(string message)
	{
		var buffer = Encoding.UTF8.GetBytes(message);

		foreach (var client in _connectedClients)
		{
			if (client.Value.State == WebSocketState.Open)
			{
				await client.Value.SendAsync(
					new ArraySegment<byte>(buffer),
					WebSocketMessageType.Text,
					true,
					CancellationToken.None);
			}
		}
	}

	/// <summary>
	/// 接受客户端连接
	/// </summary>
	private async Task AcceptConnectionsAsync()
	{
		while (!_cancellationTokenSource.Token.IsCancellationRequested)
		{
			try
			{
				var context = await _listener.GetContextAsync();

				if (context.Request.IsWebSocketRequest)
				{
					var webSocketContext = await context.AcceptWebSocketAsync(null);
					var clientId = Guid.NewGuid().ToString();
					_connectedClients.TryAdd(clientId, webSocketContext.WebSocket);

					ClientConnected?.Invoke(this, clientId);
					Console.WriteLine($"客户端 {clientId} 已连接");

					// 处理客户端消息
					_ = HandleClientAsync(clientId, webSocketContext.WebSocket);
				}
				else
				{
					context.Response.StatusCode = 400;
					context.Response.Close();
				}
			}
			catch (Exception ex)
			{
				if (!_cancellationTokenSource.Token.IsCancellationRequested)
				{
					ErrorOccurred?.Invoke(this, ex);
					Console.WriteLine($"接受客户端连接时出错: {ex.Message}");
				}
			}
		}
	}

	/// <summary>
	/// 处理客户端消息
	/// </summary>
	private async Task HandleClientAsync(string clientId, WebSocket webSocket)
	{
		var buffer = new byte[_bufferSize];

		try
		{
			while (webSocket.State == WebSocketState.Open &&
				   !_cancellationTokenSource.Token.IsCancellationRequested)
			{
				var receiveResult = await webSocket.ReceiveAsync(
					new ArraySegment<byte>(buffer),
					_cancellationTokenSource.Token);

				if (receiveResult.MessageType == WebSocketMessageType.Close)
				{
					await webSocket.CloseAsync(
						WebSocketCloseStatus.NormalClosure,
						string.Empty,
						CancellationToken.None);

					_connectedClients.TryRemove(clientId, out _);
					ClientDisconnected?.Invoke(this, clientId);
					Console.WriteLine($"客户端 {clientId} 已断开连接");
					return;
				}

				var message = Encoding.UTF8.GetString(buffer, 0, receiveResult.Count);
				MessageReceived?.Invoke(this, new WebSocketMessageEventArgs
				{
					ClientId = clientId,
					Message = message
				});
				Console.WriteLine($"从客户端 {clientId} 收到消息: {message}");
			}
		}
		catch (Exception ex)
		{
			if (_connectedClients.TryRemove(clientId, out _))
			{
				ClientDisconnected?.Invoke(this, clientId);
				Console.WriteLine($"客户端 {clientId} 异常断开: {ex.Message}");
			}

			ErrorOccurred?.Invoke(this, ex);
		}
		finally
		{
			webSocket.Dispose();
		}
	}

	/// <summary>
	/// 释放资源
	/// </summary>
	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	/// <summary>
	/// 释放资源
	/// </summary>
	protected virtual void Dispose(bool disposing)
	{
		if (!_isDisposed)
		{
			if (disposing)
			{
				Stop();
				_cancellationTokenSource.Dispose();
			}

			_isDisposed = true;
		}
	}
}

/// <summary>
/// WebSocket消息事件参数
/// </summary>
public class WebSocketMessageEventArgs : EventArgs
{
	/// <summary>
	/// 客户端ID
	/// </summary>
	public string? ClientId { get; set; }

	/// <summary>
	/// 消息内容
	/// </summary>
	public string? Message { get; set; }
}