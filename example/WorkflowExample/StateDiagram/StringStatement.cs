using System.Collections.Immutable;
using System.IO;
using Shields.GraphViz.Models;

namespace WorkflowExample.StateDiagram
{
    public class StringStatement : Statement
    {
        readonly string _graph;

        public StringStatement(string dotGraph) : base(ImmutableDictionary<Id, Id>.Empty)
        {
            _graph = dotGraph.Replace("digraph {", "").TrimEnd('}');
        }

        public override void WriteTo(StreamWriter writer, GraphKinds graphKind)
        {
            writer.Write(_graph);
        }
    }    
}