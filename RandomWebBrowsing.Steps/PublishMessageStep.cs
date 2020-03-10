using Dawn;
using Helpers.Tracing;
using System.Text;
using System.Threading.Tasks;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace RandomWebBrowsing.Steps
{
	public class PublishMessageStep : IStepBody
	{
		private readonly Services.IMessageQueueService _messageQueueService;
		private readonly OpenTracing.ITracer? _tracer;

		public PublishMessageStep(
			Services.IMessageQueueService messageQueueService,
			OpenTracing.ITracer? tracer = default)
		{
			_messageQueueService = Guard.Argument(() => messageQueueService).NotNull().Value;
			_tracer = tracer;
		}

		public string? Message { get; set; }

		public Task<ExecutionResult> RunAsync(IStepExecutionContext context)
		{
			using var scope = _tracer?.StartSpan();

			Guard.Argument(() => Message!).NotNull().NotEmpty().NotWhiteSpace();

			scope?.Span.Log(nameof(Message), Message);

			var bytes = Encoding.UTF8.GetBytes(Message);

			_messageQueueService.Publish(bytes);

			return Task.FromResult(ExecutionResult.Next());
		}
	}
}
