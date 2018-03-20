using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using Autofac;
using Caliburn.Micro;
using NLog;
using Shields.GraphViz.Components;
using Shields.GraphViz.Models;
using Shields.GraphViz.Services;
using Stateless.Graph;
using WorkflowExample.Activities;
using WorkflowExample.Events;
using WorkflowExample.Service;
using WorkflowExample.StateDiagram;
using WorkflowExample.Workflow;
using LogManager = NLog.LogManager;

namespace WorkflowExample
{
    class Program
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        static void Main()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<TestWorkflow>().InstancePerLifetimeScope();
            builder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
                .Where(t => t.BaseType == typeof(BaseTestActivity))
                .AsSelf()
                .InstancePerLifetimeScope();

            builder.RegisterType<BoardPassScanner>();

            builder.RegisterType<EventAggregator>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<ActivityFactory>().SingleInstance();

            var container = builder.Build();

            //Used for DI of Activities
            var eventBus = container.Resolve<IEventAggregator>();

            try
            {
                using (var scope = container.BeginLifetimeScope())
                {
                    var workflow = scope.Resolve<TestWorkflow>();
                    CreateGraph(workflow, true);

                    Log.Debug("Test Workflow Created");
                    workflow.FireAndForget(Triggers.Reset);

                    string userInput;
                    do
                    {
                        Log.Info("Enter a User Input: Yes, No, Cancel, [Exit to terminate]");
                        userInput = Console.ReadLine();
                        if (Enum.TryParse(userInput, out UserEvents userEventTrigger))
                        {
                            eventBus.PublishOnBackgroundThread(new UserEventArgs(userEventTrigger));
                        }
                    } while (userInput?.ToLower() != "exit");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Exception: " + ex.Message);
                Log.Info("Press any key to continue...");
                Console.ReadKey(true);
            }
        }

        protected static void CreateGraph(TestWorkflow workflow, bool openImage)
        {
            try
            {
                var dot = UmlDotGraph.Format(workflow.GetInfo());
                Graph graph = Graph.Directed.Add(new StringStatement(dot));
                File.WriteAllText("graph.txt", dot);

                //Requires: choco install graphviz.portable
                var renderer = new Renderer(@"C:\ProgramData\chocolatey\bin\");
                using (Stream file = File.Create("graph.png"))
                {
                    renderer.RunAsync(
                        graph, file,
                        RendererLayouts.Dot,
                        RendererFormats.Png,
                        CancellationToken.None).GetAwaiter().GetResult();
                }

                if (openImage)
                    Process.Start("explorer", "graph.png");
            }
            catch (Exception e)
            {
                Log.Error(e, $"Error Creating State Diagram: {e.Message}");
            }
        }
    }    
}
