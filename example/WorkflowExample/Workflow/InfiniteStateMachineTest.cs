//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading;
//using System.Threading.Tasks;
//using Caliburn.Micro;
//using NLog;
//using LogManager = NLog.LogManager;

//namespace WorkflowExample.Workflow
//{
//    public class InfiniteStateMachineTest
//    {

//    }

//    public abstract class BaseStep
//    {
//        protected abstract Task<Type> RunStep(CancellationToken token);

//        protected Type Next<TStep>() where TStep: BaseStep
//        {
//            return typeof(TStep);
//        }
//    }

//    public class WelcomeStep: BaseStep
//    {
//        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

//        protected override async Task<Type> RunStep(CancellationToken token)
//        {
//            Log.Info("Welcome, Please press Yes to scan your boardpass...");
//            return Next<ScanBoardPassStep>();
//        }
//    }

//    public class WaitForUserStep : BaseStep
//    {
//        private static readonly Logger Log = LogManager.GetCurrentClassLogger();        

//        protected override async Task<Type> RunStep(CancellationToken token)
//        {
//            string input = String.Empty;
//            do
//            {
//                input = Console.ReadLine();
//            } while (input?.ToLower() != "yes");

//            return Next<ScanBoardPassStep>();
//        }
//    }

//    public class ScanBoardPassStep : BaseStep
//    {
//        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

//        public string BoardPassData { get; set; }

//        public bool IsDomestic { get; set; }

//        protected override async Task<Type> RunStep(CancellationToken token)
//        {
//            Log.Info("Going to Scan BoardPass... waiting...");

//            //Simulate Asynchronous Event 
//            await Task.Delay(5000, token);

//            BoardPassData = "1234";
//            IsDomestic = true;
//            Log.Info($"Scanned BoardPass: {BoardPassData}");
//        }
//    }


//}
