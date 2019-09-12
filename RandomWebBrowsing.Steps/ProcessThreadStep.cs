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
		private readonly OpenTracing.IScope? _parentScope;

		public ProcessThreadStep(
			Services.IRedditService redditService,
			Services.IMessageService messageService,
			OpenTracing.ITracer? tracer = default,
			OpenTracing.IScope? parentScope = default)
		{
			_redditService = Guard.Argument(() => redditService).NotNull().Value;
			_messageService = Guard.Argument(() => messageService).NotNull().Value;
			_tracer = tracer;
			_parentScope = parentScope;
		}

		public string? ThreadUriString { get; set; }
		public ICollection<string> Links { get; } = new List<string>();

		public async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
		{
			using var scope = _tracer?
				.BuildDefaultSpan()
				.AsChildOf(_parentScope?.Span)
				.StartActive(finishSpanOnDispose: true);

			Guard.Argument(() => ThreadUriString).NotNull().NotEmpty().NotWhiteSpace();

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
