using Microsoft.Extensions.Options;
using System;

namespace RandomWebBrowsing.Steps.Tests.Fixtures
{
	public sealed class MessageQueueServiceFixture : UserSecretsFixture, IDisposable
	{
		public MessageQueueServiceFixture()
		{
			var options = Options.Create(new Config.Settings { QueueName = "test", });
			var settingsOptions = Options.Create(base.RabbitMQSettings);

			MessageQueueService = new Services.Concrete.MessageQueueService(options, settingsOptions);
		}

		public Services.IMessageQueueService MessageQueueService { get; }

		public void Dispose() => MessageQueueService?.Dispose();
	}
}
