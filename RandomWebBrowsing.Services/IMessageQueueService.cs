using System;

namespace RandomWebBrowsing.Services
{
	public interface IMessageQueueService : IDisposable
	{
		void Acknowledge(ulong value);
		(byte[] bytes, ulong deliveryTag) Consume();
		(T value, ulong deliveryTag) Consume<T>();
		void Publish(byte[] bytes);
		void Publish<T>(T value);
	}
}
