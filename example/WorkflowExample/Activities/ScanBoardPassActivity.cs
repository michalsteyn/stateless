using System;
using System.Threading.Tasks;
using Stateless;
using WorkflowExample.Workflow;

namespace WorkflowExample.Activities
{
    public class ScanBoardPassActivity : BaseTestActivity
    {
        public string BoardPassData { get; set; }

        public bool IsDomestic { get; set; }

        protected override async Task RunImplementationAsync(Workflow<States, Triggers> workflow, StateMachine<States, Triggers>.Transition transition)
        {
            Console.WriteLine("Going to Scan BoardPass... waiting...");

            //Simulate Asynchronous Event 
            await Task.Delay(2000);

            BoardPassData = "1234";
            IsDomestic = true;
            Console.WriteLine($"Scanned BoardPass: {BoardPassData}");
        }
    }
}