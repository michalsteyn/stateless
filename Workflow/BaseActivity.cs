using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Stateless.Workflow
{
    public abstract class BaseActivity<TState, TTrigger, TData> where TData : new()
    {
        protected BaseActivity(Workflow<TState, TTrigger, TData> workflow, TState state)
        {
            Data = workflow.Data;
            State = state;
        }

        public TData Data { get; }

        public bool HasError { get; set; }

        public TState State { get; }

        public string Name => Regex.Replace(GetType().Name, "Activity$", "");

        public bool FireActivityCompletedTrigger { get; set; } = true;

        public async Task RunAsync(CancellationToken token)
        {
            await RunImplementationAsync(token);
        }

        protected abstract Task RunImplementationAsync(CancellationToken token);
    }
}