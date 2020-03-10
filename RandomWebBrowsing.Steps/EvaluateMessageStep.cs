using Dawn;
using Helpers.Tracing;
using System.Threading.Tasks;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace RandomWebBrowsing.Steps
{
	public class EvaluateMessageStep : IStepBody
	{
		private readonly Services.IMessageService _evaluateMessageService;
		private readonly OpenTracing.ITracer? _tracer;

		public EvaluateMessageStep(
			Services.IMessageService evaluateMessageService,
			OpenTracing.ITracer? tracer = default)
		{
			_evaluateMessageService = evaluateMessageService;
			_tracer = tracer;
		}

		public string? Message { get; set; }
		public Models.MessageTypes? MessageTypes { get; set; }

		public Task<ExecutionResult> RunAsync(IStepExecutionContext context)
		{
			using var scope = _tracer?.StartSpan();

			Guard.Argument(() => Message!).NotNull().NotEmpty().NotWhiteSpace();

			MessageTypes = _evaluateMessageService.GetMessageTypes(Message!);

			Guard.Argument(() => MessageTypes)
				.NotNull()
				.NotEqual(Models.MessageTypes.None);

			scope?.Span.Log(
				nameof(Message), Message,
				nameof(MessageTypes), MessageTypes);

			return Task.FromResult(ExecutionResult.Next());
		}
	}
}
