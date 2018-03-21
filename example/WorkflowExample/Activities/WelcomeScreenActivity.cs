using System.Threading;
using System.Threading.Tasks;
using NLog;
using WorkflowExample.Workflow;

namespace WorkflowExample.Activities
{
    public class WelcomeScreenActivity : BaseTestActivity
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        protected override async Task RunImplementationAsync(CancellationToken token)
        {
            Log.Info("Welcome, Please press Yes to scan your boardpass...");
        }

        public WelcomeScreenActivity(TestWorkflow workflow) : base(workflow, States.Welcome) {  }
    }
}