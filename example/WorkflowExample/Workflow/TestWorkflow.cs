using Caliburn.Micro;
using Microsoft.Extensions.Logging;
using Stateless.Workflow;

namespace WorkflowExample.Workflow
{
    public partial class TestWorkflow: Workflow<States, Triggers, DataContext>
    {
      public TestWorkflow(ActivityFactory activityFactory, IEventAggregator eventAggregator, ILogger<TestWorkflow> logger) : 
          base(States.Welcome, activityFactory, logger, Triggers.ActivityCompleted)
        {
            eventAggregator.Subscribe(this);
            InitTriggers();
            InitSteps();
        }
    }
}
