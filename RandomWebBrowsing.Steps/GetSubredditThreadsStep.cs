using Dawn;
using Helpers.Common;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace RandomWebBrowsing.Steps
{
	public class GetSubredditThreadsStep : IStepBody
	{
		private readonly Services.IRedditService _redditService;

		public GetSubredditThreadsStep(
			Services.IRedditService redditService)
		{
			_redditService = Guard.Argument(() => redditService).NotNull().Value;
		}

		public string? SubredditUriString { get; set; }
		public ICollection<string> TheadsUris { get; } = new List<string>();

		public async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
		{
			Guard.Argument(() => SubredditUriString)
				.NotNull()
				.NotEmpty()
				.NotWhiteSpace()
				.Matches(@"^https:\/\/old\.reddit\.com\/r\/[_\d\w]+\/.rss\b");

			var subredditUri = new Uri(SubredditUriString!, UriKind.Absolute).StripQuery();

			await foreach (var uri in _redditService.GetSubredditThreadsAsync(subredditUri))
			{
				TheadsUris.Add(uri.StripQuery().OriginalString);
			}

			return ExecutionResult.Next();
		}
	}
}
