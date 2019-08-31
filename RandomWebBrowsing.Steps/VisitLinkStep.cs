using Dawn;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace RandomWebBrowsing.Steps
{
	public class VisitLinkStep : IStepBody
	{
		private readonly Clients.IHttpClient _httpClient;
		private readonly ILogger? _logger;

		public VisitLinkStep(
			Clients.IHttpClient httpClient,
			ILogger<VisitLinkStep>? logger = default)
		{
			_httpClient = Guard.Argument(()=> httpClient).NotNull().Value;
			_logger = logger;
		}

		public string? UriString { get; set; }

		public async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
		{
			Guard.Argument(() => UriString).NotNull().NotEmpty().NotWhiteSpace().StartsWith("http");

			_logger?.LogInformation(string.Concat(nameof(VisitLinkStep), ":", UriString));

			var uri = new Uri(UriString, UriKind.Absolute);

			await _httpClient.VisitLinkAsync(uri);

			return ExecutionResult.Next();
		}
	}
}
