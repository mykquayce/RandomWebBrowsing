using Dawn;
using Microsoft.Extensions.Options;

namespace RandomWebBrowsing.Services.Concrete
{
	public class MessageQueueService : Helpers.RabbitMQ.Concrete.RabbitMQService, IMessageQueueService
	{
		private readonly string _queueName;

		public MessageQueueService(
			IOptions<Config.Settings> options,
			IOptions<Helpers.RabbitMQ.Models.RabbitMQSettings> settingsOptions,
			OpenTracing.ITracer? tracer = default)
			: base(settingsOptions.Value, tracer)
		{
			_queueName = Guard.Argument(() => options).NotNull()
				.Wrap(o => o.Value).NotNull()
				.Wrap(s => s.QueueName!).NotNull().NotEmpty().NotWhiteSpace().Value;
		}

		public (byte[] bytes, ulong deliveryTag) Consume() => base.Consume(_queueName);

		public (T value, ulong deliveryTag) Consume<T>() => base.Consume<T>(_queueName);
		public void Publish(byte[] bytes) => base.Publish(_queueName, bytes);
		public void Publish<T>(T value) => base.Publish<T>(_queueName, value);
	}
}
