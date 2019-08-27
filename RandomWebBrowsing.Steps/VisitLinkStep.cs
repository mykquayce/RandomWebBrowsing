using Dawn;
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

		public VisitLinkStep(
			Clients.IHttpClient httpClient)
		{
			_httpClient = Guard.Argument(()=> httpClient).NotNull().Value;
		}

		public string? UriString { get; set; }

		public async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
		{
			Guard.Argument(() => UriString).NotNull().NotEmpty().NotWhiteSpace().StartsWith("http");

			var uri = new Uri(UriString, UriKind.Absolute);

			await _httpClient.VisitLinkAsync(uri);

			return ExecutionResult.Next();
		}
	}
}
