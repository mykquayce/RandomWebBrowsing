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
		private readonly OpenTracing.IScope? _parentScope;

		public EvaluateMessageStep(
			Services.IMessageService evaluateMessageService,
			OpenTracing.ITracer? tracer = default,
			OpenTracing.IScope? parentScope = default)
		{
			_evaluateMessageService = evaluateMessageService;
			_tracer = tracer;
			_parentScope = parentScope;
		}

		public string? Message { get; set; }
		public Models.MessageTypes? MessageTypes { get; set; }

		public Task<ExecutionResult> RunAsync(IStepExecutionContext context)
		{
			using var scope = _tracer?
				.BuildDefaultSpan()
				.AsChildOf(_parentScope?.Span)
				.StartActive(finishSpanOnDispose: true);

			Guard.Argument(() => Message).NotNull().NotEmpty().NotWhiteSpace();

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
