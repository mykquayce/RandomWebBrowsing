using Dawn;
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

		public GetUriRedirectStep(
			Clients.IHttpClient httpClient)
		{
			_httpClient = Guard.Argument(() => httpClient).NotNull().Value;
		}

		public Uri? Uri { get; set; }
		public Uri? RedirectUri { get; set; }

		public async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
		{
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

			RedirectUri = new Uri(locations.Single(), UriKind.Absolute);

			return ExecutionResult.Next();
		}
	}
}
