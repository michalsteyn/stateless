using System.Threading;
using System.Threading.Tasks;
using NLog;
using Stateless;

namespace WorkflowExample
{
    public abstract class BaseActivity<TState, TTrigger>
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public bool HasError { get; set; }

        public bool FireActivityCompletedTrigger { get; set; } = true;

        public async Task RunAsync(Workflow<TState, TTrigger> workflow,
            StateMachine<TState, TTrigger>.Transition transition, CancellationToken token)
        {
            //Add tracing here
            Log.Debug($"Starting Activity: State: {transition.Destination}, From State: {transition.Source}, Trigger: {transition.Trigger}");
            await RunImplementationAsync(workflow, transition, token);
            Log.Debug("Completed Activity");            
        }

        protected abstract Task RunImplementationAsync(Workflow<TState, TTrigger> workflow,
            StateMachine<TState, TTrigger>.Transition transition, CancellationToken token);
    }
}