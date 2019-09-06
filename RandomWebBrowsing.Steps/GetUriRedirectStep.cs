using Dawn;
using Helpers.Tracing;
using OpenTracing;
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
		private readonly ITracer? _tracer;

		public GetUriRedirectStep(
			Clients.IHttpClient httpClient,
			ITracer? tracer = default)
		{
			_httpClient = Guard.Argument(() => httpClient).NotNull().Value;
			_tracer = tracer;
		}

		public Uri? Uri { get; set; }
		public Uri? RedirectUri { get; set; }

		public async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
		{
			using var scope = _tracer?
				.BuildDefaultSpan()
				.WithTag(nameof(Uri), Uri?.OriginalString)
				.StartActive(finishSpanOnDispose: true);

			Guard.Argument(() => Uri)
				.NotNull()
				.Require(uri => uri!.IsAbsoluteUri, _ => nameof(Uri) + " must be absolute");

			Guard.Argument(() => Uri!.OriginalString).NotNull().NotEmpty().NotWhiteSpace();

			var headers = await _httpClient.GetHeadersAsync(Uri!);

			Guard.Argument(() => headers).NotNull().NotEmpty().Require(d => d.ContainsKey("location"));

			var locations = headers["location"];

			Guard.Argument(() => locations).NotNull().Count(1);

			var location = locations.Single();

			Guard.Argument(() => location).NotNull().NotEmpty().NotWhiteSpace().StartsWith("http");

			RedirectUri = new Uri(location, UriKind.Absolute);

			scope?.Span.Log(nameof(RedirectUri), location);

			return ExecutionResult.Next();
		}
	}
}
