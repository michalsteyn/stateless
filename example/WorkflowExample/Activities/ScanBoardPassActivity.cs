using System.Threading;
using System.Threading.Tasks;
using NLog;
using Stateless;
using Stateless.Workflow;
using WorkflowExample.Service;
using WorkflowExample.Workflow;

namespace WorkflowExample.Activities
{
    public class ScanBoardPassActivity : BaseTestActivity
    {
        private readonly BoardPassScanner _boardPassScanner;
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public string BoardPassData { get; set; }

        public bool HasValidBoardPass => !string.IsNullOrEmpty(BoardPassData);

        public bool IsDomestic { get; set; }

        public ScanBoardPassActivity(BoardPassScanner boardPassScanner) : base(States.ScanningBoardPass)
        {
            _boardPassScanner = boardPassScanner;
        }        

        protected override async Task RunImplementationAsync(Workflow<States, Triggers, DataContext> workflow,
            StateMachine<States, Triggers>.Transition transition, CancellationToken token)
        {
            Log.Info("Going to Scan BoardPass... waiting...");

            //This may be a Service, or an embedded WF. In this Example, the BoardPassScanner will also subscribe to the UserEvents.Cancel
            BoardPassData = await _boardPassScanner.ScanBoardPass();
            if(!HasValidBoardPass)
                Log.Warn("Failed to read BoardPass");
            IsDomestic = true;
            Log.Info($"Scanned BoardPass: {BoardPassData}");
        }
    }
}