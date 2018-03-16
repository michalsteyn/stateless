using System;
using Stateless;

namespace WorkflowExample.Extensions
{
    public static class StateConfigurationExtensions
    {
        public static StateMachine<TState, TTrigger>.StateConfiguration RunActivityAsync<TState, TTrigger>(
            this StateMachine<TState, TTrigger>.StateConfiguration config, BaseActivity<TState, TTrigger> activity,
            string description = null)
        {
            if (!(config.Machine is Workflow<TState, TTrigger> workflow))
                throw new Exception("Only supported on Workflow Engine");

            config.OnEntryAsync(async trigger => await activity.RunAsync(workflow, trigger));
            return config;
        }

        public static StateMachine<TState, TTrigger>.StateConfiguration RunActivityAsync<TState, TTrigger, TActivity>(
            this StateMachine<TState, TTrigger>.StateConfiguration config,
            string description = null)
        {
            if(!(config.Machine is Workflow<TState, TTrigger> workflow))
                throw new Exception("Only supported on Workflow Engine");

            config.OnEntryAsync(async trigger =>
            {
                var activity = workflow.ActivityFactory.GetActivity<TActivity>() as BaseActivity<TState, TTrigger>;
                if(activity == null) throw new Exception($"Error activating Activity: {typeof(TActivity)}");
                await activity.RunAsync(workflow, trigger);                
            }, description);
            
            return config;
        }

        public static StateMachine<TState, TTrigger>.StateConfiguration OnCompletion<TState, TTrigger, TActivity>(
            this StateMachine<TState, TTrigger>.StateConfiguration config, TState newState, Func<TActivity, bool> condition, 
            string description = null) where TActivity : class
        {
            if (!(config.Machine is Workflow<TState, TTrigger> workflow))
                throw new Exception("Only supported on Workflow Engine");

            config.PermitIf(workflow.CompletedActivityTrigger, newState, baseActivity =>
            {
                var a = baseActivity as TActivity;
                return condition(a);

            }, description);
            return config;
        }

        public static StateMachine<TState, TTrigger>.StateConfiguration OnCompletion<TState, TTrigger>(
            this StateMachine<TState, TTrigger>.StateConfiguration config, TState newState,
            string description = null)
        {
            if (!(config.Machine is Workflow<TState, TTrigger> workflow))
                throw new Exception("Only supported on Workflow Engine");

            config.PermitIf(workflow.CompletedActivityTrigger, newState, activity => true, description);
            return config;
        }
    }
}