using Dawn;
using Helpers.Tracing;
using System;
using System.Threading.Tasks;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace RandomWebBrowsing.Steps
{
	public class GetRandomSubredditStep : IStepBody
	{
		private readonly Services.IRedditService _redditService;
		private readonly OpenTracing.ITracer? _tracer;

		public GetRandomSubredditStep(
			Services.IRedditService redditService,
			OpenTracing.ITracer? tracer = default)
		{
			_redditService = Guard.Argument(() => redditService).NotNull().Value;
			_tracer = tracer;
		}

		public Uri? RandomSubredditUri { get; set; }

		public async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
		{
			using var scope = _tracer?.StartSpan();

			RandomSubredditUri = await _redditService.GetRandomSubredditAsync();

			scope?.Span.Log(
				nameof(RandomSubredditUri), RandomSubredditUri.OriginalString);

			return ExecutionResult.Next();
		}
	}
}
