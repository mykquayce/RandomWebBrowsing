using Dawn;
using System.Threading.Tasks;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace RandomWebBrowsing.Steps
{
	public class ConsumeMessageStep : IStepBody
	{
		private readonly Services.IMessageQueueService _messageQueueService;

		public ConsumeMessageStep(
			Services.IMessageQueueService messageQueueService)
		{
			_messageQueueService = Guard.Argument(() => messageQueueService).NotNull().Value;
		}

		public string? Message { get; set; }
		public ulong? DeliveryTag { get; set; }

		public Task<ExecutionResult> RunAsync(IStepExecutionContext context)
		{

			try
			{
				var (bytes, deliveryTag) = _messageQueueService.Consume();

				Guard.Argument(() => bytes)
					.NotNull()
					.NotEmpty()
					.DoesNotContain((byte)0);

				Message = System.Text.Encoding.UTF8.GetString(bytes);

				DeliveryTag = Guard.Argument(() => deliveryTag).Positive().Value;
			}
			catch (Helpers.RabbitMQ.Exceptions.QueueNotFoundException)
			{ }
			catch (Helpers.RabbitMQ.Exceptions.QueueEmptyException)
			{ }

			return Task.FromResult(ExecutionResult.Next());
		}
	}
}
