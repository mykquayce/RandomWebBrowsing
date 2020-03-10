using Dawn;
using Helpers.Tracing;
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
		private readonly OpenTracing.ITracer? _tracer;

		public ProcessThreadStep(
			Services.IRedditService redditService,
			Services.IMessageService messageService,
			OpenTracing.ITracer? tracer = default)
		{
			_redditService = Guard.Argument(() => redditService).NotNull().Value;
			_messageService = Guard.Argument(() => messageService).NotNull().Value;
			_tracer = tracer;
		}

		public string? ThreadUriString { get; set; }
		public ICollection<string> Links { get; } = new List<string>();

		public async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
		{
			using var scope = _tracer?.StartSpan();

			try
			{
				Guard.Argument(() => ThreadUriString!).NotNull().NotEmpty().NotWhiteSpace();
			}
			catch (Exception exception)
			{
				scope?.Span.Log(exception);

				throw;
			}

			var uri = new Uri(ThreadUriString!, UriKind.Absolute);

			await foreach (var comment in _redditService.GetThreadCommentsAsync(uri))
			{
				foreach (var link in _messageService.GetLinksFromComment(comment))
				{
					Links.Add(link.OriginalString);
				}
			}

			scope?.Span.Log(
				nameof(ThreadUriString), ThreadUriString,
				"Links.Count", Links.Count);

			return ExecutionResult.Next();
		}
	}
}
