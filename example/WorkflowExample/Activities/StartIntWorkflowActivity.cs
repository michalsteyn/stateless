using System.Threading;
using System.Threading.Tasks;
using NLog;
using WorkflowExample.Workflow;

namespace WorkflowExample.Activities
{
    public class StartIntWorkflowActivity : BaseTestActivity
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        protected override async Task RunImplementationAsync(CancellationToken token)
        {
            Log.Info("International Traveler Detected, press Yes to complete booking");
        }

        public StartIntWorkflowActivity(TestWorkflow workflow) : base(workflow, States.StartIntWorkFlow) {  }
    }
}