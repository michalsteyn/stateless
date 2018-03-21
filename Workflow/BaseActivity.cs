using System.Threading;
using System.Threading.Tasks;
using NLog;
using WorkflowExample;

namespace Stateless.Workflow
{
    public abstract class BaseActivity<TState, TTrigger>
    {
        protected BaseActivity(TState state)
        {
            State = state;
        }

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public bool HasError { get; set; }

        public TState State { get; }

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