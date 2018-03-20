
namespace WorkflowExample.Events
{
    public enum UserEvents
    {
        Yes,
        No,
        Cancel
    }

    public class UserEventArgs
    {
        public UserEvents UserEvent { get; set; }

        public UserEventArgs()
        {
        }

        public UserEventArgs(UserEvents userEvent)
        {
            UserEvent = userEvent;
        }
    }
}
