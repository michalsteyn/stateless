namespace WorkflowExample.Workflow
{
    public partial class TestWorkflow: Workflow<States, Triggers>
    {
      public TestWorkflow(ActivityFactory activityFactory) : 
          base(States.Welcome, activityFactory, Triggers.ActivityCompleted)
        {
            InitTriggers();
            InitStates();
        }
    }
}
