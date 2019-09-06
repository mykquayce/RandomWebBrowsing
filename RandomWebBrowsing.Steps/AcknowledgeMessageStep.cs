using Dawn;
using Helpers.Tracing;
using OpenTracing;
using System.Threading.Tasks;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace RandomWebBrowsing.Steps
{
	public class AcknowledgeMessageStep : IStepBody
	{
		private readonly Services.IMessageQueueService _messageQueueService;
		private readonly ITracer? _tracer;

		public AcknowledgeMessageStep(
			Services.IMessageQueueService messageQueueService,
			ITracer? tracer = default)
		{
			_messageQueueService = Guard.Argument(() => messageQueueService).NotNull().Value;
			_tracer = tracer;
		}

		public ulong? DeliveryTag { get; set; }

		public Task<ExecutionResult> RunAsync(IStepExecutionContext context)
		{
			using var scope = _tracer?
				.BuildDefaultSpan()
				.WithTag(nameof(DeliveryTag), DeliveryTag?.ToString("D"))
				.StartActive(finishSpanOnDispose: true);

			Guard.Argument(() => DeliveryTag).NotNull().Positive();

			_messageQueueService.Acknowledge(DeliveryTag!.Value);

			return Task.FromResult(ExecutionResult.Next());
		}
	}
}
