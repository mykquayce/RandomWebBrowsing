using Dawn;
using Helpers.Tracing;
using System.Threading.Tasks;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace RandomWebBrowsing.Steps
{
	public class StartParentTraceStep : IStepBody
	{
		private readonly OpenTracing.ITracer _tracer;

		public StartParentTraceStep(
			OpenTracing.ITracer tracer)
		{
			_tracer = Guard.Argument(() => tracer).NotNull().Value;
		}

		public Task<ExecutionResult> RunAsync(IStepExecutionContext context)
		{
			var scope = _tracer
				.BuildDefaultSpan()
				.StartActive(finishSpanOnDispose: true);

			Models.Ioc.Scope = scope;

			return Task.FromResult(ExecutionResult.Next());
		}
	}
}
