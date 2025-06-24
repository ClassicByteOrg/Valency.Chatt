using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Valency.Chatt.Client
{
	/// <summary>
	/// WebSocket客户端帮助类（优化连接超时处理）
	/// </summary>
	public class WebSocketClient : IDisposable
	{
		private  ClientWebSocket _webSocket;
		private readonly CancellationTokenSource _cancellationTokenSource;
		private readonly int _bufferSize;
		private bool _isDisposed;
		private string _serverUrl;
		private const int DefaultTimeoutMs = 10000; // 默认10秒超时

		/// <summary>
		/// 连接成功事件
		/// </summary>
		public event EventHandler Connected;

		/// <summary>
		/// 连接关闭事件
		/// </summary>
		public event EventHandler<WebSocketCloseEventArgs> Closed;

		/// <summary>
		/// 收到消息事件
		/// </summary>
		public event EventHandler<string> MessageReceived;

		/// <summary>
		/// 错误发生事件
		/// </summary>
		public event EventHandler<WebSocketErrorEventArgs> ErrorOccurred;

		/// <summary>
		/// 获取连接状态
		/// </summary>
		public WebSocketState State => _webSocket.State;

		/// <summary>
		/// 初始化WebSocket客户端帮助类
		/// </summary>
		/// <param name="bufferSize">缓冲区大小</param>
		public WebSocketClient(int bufferSize = 4096)
		{
			_webSocket = new ClientWebSocket();
			_cancellationTokenSource = new CancellationTokenSource();
			_bufferSize = bufferSize;
		}

		/// <summary>
		/// 连接到WebSocket服务器（带超时处理）
		/// </summary>
		/// <param name="serverUrl">服务器URL</param>
		/// <param name="timeoutMs">连接超时时间(毫秒)，默认10000</param>
		public async Task ConnectAsync(string serverUrl, int timeoutMs = DefaultTimeoutMs)
		{
			try
			{
				_serverUrl = serverUrl;
				// 创建连接超时令牌
				using var timeoutCts = new CancellationTokenSource(timeoutMs);
				using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
					_cancellationTokenSource.Token, timeoutCts.Token);

				// 连接前重置WebSocket
				if (_webSocket.State != WebSocketState.Closed &&
					_webSocket.State != WebSocketState.Aborted && _webSocket.State != WebSocketState.None)
				{
					await _webSocket.CloseAsync(
						WebSocketCloseStatus.NormalClosure,
						"重置连接",
						CancellationToken.None);
				}

				_webSocket = new ClientWebSocket(); // 重新创建WebSocket实例

				// 执行连接并等待超时
				var connectTask = _webSocket.ConnectAsync(new Uri(serverUrl), linkedCts.Token);

				// 等待连接完成或超时
				var completedTask = await Task.WhenAny(connectTask, Task.Delay(timeoutMs, linkedCts.Token));
				if (completedTask != connectTask)
				{
					// 连接超时
					throw new TimeoutException($"连接到服务器 {serverUrl} 超时，等待时间: {timeoutMs}ms");
				}

				// 确保连接成功
				if (_webSocket.State != WebSocketState.Open)
				{
					throw new InvalidOperationException($"连接状态异常: {_webSocket.State}");
				}

				Connected?.Invoke(this, EventArgs.Empty);
				Console.WriteLine($"已连接到服务器: {serverUrl}");

				// 开始接收消息
				_ = ReceiveMessagesAsync();
			}
			catch (Exception ex)
			{
				OnErrorOccurred(ex, "连接服务器");
				throw;
			}
		}

		/// <summary>
		/// 断开与服务器的连接
		/// </summary>
		public async Task DisconnectAsync(WebSocketCloseStatus closeStatus = WebSocketCloseStatus.NormalClosure,
			string statusDescription = "客户端主动断开连接")
		{
			if (_webSocket.State == WebSocketState.Open)
			{
				await _webSocket.CloseAsync(closeStatus, statusDescription, CancellationToken.None);
				OnClosed(closeStatus, statusDescription);
			}
		}

		/// <summary>
		/// 向服务器发送消息
		/// </summary>
		/// <param name="message">消息内容</param>
		public async Task SendMessageAsync(string message)
		{
			if (_webSocket.State != WebSocketState.Open)
			{
				throw new InvalidOperationException("WebSocket连接未打开");
			}

			var buffer = Encoding.UTF8.GetBytes(message);
			await _webSocket.SendAsync(
				new ArraySegment<byte>(buffer),
				WebSocketMessageType.Text,
				true,
				_cancellationTokenSource.Token);
		}

		/// <summary>
		/// 接收服务器消息
		/// </summary>
		private async Task ReceiveMessagesAsync()
		{
			var buffer = new byte[_bufferSize];

			try
			{
				while (_webSocket.State == WebSocketState.Open &&
					   !_cancellationTokenSource.Token.IsCancellationRequested)
				{
					var result = await _webSocket.ReceiveAsync(
						new ArraySegment<byte>(buffer),
						_cancellationTokenSource.Token);

					if (result.MessageType == WebSocketMessageType.Close)
					{
						await _webSocket.CloseAsync(
							result.CloseStatus ?? WebSocketCloseStatus.NormalClosure,
							result.CloseStatusDescription,
							CancellationToken.None);

						OnClosed(result.CloseStatus ?? WebSocketCloseStatus.NormalClosure,
							result.CloseStatusDescription ?? "服务器关闭连接");
						return;
					}

					var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
					MessageReceived?.Invoke(this, message);
					Console.WriteLine($"收到服务器消息: {message}");
				}
			}
			catch (Exception ex)
			{
				OnErrorOccurred(ex, "接收消息");
			}
			finally
			{
				if (_webSocket.State != WebSocketState.Closed &&
					_webSocket.State != WebSocketState.Aborted)
				{
					await _webSocket.CloseAsync(
						WebSocketCloseStatus.NormalClosure,
						"接收消息循环结束",
						CancellationToken.None);
				}

				OnClosed(WebSocketCloseStatus.NormalClosure, "连接自然关闭");
			}
		}

		/// <summary>
		/// 触发关闭事件
		/// </summary>
		private void OnClosed(WebSocketCloseStatus closeStatus, string statusDescription)
		{
			Closed?.Invoke(this, new WebSocketCloseEventArgs
			{
				CloseStatus = closeStatus,
				StatusDescription = statusDescription
			});
			Console.WriteLine($"WebSocket连接已关闭: {closeStatus} - {statusDescription}");
		}

		/// <summary>
		/// 触发错误事件
		/// </summary>
		private void OnErrorOccurred(Exception ex, string operation)
		{
			ErrorOccurred?.Invoke(this, new WebSocketErrorEventArgs
			{
				Operation = operation,
				Exception = ex
			});
			Console.WriteLine($"WebSocket操作[{operation}]出错: {ex.Message}");
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
					_cancellationTokenSource.Cancel();

					// 确保连接关闭
					if (_webSocket.State != WebSocketState.Closed &&
						_webSocket.State != WebSocketState.Aborted)
					{
						_webSocket.CloseAsync(
							WebSocketCloseStatus.NormalClosure,
							"资源释放",
							CancellationToken.None).Wait();
					}

					_webSocket.Dispose();
					_cancellationTokenSource.Dispose();
				}

				_isDisposed = true;
			}
		}
	}

	/// <summary>
	/// WebSocket关闭事件参数
	/// </summary>
	public class WebSocketCloseEventArgs : EventArgs
	{
		/// <summary>
		/// 关闭状态码
		/// </summary>
		public WebSocketCloseStatus CloseStatus { get; set; }

		/// <summary>
		/// 状态描述
		/// </summary>
		public string StatusDescription { get; set; }
	}

	/// <summary>
	/// WebSocket错误事件参数
	/// </summary>
	public class WebSocketErrorEventArgs : EventArgs
	{
		/// <summary>
		/// 操作类型
		/// </summary>
		public string Operation { get; set; }

		/// <summary>
		/// 异常对象
		/// </summary>
		public Exception Exception { get; set; }
	}
}