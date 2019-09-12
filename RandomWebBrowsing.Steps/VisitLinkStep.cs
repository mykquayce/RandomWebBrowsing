using Dawn;
using Helpers.Tracing;
using System;
using System.Threading.Tasks;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace RandomWebBrowsing.Steps
{
	public class VisitLinkStep : IStepBody
	{
		private readonly Clients.IHttpClient _httpClient;
		private readonly OpenTracing.ITracer? _tracer;
		private readonly OpenTracing.IScope? _parentScope;

		public VisitLinkStep(
			Clients.IHttpClient httpClient,
			OpenTracing.ITracer? tracer = default,
			OpenTracing.IScope? parentScope = default)
		{
			_httpClient = Guard.Argument(()=> httpClient).NotNull().Value;
			_tracer = tracer;
			_parentScope = parentScope;
		}

		public string? UriString { get; set; }

		public async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
		{
			using var scope = _tracer?
				.BuildDefaultSpan()
				.AsChildOf(_parentScope?.Span)
				.StartActive(finishSpanOnDispose: true);

			Guard.Argument(() => UriString).NotNull().NotEmpty().NotWhiteSpace().StartsWith("http");

			scope?.Span.Log(nameof(UriString), UriString);

			var uri = new Uri(UriString, UriKind.Absolute);

			await _httpClient.VisitLinkAsync(uri);

			return ExecutionResult.Next();
		}
	}
}
