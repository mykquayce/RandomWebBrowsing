using Dawn;
using Helpers.Tracing;
using OpenTracing;
using System.Text;
using System.Threading.Tasks;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace RandomWebBrowsing.Steps
{
	public class PublishMessageStep : IStepBody
	{
		private readonly Services.IMessageQueueService _messageQueueService;
		private readonly ITracer? _tracer;

		public PublishMessageStep(
			Services.IMessageQueueService messageQueueService,
			ITracer? tracer = default)
		{
			_messageQueueService = Guard.Argument(() => messageQueueService).NotNull().Value;
			_tracer = tracer;
		}

		public string? Message { get; set; }

		public Task<ExecutionResult> RunAsync(IStepExecutionContext context)
		{
			using var scope = _tracer?
				.BuildDefaultSpan()
				.WithTag(nameof(Message), Message)
				.StartActive(finishSpanOnDispose: true);

			Guard.Argument(() => Message).NotNull().NotEmpty().NotWhiteSpace();

			var bytes = Encoding.UTF8.GetBytes(Message);

			_messageQueueService.Publish(bytes);

			return Task.FromResult(ExecutionResult.Next());
		}
	}
}
