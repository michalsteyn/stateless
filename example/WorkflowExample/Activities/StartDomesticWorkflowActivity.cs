using System.Threading;
using System.Threading.Tasks;
using NLog;
using WorkflowExample.Workflow;

namespace WorkflowExample.Activities
{
    public class StartDomesticWorkflowActivity : BaseTestActivity
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        protected override async Task RunImplementationAsync(CancellationToken token)
        {
            Log.Info("Domestic Traveler Detected, press Yes to complete booking");
        }

        public StartDomesticWorkflowActivity(TestWorkflow workflow) : base(workflow, States.StartDomesticWorkflow) { }

    }
}