using Dawn;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace RandomWebBrowsing.Steps
{
	public class ProcessThreadStep : IStepBody
	{
		private readonly Services.IRedditService _redditService;
		private readonly Services.IMessageService _messageService;

		public ProcessThreadStep(
			Services.IRedditService redditService,
			Services.IMessageService messageService)
		{
			_redditService = Guard.Argument(() => redditService).NotNull().Value;
			_messageService = Guard.Argument(() => messageService).NotNull().Value;
		}

		public string? ThreadUriString { get; set; }
		public ICollection<string> Links { get; } = new List<string>();

		public async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
		{
			Guard.Argument(() => ThreadUriString).NotNull().NotEmpty().NotWhiteSpace();

			var uri = new Uri(ThreadUriString!, UriKind.Absolute);

			await foreach (var comment in _redditService.GetThreadCommentsAsync(uri))
			{
				foreach (var link in _messageService.GetLinksFromComment(comment))
				{
					Links.Add(link.OriginalString);
				}
			}

			return ExecutionResult.Next();
		}
	}
}
