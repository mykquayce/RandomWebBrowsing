using Dawn;
using Helpers.Tracing;
using System.Threading.Tasks;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace RandomWebBrowsing.Steps
{
	public class AcknowledgeMessageStep : IStepBody
	{
		private readonly Services.IMessageQueueService _messageQueueService;
		private readonly OpenTracing.ITracer? _tracer;

		public AcknowledgeMessageStep(
			Services.IMessageQueueService messageQueueService,
			OpenTracing.ITracer? tracer = default)
		{
			_messageQueueService = Guard.Argument(() => messageQueueService).NotNull().Value;
			_tracer = tracer;
		}

		public ulong? DeliveryTag { get; set; }

		public Task<ExecutionResult> RunAsync(IStepExecutionContext context)
		{
			using var scope = _tracer?.StartSpan();

			Guard.Argument(() => DeliveryTag).NotNull().Positive();

			scope?.Span.Log(nameof(DeliveryTag), DeliveryTag?.ToString());

			_messageQueueService.Acknowledge(DeliveryTag!.Value);

			return Task.FromResult(ExecutionResult.Next());
		}
	}
}
