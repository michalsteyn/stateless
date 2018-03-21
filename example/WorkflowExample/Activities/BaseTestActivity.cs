using Stateless.Workflow;
using WorkflowExample.Workflow;

namespace WorkflowExample.Activities
{
    public abstract class BaseTestActivity : BaseActivity<States, Triggers, DataContext>
    {
        protected BaseTestActivity(States state) : base(state) {  }
    }
}