namespace WorkflowExample.Workflow
{
    public enum States
    {
        Welcome,
        ScanningBoardPass,
        StartIntWorkFlow,
        StartDomesticWorkflow,
        CompleteBooking,
        Goodbye
    }

    public enum Triggers
    {
        Reset,
        UserEvent,
        BoardPassScanned,
        ActivityCompleted
    }

    public enum UserEvents
    {
        Yes,
        No,
        Cancel
    }
}
