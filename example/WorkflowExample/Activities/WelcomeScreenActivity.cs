using System;
using System.Threading.Tasks;
using Stateless;
using WorkflowExample.Workflow;

namespace WorkflowExample.Activities
{
    public class WelcomeScreenActivity : BaseTestActivity
    {
        protected override async Task RunImplementationAsync(Workflow<States, Triggers> workflow, StateMachine<States, Triggers>.Transition transition)
        {
            Console.WriteLine("Welcome, Please press Yes to scan your boardpass...");
        }
    }
}