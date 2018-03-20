﻿using Caliburn.Micro;
using NLog;
using Stateless;
using WorkflowExample.Events;
using LogManager = NLog.LogManager;

namespace WorkflowExample.Workflow
{
    public partial class TestWorkflow: IHandle<UserEventArgs>
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private StateMachine<States,Triggers>.TriggerWithParameters<UserEvents> _userEventsTrigger;

        private void InitTriggers()
        {
            OnUnhandledTrigger((states, triggers) =>
            {
                if (triggers != Triggers.ActivityCompleted)
                    Log.Warn($"...Invalid Trigger: {triggers} in State: {states}");
            });

            _userEventsTrigger = SetTriggerParameters<UserEvents>(Triggers.UserEvent);
        }

        public void TriggerUserEvent(UserEvents userEvent)
        {
            if(userEvent == UserEvents.Cancel) CancelWorkflow();
            FireAndForget(_userEventsTrigger, userEvent);
        }

        public void Handle(UserEventArgs message)
        {
            TriggerUserEvent(message.UserEvent);
        }
    }
}
