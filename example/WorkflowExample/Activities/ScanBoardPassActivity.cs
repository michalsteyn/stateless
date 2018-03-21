using System;
using System.Threading;
using System.Threading.Tasks;
using Caliburn.Micro;
using NLog;
using WorkflowExample.Service;
using WorkflowExample.Workflow;
using LogManager = NLog.LogManager;

namespace WorkflowExample.Activities
{
    public class ScanBoardPassActivity : BaseTestActivity
    {
        private readonly IEventAggregator _eventAggregator;
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private readonly Random _rand = new Random(DateTime.Now.GetHashCode());

        public ScanBoardPassActivity(TestWorkflow workflow, IEventAggregator eventAggregator) : base(workflow, States.ScanningBoardPass)
        {
            _eventAggregator = eventAggregator;
        }        

        protected override async Task RunImplementationAsync(CancellationToken token)
        {
            Log.Info("Going to Scan BoardPass... waiting...");

            //This may be a Service, or an embedded WF. In this Example, the BoardPassScanner will also subscribe to the UserEvents.Cancel
            Data.BoardPassData = await new BoardPassScanner(_eventAggregator).ScanBoardPass();
            if (!Data.HasValidBoardPass)
            {
                Log.Warn("Failed to read BoardPass");
            }

            Data.IsDomestic = _rand.NextDouble() < 0.5;
            Log.Info($"Scanned BoardPass: {Data.BoardPassData}");
        }
    }
}