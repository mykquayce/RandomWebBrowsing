using Dawn;
using Helpers.Tracing;
using OpenTracing;
using System;
using System.Threading.Tasks;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace RandomWebBrowsing.Steps
{
	public class VisitLinkStep : IStepBody
	{
		private readonly Clients.IHttpClient _httpClient;
		private readonly ITracer? _tracer;

		public VisitLinkStep(
			Clients.IHttpClient httpClient,
			ITracer? tracer = default)
		{
			_httpClient = Guard.Argument(()=> httpClient).NotNull().Value;
			_tracer = tracer;
		}

		public string? UriString { get; set; }

		public async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
		{
			using var scope = _tracer?
				.BuildDefaultSpan()
				.WithTag(nameof(UriString), UriString)
				.StartActive(finishSpanOnDispose: true);

			Guard.Argument(() => UriString).NotNull().NotEmpty().NotWhiteSpace().StartsWith("http");

			var uri = new Uri(UriString, UriKind.Absolute);

			await _httpClient.VisitLinkAsync(uri);

			return ExecutionResult.Next();
		}
	}
}
