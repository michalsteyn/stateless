using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Stateless.Workflow
{
    public abstract class BaseActivity<TState, TTrigger>
    {
        protected BaseActivity(TState state)
        {
            State = state;
        }

        public bool HasError { get; set; }

        public TState State { get; }

        public string Name => Regex.Replace(GetType().Name, "Activity$", "");

        public bool FireActivityCompletedTrigger { get; set; } = true;

        public async Task RunAsync(Workflow<TState, TTrigger> workflow, 
            StateMachine<TState, TTrigger>.Transition transition, 
            CancellationToken token)
        {
            await RunImplementationAsync(workflow, transition, token);
        }

        protected abstract Task RunImplementationAsync(Workflow<TState, TTrigger> workflow, 
            StateMachine<TState, TTrigger>.Transition transition, 
            CancellationToken token);
    }
}