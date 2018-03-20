using Caliburn.Micro;

namespace WorkflowExample.Workflow
{
    public partial class TestWorkflow: Workflow<States, Triggers>
    {
      public TestWorkflow(ActivityFactory activityFactory, IEventAggregator eventAggregator) : 
          base(States.Welcome, activityFactory, Triggers.ActivityCompleted)
        {
            eventAggregator.Subscribe(this);
            InitTriggers();
            InitStates();
        }
    }
}
