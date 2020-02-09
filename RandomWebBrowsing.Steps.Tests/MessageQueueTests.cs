using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;
using WorkflowCore.Models;
using Xunit;

namespace RandomWebBrowsing.Steps.Tests
{
	public sealed class MessageQueueTests : IDisposable
	{
		private static readonly Config.Settings _settings = new Config.Settings { QueueName = "test", };
		private static readonly Helpers.RabbitMQ.Concrete.RabbitMQSettings _rabbitSettings = new Helpers.RabbitMQ.Concrete.RabbitMQSettings
		{
			HostName = "localhost",
			Password = "guest",
			Port = 5_672,
			UserName = "guest",
			VirtualHost = "/",
		};

		private readonly Services.IMessageQueueService _messageQueueService;
		private readonly Steps.AcknowledgeMessageStep _acknowledgeMessageStep;
		private readonly Steps.ConsumeMessageStep _consumeMessageStep;
		private readonly Steps.PublishMessageStep _publishMessageStep;

		public MessageQueueTests()
		{
			var options = Options.Create(_settings);
			var rabbitSettingsOptions = Options.Create(_rabbitSettings);

			_messageQueueService = new Services.Concrete.MessageQueueService(options, rabbitSettingsOptions);

			_consumeMessageStep = new Steps.ConsumeMessageStep(_messageQueueService);
			_acknowledgeMessageStep = new Steps.AcknowledgeMessageStep(_messageQueueService);
			_publishMessageStep = new Steps.PublishMessageStep(_messageQueueService);
		}

		public void Dispose()
		{
			_messageQueueService.Dispose();
		}

		public Task<ExecutionResult> AcknowledgeAsync(ulong deliveryTag)
		{
			_acknowledgeMessageStep.DeliveryTag = deliveryTag;
			return _acknowledgeMessageStep.RunAsync(default);
		}

		public async Task<(string message, ulong deliveryTag)> ConsumeAsync()
		{
			await _consumeMessageStep.RunAsync(default);
			return (_consumeMessageStep.Message!, _consumeMessageStep.DeliveryTag!.Value);
		}

		public Task<ExecutionResult> PublishAsync(string message)
		{
			_publishMessageStep.Message = message;
			return _publishMessageStep.RunAsync(default);
		}

		[Fact]
		public async Task MessageQueueTests_PublishConsumeAcknowledge()
		{
			await PublishAsync("first");
			await PublishAsync("second");
			await PublishAsync("third");

			var (message1, deliveryTag1) = await ConsumeAsync();
			var (message2, deliveryTag2) = await ConsumeAsync();
			var (message3, deliveryTag3) = await ConsumeAsync();

			await AcknowledgeAsync(deliveryTag1);
			await AcknowledgeAsync(deliveryTag2);
			await AcknowledgeAsync(deliveryTag3);

			Assert.Equal("first", message1);
			Assert.Equal("second", message2);
			Assert.Equal("third", message3);
		}
	}
}
