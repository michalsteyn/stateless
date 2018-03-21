using System.Threading;
using System.Threading.Tasks;
using NLog;
using Stateless;
using Stateless.Workflow;
using WorkflowExample.Workflow;

namespace WorkflowExample.Activities
{
    public class StartIntWorkflowActivity : BaseTestActivity
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        protected override async Task RunImplementationAsync(Workflow<States, Triggers> workflow,
            StateMachine<States, Triggers>.Transition transition, CancellationToken token)
        {
            Log.Info("International Traveler Detected, press Yes to complete booking");
        }

        public StartIntWorkflowActivity() : base(States.StartIntWorkFlow) {  }
    }
}