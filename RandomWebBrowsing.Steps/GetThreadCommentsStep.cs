using Dawn;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace RandomWebBrowsing.Steps
{
	public class GetThreadCommentsStep : IStepBody
	{
		private readonly Services.IRedditService _redditService;

		public GetThreadCommentsStep(
			Services.IRedditService redditService)
		{
			_redditService = Guard.Argument(() => redditService).NotNull().Value;
		}

		public string? ThreadUriString { get; set; }
		public ICollection<string> Comments { get; } = new List<string>();

		public async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
		{
			Guard.Argument(() => ThreadUriString).NotNull().NotEmpty().NotWhiteSpace();

			var uri = new Uri(ThreadUriString!, UriKind.Absolute);

			await foreach (var comment in _redditService.GetThreadCommentsAsync(uri))
			{
				Comments.Add(comment);
			}

			return ExecutionResult.Next();
		}
	}
}
