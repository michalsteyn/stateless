using System;

namespace WorkflowExample.Workflow
{
    public partial class TestWorkflow
    {
        private TriggerWithParameters<UserEvents> _userEventsTrigger;

        private void InitTriggers()
        {
            OnUnhandledTrigger((states, triggers) =>
            {
                if (triggers == Triggers.ActivityCompleted) return;
                Console.WriteLine($"...Invalid Trigger: {triggers} in State: {states}");
            });

            _userEventsTrigger = SetTriggerParameters<UserEvents>(Triggers.UserEvent);
        }

        public void TriggerUserEvent(UserEvents userEvent)
        {
            FireAndForget(_userEventsTrigger, userEvent);
        }
    }
}
