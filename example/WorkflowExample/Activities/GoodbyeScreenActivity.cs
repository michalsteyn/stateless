using System;
using System.Threading.Tasks;
using Stateless;
using WorkflowExample.Workflow;

namespace WorkflowExample.Activities
{
    public class GoodbyeScreenActivity : BaseTestActivity
    {
        protected override async Task RunImplementationAsync(Workflow<States, Triggers> workflow, StateMachine<States, Triggers>.Transition transition)
        {
            Console.WriteLine("Have a good day");
        }
    }

    
}
