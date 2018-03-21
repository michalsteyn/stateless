using System.Threading;
using System.Threading.Tasks;
using NLog;
using WorkflowExample.Workflow;

namespace WorkflowExample.Activities
{
    public class GoodbyeScreenActivity : BaseTestActivity
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        protected override async Task RunImplementationAsync(CancellationToken token)
        {
            Log.Info("Have a good day");
        }

        public GoodbyeScreenActivity(TestWorkflow workflow) : base(workflow, States.Goodbye) { }
    }
}
