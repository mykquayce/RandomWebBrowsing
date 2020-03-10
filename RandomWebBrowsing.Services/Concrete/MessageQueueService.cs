using Dawn;
using Helpers.Tracing;
using Microsoft.Extensions.Options;
using RabbitMQ.Client.Exceptions;

namespace RandomWebBrowsing.Services.Concrete
{
	public class MessageQueueService : Helpers.RabbitMQ.Concrete.RabbitMQService, IMessageQueueService
	{
		private readonly string _queueName;
		private readonly OpenTracing.ITracer? _tracer;

		public MessageQueueService(
			IOptions<Config.Settings> options,
			IOptions<Helpers.RabbitMQ.Models.RabbitMQSettings> settingsOptions,
			OpenTracing.ITracer? tracer = default)
			: base(settingsOptions.Value)
		{
			Guard.Argument(() => options).NotNull();
			Guard.Argument(() => options.Value).NotNull();
			_queueName = Guard.Argument(() => options.Value.QueueName!).NotNull().NotEmpty().NotWhiteSpace().Value;

			_tracer = tracer;
		}

		public (byte[] bytes, ulong deliveryTag) Consume()
		{
			using var scope = _tracer?.StartSpan();

			scope?.Span.SetTag(nameof(_queueName), _queueName);

			return base.Consume(_queueName);
		}

		public (T value, ulong deliveryTag) Consume<T>() => base.Consume<T>(_queueName);
		public void Publish(byte[] bytes) => base.Publish(_queueName, bytes);
		public void Publish<T>(T value) => base.Publish<T>(_queueName, value);
	}
}
