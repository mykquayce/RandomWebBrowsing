using Dawn;
using System.Threading.Tasks;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace RandomWebBrowsing.Steps
{
	public class StopParentTraceStep : IStepBody
	{
		private readonly OpenTracing.IScope _scope;

		public StopParentTraceStep(
			OpenTracing.IScope scope)
		{
			_scope = Guard.Argument(() => scope).NotNull().Value;
		}

		public Task<ExecutionResult> RunAsync(IStepExecutionContext context)
		{
			_scope.Span.Finish();
			_scope.Dispose();

			return Task.FromResult(ExecutionResult.Next());
		}
	}
}
