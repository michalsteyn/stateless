using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using Stateless;
using Stateless.Reflection;

namespace WorkflowExample
{
    public class Workflow<TState, TTrigger>
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        internal StateMachine<TState, TTrigger>.TriggerWithParameters<BaseActivity<TState, TTrigger>> CompletedActivityTrigger;
        public IActivityFactory ActivityFactory { get; }
        public TTrigger CompletedTrigger { get; }
        public BaseActivity<TState, TTrigger> RunningActivity { get; protected set; }
        public CancellationTokenSource CancellationTokenSource { get; protected set; }

        private readonly StateMachine<TState, TTrigger> _stateMachine;
        private readonly IDictionary<TState, StateMachine<TState, TTrigger>.StateConfiguration> _states = new ConcurrentDictionary<TState, StateMachine<TState, TTrigger>.StateConfiguration>();
        private StateMachine<TState, TTrigger>.StateConfiguration _currentStateConfiguration;

        public Workflow(TState initialState, ActivityFactory activityFactory, TTrigger completedTrigger)
        {
            _stateMachine = new StateMachine<TState, TTrigger>(initialState);
            _currentStateConfiguration = ConfigureState(initialState);

            MappedDiagnosticsLogicalContext.Set("WorkflowState", "Hi");
            ActivityFactory = activityFactory;
            CompletedTrigger = completedTrigger;
            CompletedActivityTrigger = _stateMachine.SetTriggerParameters<BaseActivity<TState, TTrigger>>(completedTrigger);
            _stateMachine.OnTransitioned(transition => GlobalDiagnosticsContext.Set("WorkflowState", transition.Destination));
        }

        internal async Task FireCompletedActivityAsync(BaseActivity<TState, TTrigger> activity)
        {
            await _stateMachine.FireAsync(CompletedActivityTrigger, activity);
        }

        public void FireAndForget(TTrigger trigger)
        {
            Task.Run(()=>
            {
                try
                {
                    _stateMachine.FireAsync(trigger);
                }
                catch (Exception e)
                {                    
                    ErrorHandler(this, e, "Error firing trigger");
                }
            });
        }

        public void FireAndForget<TArg0>(StateMachine<TState,TTrigger>.TriggerWithParameters<TArg0> trigger, TArg0 arg0)
        {
            Task.Run(() =>
            {
                try
                {
                    _stateMachine.FireAsync(trigger, arg0);
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

        public async Task RunActivity(BaseActivity<TState, TTrigger> activity, StateMachine<TState, TTrigger>.Transition transition)
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

        public async Task RunActivity<TActivity>(StateMachine<TState,TTrigger>.Transition transition)
        {
            var activity = ActivityFactory.GetActivity<TActivity>() as BaseActivity<TState, TTrigger>;
            await RunActivity(activity, transition);
        }

        public void CancelWorkflow()
        {
            Log.Info("Canceling Workflow");
            CancellationTokenSource?.Cancel();
        }

        public Workflow<TState, TTrigger> Configure(BaseActivity<TState, TTrigger> activity, string description = null)
        {
            ConfigureState(activity.State)
                .OnEntryAsync(async transition => await RunActivity(activity, transition));

            return this;
        }

        public Workflow<TState, TTrigger> Configure<TActivity>(string description = null) where TActivity: BaseActivity<TState, TTrigger>
        {
            var activity = ActivityFactory.GetActivity<TActivity>();

            ConfigureState(activity.State).OnEntryAsync(async transition =>
            {
                await RunActivity<TActivity>(transition);
            }, description);
            return this;
        }

        public Workflow<TState, TTrigger> When<TFrom, TTo>(Func<TFrom, bool> condition,
            string description = null) 
            where TFrom : BaseActivity<TState, TTrigger>
            where TTo: BaseActivity<TState, TTrigger>
        {
            var newState = ActivityFactory.GetActivity<TTo>().State;

            _currentStateConfiguration.PermitIf(CompletedActivityTrigger, newState, baseActivity =>
            {
                var a = baseActivity as TFrom;
                return condition(a);
            }, description);

            return this;
        }

        public Workflow<TState, TTrigger> Then<TActivity>(string description = null) where TActivity: BaseActivity<TState, TTrigger>
        {
            var newState = ActivityFactory.GetActivity<TActivity>().State;
            _currentStateConfiguration.PermitIf(CompletedActivityTrigger, newState, activity => true, description);
            return this;
        }

        public void OnUnhandledTrigger(Action<TState, TTrigger> unhandledTriggerAction) => 
            _stateMachine.OnUnhandledTrigger(unhandledTriggerAction);

        public StateMachine<TState,TTrigger>.TriggerWithParameters<T> SetTriggerParameters<T>(TTrigger trigger) => 
            _stateMachine.SetTriggerParameters<T>(trigger);

        public StateMachineInfo GetInfo() => _stateMachine.GetInfo();

        public Workflow<TState, TTrigger> On(TTrigger trigger, TState newState)
        {
            _currentStateConfiguration.PermitIf(trigger, newState);
            return this;
        }

        public Workflow<TState, TTrigger> On<TArg, TActivity>(StateMachine<TState, TTrigger>.TriggerWithParameters<TArg> trigger, 
            Func<TArg, bool> guard, string guardDescription = null) where TActivity: BaseActivity<TState, TTrigger>
        {
            var newState = ActivityFactory.GetActivity<TActivity>().State;
            _currentStateConfiguration.PermitIf(trigger, newState, guard, guardDescription);
            return this;
        }

        public Workflow<TState, TTrigger> RepeatOn<TArg>(StateMachine<TState, TTrigger>.TriggerWithParameters<TArg> trigger, Func<TArg, bool> guard,
            string guardDescription = null)
        {
            _currentStateConfiguration.PermitReentryIf(trigger, guard, guardDescription);
            return this;
        }

        public Workflow<TState, TTrigger> RepeatOn(TTrigger trigger)
        {
            _currentStateConfiguration.PermitReentryIf(trigger);
            return this;
        }

        private StateMachine<TState, TTrigger>.StateConfiguration ConfigureState(TState state)
        {
            if (_states.ContainsKey(state))
            {
                _currentStateConfiguration = _states[state];
            }
            else
            {
                var config = _stateMachine.Configure(state);
                _states[state] = config;
                _currentStateConfiguration = config;
            }
            return _currentStateConfiguration;
        }
    }
}