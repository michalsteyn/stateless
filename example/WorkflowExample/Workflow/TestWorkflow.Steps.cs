using WorkflowExample.Activities;
using WorkflowExample.Events;

namespace WorkflowExample.Workflow
{
    public partial class TestWorkflow
    {
        //This could be split into separate partial classes
        protected void InitSteps()
        {
            Configure<WelcomeScreenActivity>()
                .RepeatOn(Triggers.Reset)
                .RepeatOn(_userEventsTrigger, userEvent => userEvent != UserEvents.Yes, "Invalid")
                .On<UserEvents, ScanBoardPassActivity>(_userEventsTrigger, userEvent => userEvent == UserEvents.Yes, "Yes");

            Configure<ScanBoardPassActivity>()
                .When<ScanBoardPassActivity, StartDomesticWorkflowActivity>(scan => scan.IsDomestic, "Domestic Passenger")
                .When<ScanBoardPassActivity, StartIntWorkflowActivity>(scan => !scan.IsDomestic, "International Passenger")
                .When<ScanBoardPassActivity, GoodbyeScreenActivity>(scan => !scan.HasValidBoardPass, "Invalid BoardPass")
                .On<UserEvents, GoodbyeScreenActivity>(_userEventsTrigger, userEvent => userEvent == UserEvents.Cancel, "Cancel");

            Configure<StartIntWorkflowActivity>()
                .On<UserEvents, CompletingBookingActivity>(_userEventsTrigger, userEvent => userEvent == UserEvents.Yes, "Yes")
                .On<UserEvents, GoodbyeScreenActivity>(_userEventsTrigger, userEvent => userEvent != UserEvents.Yes, "Cancel");

            Configure<StartDomesticWorkflowActivity>()
                .On<UserEvents, CompletingBookingActivity>(_userEventsTrigger, userEvent => userEvent == UserEvents.Yes, "Yes")
                .On<UserEvents, GoodbyeScreenActivity>(_userEventsTrigger, userEvent => userEvent != UserEvents.Yes, "Cancel");

            Configure<CompletingBookingActivity>()
                .Then<GoodbyeScreenActivity>();

            Configure<GoodbyeScreenActivity>()
                .Then<WelcomeScreenActivity>();
        }
    }
}
