using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkflowExample.Workflow
{
    public partial class TestWorkflow
    {
        protected override void ErrorHandler(object sender, Exception ex, string message)
        {
            Log.Error(ex, "Error in Test Workflow");
            base.ErrorHandler(sender, ex, message);
            Fire(Triggers.Reset);
        }
    }
}
