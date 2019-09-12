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
		private readonly OpenTracing.IScope? _parentScope;

		public PublishMessageStep(
			Services.IMessageQueueService messageQueueService,
			OpenTracing.ITracer? tracer = default,
			OpenTracing.IScope? parentScope = default)
		{
			_messageQueueService = Guard.Argument(() => messageQueueService).NotNull().Value;
			_tracer = tracer;
			_parentScope = parentScope;
		}

		public string? Message { get; set; }

		public Task<ExecutionResult> RunAsync(IStepExecutionContext context)
		{
			using var scope = _tracer?
				.BuildDefaultSpan()
				.AsChildOf(_parentScope?.Span)
				.StartActive(finishSpanOnDispose: true);

			Guard.Argument(() => Message).NotNull().NotEmpty().NotWhiteSpace();

			scope?.Span.Log(nameof(Message), Message);

			var bytes = Encoding.UTF8.GetBytes(Message);

			_messageQueueService.Publish(bytes);

			return Task.FromResult(ExecutionResult.Next());
		}
	}
}
