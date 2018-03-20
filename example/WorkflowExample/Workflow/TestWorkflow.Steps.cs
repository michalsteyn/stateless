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
                .TransitionOn<UserEvents, ScanBoardPassActivity>(_userEventsTrigger, userEvent => userEvent == UserEvents.Yes, "Yes");

            Configure<ScanBoardPassActivity>()
                .Transition<ScanBoardPassActivity, StartDomesticWorkflowActivity>(scan => scan.IsDomestic, "Domestic Passenger")
                .Transition<ScanBoardPassActivity, StartIntWorkflowActivity>(scan => !scan.IsDomestic, "International Passenger")
                .Transition<ScanBoardPassActivity, GoodbyeScreenActivity>(scan => !scan.HasValidBoardPass, "Invalid BoardPass")
                .TransitionOn<UserEvents, GoodbyeScreenActivity>(_userEventsTrigger, userEvent => userEvent == UserEvents.Cancel, "Cancel");

            Configure<StartIntWorkflowActivity>()
                .TransitionOn<UserEvents, CompletingBookingActivity>(_userEventsTrigger, userEvent => userEvent == UserEvents.Yes, "Yes")
                .TransitionOn<UserEvents, GoodbyeScreenActivity>(_userEventsTrigger, userEvent => userEvent != UserEvents.Yes, "Cancel");

            Configure<StartDomesticWorkflowActivity>()
                .TransitionOn<UserEvents, CompletingBookingActivity>(_userEventsTrigger, userEvent => userEvent == UserEvents.Yes, "Yes")
                .TransitionOn<UserEvents, GoodbyeScreenActivity>(_userEventsTrigger, userEvent => userEvent != UserEvents.Yes, "Cancel");

            Configure<CompletingBookingActivity>()
                .Transition<GoodbyeScreenActivity>();

            Configure<GoodbyeScreenActivity>()
                .Transition<WelcomeScreenActivity>();
        }
    }
}
