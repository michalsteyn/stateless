using System.Threading;
using System.Threading.Tasks;
using NLog;
using Stateless;
using WorkflowExample.Workflow;

namespace WorkflowExample.Activities
{
    public class CompletingBookingActivity : BaseTestActivity
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        protected override async Task RunImplementationAsync(Workflow<States, Triggers> workflow,
            StateMachine<States, Triggers>.Transition transition, CancellationToken token)
        {
            Log.Info("Completing booking...");
            await Task.Delay(2000);
            Log.Info("Booking Complete!");
        }

        public CompletingBookingActivity() : base(States.CompleteBooking) {  }
    }
}
