using System;
using System.Threading.Tasks;
using Stateless;

namespace WorkflowExample
{
    public abstract class BaseActivity<TState, TTrigger>
    {
        public bool HasError { get; protected set; }

        public async Task RunAsync(Workflow<TState, TTrigger> workflow, StateMachine<TState, TTrigger>.Transition transition)
        {
            try
            {
                //Add tracing here
                Console.WriteLine($"...Starting Activity: State: {transition.Destination}, From State: {transition.Source}, Trigger: {transition.Trigger}");
                await RunImplementationAsync(workflow, transition);
                Console.WriteLine("...Completed Activity");

                //Invoke OnCompletion Trigger
                await workflow.FireCompletedActivityAsync(this);
            }
            catch (Exception e)
            {
                HasError = true;
                Console.WriteLine(e);
                workflow.ReportError(this, e, $"Error running activity: {GetType()}, in State: {transition.Destination}");
            }
        }

        protected abstract Task RunImplementationAsync(Workflow<TState, TTrigger> workflow, StateMachine<TState, TTrigger>.Transition transition);
    }

    public class Workflow<TState, TTrigger> : StateMachine<TState, TTrigger>
    {
        internal TriggerWithParameters<BaseActivity<TState, TTrigger>> CompletedActivityTrigger;
        public IActivityFactory ActivityFactory { get; }
        public TTrigger CompletedTrigger { get; }


        private Workflow(Func<TState> stateAccessor, Action<TState> stateMutator) : base(stateAccessor, stateMutator) { }

        public Workflow(TState initialState, ActivityFactory activityFactory, TTrigger completedTrigger) : base(initialState)
        {
            ActivityFactory = activityFactory;
            CompletedTrigger = completedTrigger;
            CompletedActivityTrigger = SetTriggerParameters<BaseActivity<TState, TTrigger>>(completedTrigger);
        }

        internal async Task FireCompletedActivityAsync(BaseActivity<TState, TTrigger> activity)
        {
            await FireAsync(CompletedActivityTrigger, activity);
        }

        public void FireAndForget(TTrigger trigger)
        {
            Task.Run(()=>
            {
                try
                {
                    FireAsync(trigger);
                }
                catch (Exception e)
                {
                    
                    ErrorHandler(this, e, "Error firing trigger");
                }
            });
        }

        public void FireAndForget<TArg0>(TriggerWithParameters<TArg0> trigger, TArg0 arg0)
        {
            Task.Run(() =>
            {
                try
                {
                    FireAsync(trigger, arg0);
                }
                catch (Exception e)
                {

                    ErrorHandler(this, e, "Error firing trigger");
                }
            });
        }

        protected virtual void ErrorHandler(object sender, Exception ex, string message)
        {
            Console.WriteLine($"Error Thrown by: {sender}, Message: {message}, Exception: {ex}");
        }

        public void ReportError(object sender, Exception ex, string message)
        {
            ErrorHandler(sender, ex, message);
        }        
    }
}