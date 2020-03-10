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

		public ConsumeMessageStep(
			Services.IMessageQueueService messageQueueService,
			OpenTracing.ITracer? tracer = default)
		{
			_messageQueueService = Guard.Argument(() => messageQueueService).NotNull().Value;
			_tracer = tracer;
		}

		public string? Message { get; set; }
		public ulong? DeliveryTag { get; set; }

		public Task<ExecutionResult> RunAsync(IStepExecutionContext context)
		{
			using var scope = _tracer?.StartSpan();

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
