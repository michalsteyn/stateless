using WorkflowExample.Workflow;

namespace WorkflowExample.Activities
{
    public abstract class BaseTestActivity : BaseActivity<States, Triggers>
    {
        protected BaseTestActivity(States state) : base(state) {  }
    }
}