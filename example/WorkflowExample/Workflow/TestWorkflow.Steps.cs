using WorkflowExample.Activities;
using WorkflowExample.Events;

namespace WorkflowExample.Workflow
{
    public partial class TestWorkflow
    {
        //This could be split into separate partial classes
        protected void InitSteps()
        {
            Configure(new WelcomeScreenActivity(this))
                .RepeatOn(Triggers.Reset)
                .RepeatOn(_userEventsTrigger, userEvent => userEvent != UserEvents.Yes, "Invalid")
                .On(_userEventsTrigger, userEvent => userEvent == UserEvents.Yes, () => new ScanBoardPassActivity(this, _eventAggregator), "Yes");

            Configure(new ScanBoardPassActivity(this, _eventAggregator))
                .When(scan => scan.IsDomestic, () => new StartDomesticWorkflowActivity(this), "Domestic Passenger")
                .When(scan => !scan.IsDomestic, () => new StartIntWorkflowActivity(this), "International Passenger")
                .When(scan => !scan.HasValidBoardPass, () => new GoodbyeScreenActivity(this), "Invalid BoardPass")
                .On(_userEventsTrigger, userEvent => userEvent == UserEvents.Cancel, () => new GoodbyeScreenActivity(this), "Cancel");

            Configure(new StartIntWorkflowActivity(this))
                .On(_userEventsTrigger, userEvent => userEvent == UserEvents.Yes, () => new CompletingBookingActivity(this), "Yes")
                .On(_userEventsTrigger, userEvent => userEvent != UserEvents.Yes, () => new GoodbyeScreenActivity(this), "Cancel");

            Configure(new StartDomesticWorkflowActivity(this))
                .On(_userEventsTrigger, userEvent => userEvent == UserEvents.Yes, () => new CompletingBookingActivity(this), "Yes")
                .On(_userEventsTrigger, userEvent => userEvent != UserEvents.Yes, () => new GoodbyeScreenActivity(this), "Cancel");

            Configure(new CompletingBookingActivity(this))
                .Then(() => new GoodbyeScreenActivity(this));

            Configure(new GoodbyeScreenActivity(this))
                .Then(() => new WelcomeScreenActivity(this));
        }
    }
}
