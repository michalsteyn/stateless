using System.Threading;
using System.Threading.Tasks;
using NLog;
using Stateless;
using WorkflowExample.Workflow;

namespace WorkflowExample.Activities
{
    public class WelcomeScreenActivity : BaseTestActivity
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        protected override async Task RunImplementationAsync(Workflow<States, Triggers> workflow,
            StateMachine<States, Triggers>.Transition transition, CancellationToken token)
        {
            Log.Info("Welcome, Please press Yes to scan your boardpass...");
        }
    }
}