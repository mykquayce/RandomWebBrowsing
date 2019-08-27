using Dawn;
using System.Threading.Tasks;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace RandomWebBrowsing.Steps
{
	public class EvaluateMessageStep : IStepBody
	{
		private readonly Services.IMessageService _evaluateMessageService;

		public EvaluateMessageStep(
			Services.IMessageService evaluateMessageService)
		{
			_evaluateMessageService = evaluateMessageService;
		}

		public string? Message { get; set; }
		public Models.MessageTypes? MessageTypes { get; set; }

		public Task<ExecutionResult> RunAsync(IStepExecutionContext context)
		{
			Guard.Argument(() => Message).NotNull().NotEmpty().NotWhiteSpace();

			MessageTypes = _evaluateMessageService.GetMessageTypes(Message!);

			Guard.Argument(() => MessageTypes)
				.NotNull()
				.NotEqual(Models.MessageTypes.None);

			return Task.FromResult(ExecutionResult.Next());
		}
	}
}
