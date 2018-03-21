using Caliburn.Micro;
using Microsoft.Extensions.Logging;
using Stateless.Workflow;

namespace WorkflowExample.Workflow
{
    public partial class TestWorkflow: Workflow<States, Triggers, DataContext>
    {
        private readonly IEventAggregator _eventAggregator;

        public TestWorkflow(IEventAggregator eventAggregator, ILogger<TestWorkflow> logger) : 
          base(States.Welcome, logger, Triggers.ActivityCompleted)
        {
            _eventAggregator = eventAggregator;
            eventAggregator.Subscribe(this);

            InitTriggers();
            InitSteps();
        }
    }
}
