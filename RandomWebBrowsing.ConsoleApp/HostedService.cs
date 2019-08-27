using Dawn;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace RandomWebBrowsing.ConsoleApp
{
	public sealed class HostedService : IHostedService, IDisposable
	{
		private Timer? _timer;
		private readonly IWorkflowHost _workflowHost;

		public HostedService(
			IWorkflowHost workflowHost)
		{
			_workflowHost = Guard.Argument(() => workflowHost).NotNull().Value;

			_workflowHost.OnStepError += WorkflowHost_OnStepError;

			_workflowHost.RegisterWorkflow<Workflows.Workflow, Models.PersistenceData>();

			_workflowHost.Start();
		}

		public void Dispose()
		{
			_timer?.Dispose();
		}

		private void WorkflowHost_OnStepError(
			WorkflowInstance workflow,
			WorkflowStep step,
			Exception exception)
		{
#if DEBUG
			System.Diagnostics.Debugger.Break();
#endif

			throw new NotImplementedException();
		}

		public Task StartAsync(CancellationToken cancellationToken)
		{
			_timer = new Timer(DoWork!, state: default, dueTime: TimeSpan.Zero, period: TimeSpan.FromSeconds(1));
			return Task.CompletedTask;
		}

		public Task StopAsync(CancellationToken cancellationToken)
		{
			_timer?.Change(dueTime: Timeout.Infinite, period: 0);
			_workflowHost.Stop();
			return Task.CompletedTask;
		}

		private void DoWork(object state)
		{
			var persistenceData = new Models.PersistenceData();
			_workflowHost.StartWorkflow(workflowId: nameof(Workflows.Workflow), data: persistenceData);
		}
	}
}
