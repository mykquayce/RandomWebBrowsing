using Dawn;
using Helpers.Tracing;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace RandomWebBrowsing.WorkerService
{
	public class Worker : BackgroundService
	{
		private readonly int _intervalMilliseconds;
		private readonly IWorkflowHost _workflowHost;
		private readonly ILogger<Worker> _logger;

		public Worker(
			IWorkflowHost workflowHost,
			IOptions<Config.Settings> options,
			ILogger<Worker> logger)
		{
			_workflowHost = Guard.Argument(() => workflowHost).NotNull().Value;

			Guard.Argument(() => options).NotNull();
			Guard.Argument(() => options.Value).NotNull();
			_intervalMilliseconds = Guard.Argument(() => options.Value.IntervalMilliseconds).Positive().Value!.Value;
			_logger = logger;

			_workflowHost.OnStepError += WorkflowHost_OnStepError;

			_workflowHost.RegisterWorkflow<Workflows.Workflow, Models.PersistenceData>();

			_workflowHost.Start();
		}

		private void WorkflowHost_OnStepError(
			WorkflowInstance workflow,
			WorkflowStep step,
			Exception exception)
		{
#if DEBUG
			System.Diagnostics.Debugger.Break();
#endif

			_logger?.LogError(exception, exception.Message);
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			while (!stoppingToken.IsCancellationRequested)
			{
				var persistenceData = new Models.PersistenceData();
				await _workflowHost.StartWorkflow(workflowId: nameof(Workflows.Workflow), data: persistenceData);

				await Task.Delay(_intervalMilliseconds, stoppingToken);
			}

			_workflowHost.Stop();
		}
	}
}
