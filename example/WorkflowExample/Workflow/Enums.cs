namespace WorkflowExample.Workflow
{
    public enum States
    {
        Welcome,
        ScanningBoardPass,
        StartIntWorkFlow,
        StartDomesticWorkflow,
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
