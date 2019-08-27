using Dawn;
using System.Collections.Generic;
using System.Threading.Tasks;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace RandomWebBrowsing.Steps
{
	public class GetLinksFromCommentStep : IStepBody
	{
		private readonly Services.IMessageService _messageService;

		public GetLinksFromCommentStep(
			Services.IMessageService messageService)
		{
			_messageService = Guard.Argument(() => messageService).NotNull().Value;
		}

		public string? Comment { get; set; }
		public ICollection<string> Links { get; } = new List<string>();

		public Task<ExecutionResult> RunAsync(IStepExecutionContext context)
		{
			Guard.Argument(() => Comment).NotNull().NotEmpty().NotWhiteSpace();

			foreach (var link in _messageService.GetLinksFromComment(Comment!))
			{
				Links.Add(link.OriginalString);
			}

			return Task.FromResult(ExecutionResult.Next());
		}
	}
}
