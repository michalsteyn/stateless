using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using Autofac;
using Caliburn.Micro;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using Shields.GraphViz.Components;
using Shields.GraphViz.Models;
using Shields.GraphViz.Services;
using Stateless.Graph;
using WorkflowExample.Activities;
using WorkflowExample.Events;
using WorkflowExample.Service;
using WorkflowExample.StateDiagram;
using WorkflowExample.Workflow;

namespace WorkflowExample
{
    class Program
    {
        static void Main()
        {
            var container = BuildIoCContainer();
            var eventBus = container.Resolve<IEventAggregator>();
            var logger = container.Resolve<ILogger<TestWorkflow>>();
            try
            {
                using (var scope = container.BeginLifetimeScope())
                {
                    var workflow = scope.Resolve<TestWorkflow>();
                    WriteWorkflowDiagramToFile(workflow, logger, true);

                    logger.LogDebug("Test Workflow Created");
                    workflow.FireAndForget(Triggers.Reset);

                    string userInput;
                    do
                    {
                        logger.LogInformation("Enter a User Input: Yes, No, Cancel, [Exit to terminate]");
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
                logger.LogError(ex, "Exception: " + ex.Message);
                logger.LogInformation("Press any key to continue...");
                Console.ReadKey(true);
            }
        }

        private static IContainer BuildIoCContainer()
        {
            var builder = new ContainerBuilder();

            var logFactory = new LoggerFactory()
                .AddNLog(new NLogProviderOptions
                {
                    CaptureMessageProperties = true,
                    CaptureMessageTemplates = true
                });

            var logger = logFactory.CreateLogger<TestWorkflow>();
            builder.RegisterInstance(logger).As<ILogger<TestWorkflow>>();

            builder.RegisterType<TestWorkflow>().InstancePerLifetimeScope();

            builder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
                .Where(t => t.BaseType == typeof(BaseTestActivity))
                .AsSelf()
                .InstancePerLifetimeScope();

            builder.RegisterType<BoardPassScanner>();
            builder.RegisterType<EventAggregator>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<ActivityFactory>().SingleInstance();

            return builder.Build();
        }

        protected static void WriteWorkflowDiagramToFile(TestWorkflow workflow, 
            ILogger logger, 
            bool openImage = false,
            string fileName = "graph",
            string graphvizPath = @"C:\ProgramData\chocolatey\bin\")
        {
            try
            {
                var dot = UmlDotGraph.Format(workflow.GetInfo());
                var graph = Graph.Directed.Add(new StringStatement(dot));
                File.WriteAllText($"{fileName}.txt", dot);

                //Requires: choco install graphviz.portable
                var renderer = new Renderer(graphvizPath);
                using (Stream file = File.Create($"{fileName}.png"))
                {
                    renderer.RunAsync(graph, file, RendererLayouts.Dot, RendererFormats.Png, CancellationToken.None)
                        .GetAwaiter()
                        .GetResult();
                }

                if (openImage)
                    Process.Start("explorer", $"{fileName}.png");
            }
            catch (Exception e)
            {
                logger.LogError(e, $"Error Creating State Diagram: {e.Message}");
            }
        }
    }    
}
