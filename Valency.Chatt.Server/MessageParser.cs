using Valency.Chatt.Core;

namespace Valency.Chatt.Server
{
	internal class MessageParser
	{
		public static void Parse(Core.Message message)
		{
			switch (message.MessageType)
			{
				case MessageType.Login:
					break;
				case MessageType.Register:
					break;
				case MessageType.GetMessageByID:
					break;
				case MessageType.GetAllMessage:
					break;
				case MessageType.AddUser:
					break;
				case MessageType.DelUser:
					break;
				case MessageType.NullRequest:
					break;
				case MessageType.SendMessage:
					break;
				default:
					break;
			}
		}
	}
}
