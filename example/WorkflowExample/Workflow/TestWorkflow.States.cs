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
            RunActivityAsync<WelcomeScreenActivity>(States.Welcome)
                .RepeatOn(Triggers.Reset)
                .RepeatOn(_userEventsTrigger, userEvent => userEvent != UserEvents.Yes, "Invalid")
                .ThenOn(_userEventsTrigger, States.ScanningBoardPass, userEvent => userEvent == UserEvents.Yes, "Yes");

            RunActivityAsync<ScanBoardPassActivity>(States.ScanningBoardPass)
                .Then<ScanBoardPassActivity>(States.StartDomesticWorkflow, activity => activity.IsDomestic, "Domestic Passenger")
                .Then<ScanBoardPassActivity>(States.StartIntWorkFlow, activity => !activity.IsDomestic, "International Passenger")
                .Then<ScanBoardPassActivity>(States.Goodbye, activity => !activity.HasValidBoardPass, "Invalid BoardPass")
                .ThenOn(_userEventsTrigger, States.Goodbye, userEvent => userEvent == UserEvents.Cancel, "Cancel");

            RunActivityAsync<StartIntWorkflowActivity>(States.StartIntWorkFlow)
                .ThenOn(_userEventsTrigger, States.CompleteBooking, userEvent => userEvent == UserEvents.Yes, "Yes")
                .ThenOn(_userEventsTrigger, States.Goodbye, userEvent => userEvent != UserEvents.Yes, "Cancel");

            RunActivityAsync<StartDomesticWorkflowActivity>(States.StartDomesticWorkflow)
                .ThenOn(_userEventsTrigger, States.CompleteBooking, userEvent => userEvent == UserEvents.Yes, "Yes")
                .ThenOn(_userEventsTrigger, States.Goodbye, userEvent => userEvent != UserEvents.Yes, "Cancel");

            RunActivityAsync<CompletingBookingActivity>(States.CompleteBooking)
                .Then(States.Goodbye);

            RunActivityAsync<GoodbyeScreenActivity>(States.Goodbye)
                .Then(States.Welcome);
        }
    }
}
