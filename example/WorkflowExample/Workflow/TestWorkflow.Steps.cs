using WorkflowExample.Activities;
using WorkflowExample.Events;

namespace WorkflowExample.Workflow
{
    public partial class TestWorkflow
    {
        //This could be split into separate partial classes
        protected void InitSteps()
        {
            RunActivityAsync<WelcomeScreenActivity>()
                .RepeatOn(Triggers.Reset)
                .RepeatOn(_userEventsTrigger, userEvent => userEvent != UserEvents.Yes, "Invalid")
                .TransitionOn<UserEvents, ScanBoardPassActivity>(_userEventsTrigger, userEvent => userEvent == UserEvents.Yes, "Yes");

            RunActivityAsync<ScanBoardPassActivity>()
                .Transition<ScanBoardPassActivity, StartDomesticWorkflowActivity>(activity => activity.IsDomestic, "Domestic Passenger")
                .Transition<ScanBoardPassActivity, StartIntWorkflowActivity>(activity => !activity.IsDomestic, "International Passenger")
                .Transition<ScanBoardPassActivity, GoodbyeScreenActivity>(activity => !activity.HasValidBoardPass, "Invalid BoardPass")
                .TransitionOn<UserEvents, GoodbyeScreenActivity>(_userEventsTrigger, userEvent => userEvent == UserEvents.Cancel, "Cancel");

            RunActivityAsync<StartIntWorkflowActivity>()
                .TransitionOn<UserEvents, CompletingBookingActivity>(_userEventsTrigger, userEvent => userEvent == UserEvents.Yes, "Yes")
                .TransitionOn<UserEvents, GoodbyeScreenActivity>(_userEventsTrigger, userEvent => userEvent != UserEvents.Yes, "Cancel");

            RunActivityAsync<StartDomesticWorkflowActivity>()
                .TransitionOn<UserEvents, CompletingBookingActivity>(_userEventsTrigger, userEvent => userEvent == UserEvents.Yes, "Yes")
                .TransitionOn<UserEvents, GoodbyeScreenActivity>(_userEventsTrigger, userEvent => userEvent != UserEvents.Yes, "Cancel");

            RunActivityAsync<CompletingBookingActivity>()
                .Transition(States.Goodbye);

            RunActivityAsync<GoodbyeScreenActivity>()
                .Transition(States.Welcome);
        }
    }
}
