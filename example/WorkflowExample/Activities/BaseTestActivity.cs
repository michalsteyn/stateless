using Stateless.Workflow;
using WorkflowExample.Workflow;

namespace WorkflowExample.Activities
{
    public abstract class BaseTestActivity : BaseActivity<States, Triggers, DataContext>
    {
        protected BaseTestActivity(Workflow<States, Triggers, DataContext> workflow, States state) : base(workflow, state) {  }
    }
}