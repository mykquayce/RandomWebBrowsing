using Dawn;
using System.Threading.Tasks;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace RandomWebBrowsing.Steps
{
	public class AcknowledgeMessageStep : IStepBody
	{
		private readonly Services.IMessageQueueService _messageQueueService;

		public AcknowledgeMessageStep(
			Services.IMessageQueueService messageQueueService)
		{
			_messageQueueService = Guard.Argument(() => messageQueueService).NotNull().Value;
		}

		public ulong? DeliveryTag { get; set; }

		public Task<ExecutionResult> RunAsync(IStepExecutionContext context)
		{
			Guard.Argument(() => DeliveryTag).NotNull().Positive();

			_messageQueueService.Acknowledge(DeliveryTag!.Value);

			return Task.FromResult(ExecutionResult.Next());
		}
	}
}
