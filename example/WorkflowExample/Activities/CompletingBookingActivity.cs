using System.Threading;
using System.Threading.Tasks;
using NLog;
using Stateless.Workflow;
using WorkflowExample.Workflow;

namespace WorkflowExample.Activities
{
    public class CompletingBookingActivity : BaseTestActivity
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        protected override async Task RunImplementationAsync(CancellationToken token)
        {
            Log.Info("Completing booking...");
            await Task.Delay(1000);
            Log.Info("Booking Complete!");
        }

        public CompletingBookingActivity(Workflow<States, Triggers, DataContext> workflow) : base(workflow, States.CompleteBooking) {  }
    }
}
