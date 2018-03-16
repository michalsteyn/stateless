using System;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using Stateless;

namespace WorkflowExample
{
    public abstract class BaseActivity<TState, TTrigger>
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public bool HasError { get; set; }

        public bool FireActivityCompletedTrigger { get; set; } = true;

        public async Task RunAsync(Workflow<TState, TTrigger> workflow,
            StateMachine<TState, TTrigger>.Transition transition, CancellationToken token)
        {
            //Add tracing here
            Log.Debug($"Starting Activity: State: {transition.Destination}, From State: {transition.Source}, Trigger: {transition.Trigger}");
            await RunImplementationAsync(workflow, transition, token);
            Log.Debug("Completed Activity");            
        }

        protected abstract Task RunImplementationAsync(Workflow<TState, TTrigger> workflow,
            StateMachine<TState, TTrigger>.Transition transition, CancellationToken token);
    }

    public class Workflow<TState, TTrigger> : StateMachine<TState, TTrigger>
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        internal TriggerWithParameters<BaseActivity<TState, TTrigger>> CompletedActivityTrigger;
        public IActivityFactory ActivityFactory { get; }
        public TTrigger CompletedTrigger { get; }
        public BaseActivity<TState, TTrigger> RunningActivity { get; protected set; }
        public CancellationTokenSource CancellationTokenSource { get; protected set; }


        private Workflow(Func<TState> stateAccessor, Action<TState> stateMutator) : base(stateAccessor, stateMutator) { }

        public Workflow(TState initialState, ActivityFactory activityFactory, TTrigger completedTrigger) : base(initialState)
        {
            MappedDiagnosticsLogicalContext.Set("WorkflowState", "Hi");
            ActivityFactory = activityFactory;
            CompletedTrigger = completedTrigger;
            CompletedActivityTrigger = SetTriggerParameters<BaseActivity<TState, TTrigger>>(completedTrigger);
            OnTransitioned(transition => GlobalDiagnosticsContext.Set("WorkflowState", transition.Destination));
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
            Log.Error(ex, $"Error Thrown by: {sender}, Message: {message}, Exception: {ex}");
        }

        public void ReportError(object sender, Exception ex, string message)
        {
            ErrorHandler(sender, ex, message);
        }

        public async Task RunActivity(BaseActivity<TState, TTrigger> activity, Transition transition)
        {
            try
            {
                CancellationTokenSource = new CancellationTokenSource();
                RunningActivity = activity;
                if (RunningActivity == null) throw new Exception($"Error activating Activity: {activity.GetType()}");

                MappedDiagnosticsLogicalContext.Set("WorkflowActivity", $" [{activity.GetType().Name}]");
                await RunningActivity.RunAsync(this, transition, CancellationTokenSource.Token);

                //Invoke OnCompletion Trigger
                if (RunningActivity.FireActivityCompletedTrigger) await FireCompletedActivityAsync(RunningActivity);
            }
            catch (OperationCanceledException)
            {
                Log.Info("Activity Canceled");
            }
            catch (Exception e)
            {
                if (RunningActivity != null) RunningActivity.HasError = true;
                Log.Error(e);
                ReportError(this, e, $"Error running activity: {GetType()}, in State: {transition.Destination}");
            }
            finally
            {
                RunningActivity = null;
                MappedDiagnosticsLogicalContext.Set("WorkflowActivity", "");
            }
        }

        public async Task RunActivity<TActivity>(Transition transition)
        {
            var activity = ActivityFactory.GetActivity<TActivity>() as BaseActivity<TState, TTrigger>;
            await RunActivity(activity, transition);
        }

        public void CancelWorkflow()
        {
            Log.Info("Canceling Workflow");
            CancellationTokenSource?.Cancel();
        }
    }
}