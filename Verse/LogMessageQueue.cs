using System.Collections.Generic;

namespace Verse
{
	public class LogMessageQueue
	{
		public int maxMessages = 200;

		private Queue<LogMessage> messages = new Queue<LogMessage>();

		private LogMessage lastMessage;

		public IEnumerable<LogMessage> Messages => messages;

		public void Enqueue(LogMessage msg)
		{
			if (lastMessage != null && msg.CanCombineWith(lastMessage))
			{
				lastMessage.repeats++;
			}
			else
			{
				lastMessage = msg;
				messages.Enqueue(msg);
				if (messages.Count > maxMessages)
				{
					LogMessage oldMessage = messages.Dequeue();
					EditWindow_Log.Notify_MessageDequeued(oldMessage);
				}
			}
		}

		internal void Clear()
		{
			messages.Clear();
			lastMessage = null;
		}
	}
}
