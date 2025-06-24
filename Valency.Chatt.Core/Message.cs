namespace Valency.Chatt.Core
{
	[Serializable]
	public class Message
	{
		public Message() { }

		public MessageType MessageType { get; set; }

		public string Content { get; set; } = "";
	}


	public enum MessageType
	{
		Login, Register, GetMessageByID, GetAllMessage, AddUser, DelUser, NullRequest, SendMessage, MessageReceived
	}
}
