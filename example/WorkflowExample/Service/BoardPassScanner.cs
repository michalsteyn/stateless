using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Caliburn.Micro;
using NLog;
using WorkflowExample.Events;
using LogManager = Caliburn.Micro.LogManager;

namespace WorkflowExample.Service
{
    public class BoardPassScanner: IHandle<UserEventArgs>
    {
        private static readonly Logger Log = NLog.LogManager.GetCurrentClassLogger();

        private CancellationTokenSource _internalCancellationTokenSource;

        public BoardPassScanner(IEventAggregator eventAggregator)
        {
            eventAggregator.Subscribe(this);
        }

        public void Handle(UserEventArgs message)
        {
            if (_internalCancellationTokenSource != null && message.UserEvent == UserEvents.Cancel)
            {
                _internalCancellationTokenSource.Cancel();
            }
        }

        public async Task<string> ScanBoardPass()
        {
            try
            {
                _internalCancellationTokenSource = new CancellationTokenSource();
                await Task.Delay(5000, _internalCancellationTokenSource.Token);
                return "1234";
            }
            catch (OperationCanceledException)
            {
                Log.Warn("BoardPass Scanning Terminated by User");
                return null;
            }
        }
    }
}
