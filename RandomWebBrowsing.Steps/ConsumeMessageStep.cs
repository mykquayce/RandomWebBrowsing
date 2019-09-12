using Dawn;
using Helpers.Tracing;
using System.Threading.Tasks;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace RandomWebBrowsing.Steps
{
	public class ConsumeMessageStep : IStepBody
	{
		private readonly Services.IMessageQueueService _messageQueueService;
		private readonly OpenTracing.ITracer? _tracer;
		private readonly OpenTracing.IScope? _parentScope;

		public ConsumeMessageStep(
			Services.IMessageQueueService messageQueueService,
			OpenTracing.ITracer? tracer = default,
			OpenTracing.IScope? parentScope = default)
		{
			_messageQueueService = Guard.Argument(() => messageQueueService).NotNull().Value;
			_tracer = tracer;
			_parentScope = parentScope;
		}

		public string? Message { get; set; }
		public ulong? DeliveryTag { get; set; }

		public Task<ExecutionResult> RunAsync(IStepExecutionContext context)
		{
			using var scope = _tracer?
				.BuildDefaultSpan()
				.AsChildOf(_parentScope?.Span)
				.StartActive(finishSpanOnDispose: true);

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

			scope?.Span.Log(
				nameof(Message), Message,
				nameof(DeliveryTag), DeliveryTag
			);

			return Task.FromResult(ExecutionResult.Next());
		}
	}
}
