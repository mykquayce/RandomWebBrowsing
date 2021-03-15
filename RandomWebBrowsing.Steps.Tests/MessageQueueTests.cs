using System.Threading.Tasks;
using WorkflowCore.Models;
using Xunit;

namespace RandomWebBrowsing.Steps.Tests
{
	public class MessageQueueTests : IClassFixture<Fixtures.MessageQueueServiceFixture>
	{
		private readonly Steps.AcknowledgeMessageStep _acknowledgeMessageStep;
		private readonly Steps.ConsumeMessageStep _consumeMessageStep;
		private readonly Steps.PublishMessageStep _publishMessageStep;

		public MessageQueueTests(Fixtures.MessageQueueServiceFixture fixture)
		{
			_acknowledgeMessageStep = new Steps.AcknowledgeMessageStep(fixture.MessageQueueService);
			_consumeMessageStep = new Steps.ConsumeMessageStep(fixture.MessageQueueService);
			_publishMessageStep = new Steps.PublishMessageStep(fixture.MessageQueueService);
		}

		public Task<ExecutionResult> AcknowledgeAsync(ulong deliveryTag)
		{
			_acknowledgeMessageStep.DeliveryTag = deliveryTag;
			return _acknowledgeMessageStep.RunAsync(new StepExecutionContext());
		}

		public async Task<(string message, ulong deliveryTag)> ConsumeAsync()
		{
			await _consumeMessageStep.RunAsync(new StepExecutionContext());
			return (_consumeMessageStep.Message!, _consumeMessageStep.DeliveryTag!.Value);
		}

		public Task<ExecutionResult> PublishAsync(string message)
		{
			_publishMessageStep.Message = message;
			return _publishMessageStep.RunAsync(new StepExecutionContext());
		}

#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable xUnit1004 // Test methods should not be skipped
		[Fact(Skip = "needs rabbitmq running with settings saved in user secrets. see readme.md")]
#pragma warning restore xUnit1004, IDE0079 // Test methods should not be skipped ; Remove unnecessary suppression
		public async Task PublishConsumeAcknowledge()
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
