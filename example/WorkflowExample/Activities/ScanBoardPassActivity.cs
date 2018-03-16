﻿using System.Threading;
using System.Threading.Tasks;
using NLog;
using Stateless;
using WorkflowExample.Workflow;

namespace WorkflowExample.Activities
{
    public class ScanBoardPassActivity : BaseTestActivity
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public string BoardPassData { get; set; }

        public bool IsDomestic { get; set; }

        protected override async Task RunImplementationAsync(Workflow<States, Triggers> workflow,
            StateMachine<States, Triggers>.Transition transition, CancellationToken token)
        {
            Log.Info("Going to Scan BoardPass... waiting...");

            //Simulate Asynchronous Event 
            await Task.Delay(5000, token);

            BoardPassData = "1234";
            IsDomestic = true;
            Log.Info($"Scanned BoardPass: {BoardPassData}");
        }
    }
}