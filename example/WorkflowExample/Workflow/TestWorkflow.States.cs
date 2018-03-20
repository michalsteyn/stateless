using WorkflowExample.Activities;
using WorkflowExample.Events;
using WorkflowExample.Extensions;

namespace WorkflowExample.Workflow
{
    public partial class TestWorkflow
    {
        //This could be split into separate partial classes
        protected void InitStates()
        {
            Configure(States.Welcome)
                .PermitReentryIf(Triggers.Reset)
                .PermitIf(_userEventsTrigger, States.ScanningBoardPass, userEvent => userEvent == UserEvents.Yes, "Yes")
                .PermitReentryIf(_userEventsTrigger, userEvent => userEvent != UserEvents.Yes, "Invalid")
                .RunActivityAsync<States, Triggers, WelcomeScreenActivity>();

            Configure(States.ScanningBoardPass)
                .RunActivityAsync<States, Triggers, ScanBoardPassActivity>("Scan BoardPass")
                .PermitIf(_userEventsTrigger, States.Goodbye, userEvent => userEvent == UserEvents.Cancel, "Cancel")
                .OnCompletion<States, Triggers, ScanBoardPassActivity>(States.StartDomesticWorkflow, activity => activity.IsDomestic, "Domestic Passenger")
                .OnCompletion<States, Triggers, ScanBoardPassActivity>(States.StartIntWorkFlow, activity => !activity.IsDomestic, "International Passenger")
                .OnCompletion<States, Triggers, ScanBoardPassActivity>(States.Goodbye, activity => !activity.HasValidBoardPass, "Invalid BoardPass");

            Configure(States.StartIntWorkFlow)
                .PermitIf(_userEventsTrigger, States.CompleteBooking, userEvent => userEvent == UserEvents.Yes, "Yes")
                .PermitIf(_userEventsTrigger, States.Goodbye, userEvent => userEvent != UserEvents.Yes, "Cancel")
                .RunActivityAsync<States, Triggers, StartIntWorkflowActivity>();

            Configure(States.StartDomesticWorkflow)
                .PermitIf(_userEventsTrigger, States.CompleteBooking, userEvent => userEvent == UserEvents.Yes, "Yes")
                .PermitIf(_userEventsTrigger, States.Goodbye, userEvent => userEvent != UserEvents.Yes, "Cancel")
                .RunActivityAsync<States, Triggers, StartDomesticWorkflowActivity>();

            Configure(States.CompleteBooking)
                .RunActivityAsync<States, Triggers, CompletingBookingActivity>()
                .OnCompletion(States.Goodbye);

            Configure(States.Goodbye)
                .RunActivityAsync<States, Triggers, GoodbyeScreenActivity>()
                .OnCompletion(States.Welcome);
        }
    }
}
