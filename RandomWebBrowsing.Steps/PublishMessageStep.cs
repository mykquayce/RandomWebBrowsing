using Dawn;
using System.Text;
using System.Threading.Tasks;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace RandomWebBrowsing.Steps
{
	public class PublishMessageStep : IStepBody
	{
		private readonly Services.IMessageQueueService _messageQueueService;

		public PublishMessageStep(
			Services.IMessageQueueService messageQueueService)
		{
			_messageQueueService = Guard.Argument(() => messageQueueService).NotNull().Value;
		}

		public string? Message { get; set; }

		public Task<ExecutionResult> RunAsync(IStepExecutionContext context)
		{
			Guard.Argument(() => Message).NotNull().NotEmpty().NotWhiteSpace();

			var bytes = Encoding.UTF8.GetBytes(Message);

			_messageQueueService.Publish(bytes);

			return Task.FromResult(ExecutionResult.Next());
		}
	}
}
