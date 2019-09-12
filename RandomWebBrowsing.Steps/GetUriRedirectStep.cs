using Dawn;
using Helpers.Tracing;
using System;
using System.Linq;
using System.Threading.Tasks;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace RandomWebBrowsing.Steps
{
	public class GetUriRedirectStep : IStepBody
	{
		private readonly Clients.IHttpClient _httpClient;
		private readonly OpenTracing.ITracer? _tracer;
		private readonly OpenTracing.IScope? _parentScope;

		public GetUriRedirectStep(
			Clients.IHttpClient httpClient,
			OpenTracing.ITracer? tracer = default,
			OpenTracing.IScope? parentScope = default)
		{
			_httpClient = Guard.Argument(() => httpClient).NotNull().Value;
			_tracer = tracer;
			_parentScope = parentScope;
		}

		public string? UriString { get; set; }
		public string? RedirectUriString { get; set; }

		public async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
		{
			using var scope = _tracer?
				.BuildDefaultSpan()
				.AsChildOf(_parentScope?.Span)
				.StartActive(finishSpanOnDispose: true);

			Guard.Argument(() => UriString).NotNull().NotEmpty().NotWhiteSpace().StartsWith("http");

			var uri = new Uri(UriString!, UriKind.Absolute);

			var headers = await _httpClient.GetHeadersAsync(uri);

			Guard.Argument(() => headers).NotNull().NotEmpty().Require(d => d.ContainsKey("location"));

			var locations = headers["location"];

			Guard.Argument(() => locations).NotNull().Count(1);

			var location = locations.Single();

			Guard.Argument(() => location).NotNull().NotEmpty().NotWhiteSpace().StartsWith("http");

			RedirectUriString = location;

			scope?.Span.Log(
				nameof(UriString), UriString,
				nameof(RedirectUriString), RedirectUriString);

			return ExecutionResult.Next();
		}
	}
}
