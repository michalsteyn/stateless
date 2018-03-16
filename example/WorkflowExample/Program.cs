using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using NLog;
using Shields.GraphViz.Components;
using Shields.GraphViz.Models;
using Shields.GraphViz.Services;
using Stateless.Graph;
using WorkflowExample.StateDiagram;
using WorkflowExample.Workflow;

namespace WorkflowExample
{
    class Program
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        static void Main()
        {
            //Used for DI of Activities
            var factory = new ActivityFactory();

            try
            {
                var workflow = new TestWorkflow(factory);
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
                        workflow.TriggerUserEvent(userEventTrigger);
                    }
                } while (userInput?.ToLower() != "exit");

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
