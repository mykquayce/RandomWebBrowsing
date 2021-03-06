﻿using Dawn;
using Helpers.Common;
using Helpers.Tracing;
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
		private readonly OpenTracing.ITracer? _tracer;

		public GetSubredditThreadsStep(
			Services.IRedditService redditService,
			OpenTracing.ITracer? tracer = default)
		{
			_redditService = Guard.Argument(() => redditService).NotNull().Value;
			_tracer = tracer;
		}

		public string? SubredditUriString { get; set; }
		public ICollection<string> ThreadsUris { get; } = new List<string>();

		public async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
		{
			using var scope = _tracer?.StartSpan();

			Guard.Argument(() => SubredditUriString!)
				.NotNull()
				.NotEmpty()
				.NotWhiteSpace()
				.Matches(@"^https:\/\/old\.reddit\.com\/r\/[_\d\w]+\/.rss\b");

			var subredditUri = new Uri(SubredditUriString!, UriKind.Absolute).StripQuery();

			await foreach (var uri in _redditService.GetSubredditThreadsAsync(subredditUri))
			{
				ThreadsUris.Add(uri.StripQuery().OriginalString);
			}

			scope?.Span.Log(
				nameof(SubredditUriString), SubredditUriString,
				"Count", ThreadsUris.Count);

			return ExecutionResult.Next();
		}
	}
}
