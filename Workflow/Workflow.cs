using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Stateless.Reflection;

namespace Stateless.Workflow
{
    public class Workflow<TState, TTrigger, TData> where TData: new()
    {
        private readonly ILogger<Workflow<TState, TTrigger, TData>> _logger;
        internal StateMachine<TState, TTrigger>.TriggerWithParameters<BaseActivity<TState, TTrigger, TData>> CompletedActivityTrigger;
        public TTrigger CompletedTrigger { get; }
        public BaseActivity<TState, TTrigger, TData> RunningActivity { get; protected set; }
        public CancellationTokenSource CancellationTokenSource { get; protected set; }
        public TData Data { get; } = new TData();

        private readonly StateMachine<TState, TTrigger> _stateMachine;
        private readonly IDictionary<TState, StateMachine<TState, TTrigger>.StateConfiguration> _states = new ConcurrentDictionary<TState, StateMachine<TState, TTrigger>.StateConfiguration>();
        private StateMachine<TState, TTrigger>.StateConfiguration _currentStateConfiguration;
        private readonly Guid _id = Guid.NewGuid();

        public Workflow(TState initialState, ILogger<Workflow<TState, TTrigger, TData>> logger, TTrigger completedTrigger)
        {
            _logger = logger;
            Log("Creating Workflow of [<{0},{1}>] with ID [{2}] and initial state [{3}]", typeof(TState), typeof(TTrigger), _id, initialState);

            _stateMachine = new StateMachine<TState, TTrigger>(initialState);
            _currentStateConfiguration = ConfigureState(initialState);

            CompletedTrigger = completedTrigger;
            CompletedActivityTrigger = _stateMachine.SetTriggerParameters<BaseActivity<TState, TTrigger, TData>>(completedTrigger);
            _stateMachine.OnTransitioned(transition => Log("Workflow [{0}] moved to state [{1}]", _id, transition.Destination));
        }

        internal async Task FireCompletedActivityAsync(BaseActivity<TState, TTrigger, TData> activity)
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

        public void ReportError(object sender, Exception ex, string message)
        {
            ErrorHandler(sender, ex, message);
        }

        public async Task RunActivity(BaseActivity<TState, TTrigger, TData> activity)
        {
            try
            {
                CancellationTokenSource = new CancellationTokenSource();
                RunningActivity = activity;
                if (RunningActivity == null)
                    throw new Exception($"Error activating activity [{activity.GetType()}]");

                Log($"Workflow [{_id}] running activity [{activity.Name}]");
                await RunningActivity.RunAsync(CancellationTokenSource.Token);

                // Invoke OnCompletion Trigger
                if (RunningActivity.FireActivityCompletedTrigger)
                    await FireCompletedActivityAsync(RunningActivity);
            }
            catch (OperationCanceledException)
            {
                Log("Activity Canceled");
            }
            catch (Exception e)
            {
                if (RunningActivity != null) RunningActivity.HasError = true;
                LogError(e);
                ReportError(this, e, $"Error running activity [{activity.Name}]");
            }
            finally
            {
                RunningActivity = null;
                Log($"Workflow [{_id}] finished activity [{activity.Name}]");
            }
        }

        public void CancelWorkflow()
        {
            Log("Canceling Workflow");
            CancellationTokenSource?.Cancel();
        }

        public Workflow<TState, TTrigger, TData> Configure(BaseActivity<TState, TTrigger, TData> activity, string description = null)
        {
            ConfigureState(activity.State)
                .OnEntryAsync(async transition => await RunActivity(activity));

            return this;
        }

        public Workflow<TState, TTrigger, TData> Configure<TActivity>(TActivity activity, string description = null) where TActivity: BaseActivity<TState, TTrigger, TData>
        {
            ConfigureState(activity.State).OnEntryAsync(async transition =>
            {
                await RunActivity(activity);
            }, description);
            return this;
        }

        public Workflow<TState, TTrigger, TData> When<TActivity>(Func<TData, bool> condition,
            Func<TActivity> activityFactory,
            string description = null) where TActivity: BaseActivity<TState, TTrigger, TData>
        {
            var activity = activityFactory();
            _currentStateConfiguration.PermitIf(CompletedActivityTrigger, activity.State, baseActivity => condition(activity.Data), description);
            return this;
        }

        public Workflow<TState, TTrigger, TData> Then<TActivity>(Func<TActivity> activityFactory, string description = null) where TActivity: BaseActivity<TState, TTrigger, TData>
        {
            _currentStateConfiguration.PermitIf(CompletedActivityTrigger, activityFactory().State, activity => true, description);
            return this;
        }

        public void OnUnhandledTrigger(Action<TState, TTrigger> unhandledTriggerAction) => 
            _stateMachine.OnUnhandledTrigger(unhandledTriggerAction);

        public StateMachine<TState,TTrigger>.TriggerWithParameters<T> SetTriggerParameters<T>(TTrigger trigger) => 
            _stateMachine.SetTriggerParameters<T>(trigger);

        public StateMachineInfo GetInfo() => _stateMachine.GetInfo();

        public Workflow<TState, TTrigger, TData> On(TTrigger trigger, TState newState)
        {
            _currentStateConfiguration.PermitIf(trigger, newState);
            return this;
        }

        public Workflow<TState, TTrigger, TData> On<TArg, TActivity>(StateMachine<TState, TTrigger>.TriggerWithParameters<TArg> trigger, 
            Func<TArg, bool> guard, 
            Func<TActivity> activityFactory,
            string guardDescription = null) where TActivity: BaseActivity<TState, TTrigger, TData>
        {
            _currentStateConfiguration.PermitIf(trigger, activityFactory().State, guard, guardDescription);
            return this;
        }

        public Workflow<TState, TTrigger, TData> RepeatOn<TArg>(StateMachine<TState, TTrigger>.TriggerWithParameters<TArg> trigger, Func<TArg, bool> guard,
            string guardDescription = null)
        {
            _currentStateConfiguration.PermitReentryIf(trigger, guard, guardDescription);
            return this;
        }

        public Workflow<TState, TTrigger, TData> RepeatOn(TTrigger trigger)
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

        private void Log(string message, params object[] args)
        {
            try
            {
                _logger?.LogDebug(message, args);
            }
            catch (Exception) { }
        }

        private void LogError(Exception e)
        {
            try
            {
                _logger?.LogError(e.ToString());
            }
            catch (Exception) { }
        }

        protected virtual void ErrorHandler(object sender, Exception ex, string message)
        {
            try
            {
                _logger.LogError(ex, $"Error Thrown by: {sender}, Message: {message}, Exception: {ex}");
            }
            catch (Exception) { }
        }

    }
}