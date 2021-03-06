﻿using Dawn;
using Helpers.Tracing;
using System;
using System.Threading.Tasks;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace RandomWebBrowsing.Steps
{
	public class VisitLinkStep : IStepBody
	{
		private readonly Clients.IWebClient _webClient;
		private readonly OpenTracing.ITracer? _tracer;

		public VisitLinkStep(
			Clients.IWebClient webClient,
			OpenTracing.ITracer? tracer = default)
		{
			_webClient = Guard.Argument(()=> webClient).NotNull().Value;
			_tracer = tracer;
		}

		public string? UriString { get; set; }

		public async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
		{
			using var scope = _tracer?.StartSpan();

			Guard.Argument(() => UriString!).NotNull().NotEmpty().NotWhiteSpace().StartsWith("http");

			scope?.Span.Log(nameof(UriString), UriString);

			var uri = new Uri(UriString, UriKind.Absolute);

			try
			{
				await _webClient.VisitLinkAsync(uri);
			}
#pragma warning disable CA1031 // Do not catch general exception types
			catch { }
#pragma warning restore CA1031 // Do not catch general exception types

			return ExecutionResult.Next();
		}
	}
}
